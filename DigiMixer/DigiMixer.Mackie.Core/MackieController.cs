using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace DigiMixer.Mackie.Core;

// TODO: Work out thread safety better.

/// <summary>
/// Controller to handle the protocol for a Mackie DL-series mixer.
/// </summary>
public sealed class MackieController : IDisposable
{
    private int nextSeq = 0;
    private readonly OutstandingRequest?[] outstandingRequests;
    private readonly SemaphoreSlim semaphore;
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private TcpClient? tcpClient;
    private CancellationTokenSource? cts;

    // TODO: Think about thread safety. We don't write to the lists after starting, so it's probably okay...
    private readonly List<Func<MackiePacket, CancellationToken, Task<MackiePacketBody?>>> requestHandlers;
    private readonly List<Func<MackiePacket, CancellationToken, Task>> broadcastHandlers;

    public bool Running => tcpClient is not null;

    public MackieController(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        outstandingRequests = new OutstandingRequest[256];
        semaphore = new SemaphoreSlim(1);
        requestHandlers = new();
        broadcastHandlers = new();
    }

    // Note: all the MapCommand implementations just add a general purpose handler.
    // That's not terribly efficient, but it's simple - and we don't expect to have many handlers.

    /// <summary>
    /// Convenience method to register a "handler" which never returns a packet body, and is
    /// only present for side-effects.
    /// </summary>
    public void MapCommandAction(MackieCommand command, Action<MackiePacket> handler) =>
        MapCommand(command, packet => { handler(packet); return (MackiePacketBody?) null; });

    public void MapCommand(MackieCommand command, Func<MackiePacket, byte[]?> handler) =>
        MapCommand(command, packet => handler(packet) is byte[] bytes ? new MackiePacketBody(bytes) : null);

    // Note: all the MapCommand implementations just add a general purpose handler.
    // That's not terribly efficient, but it's simple - and we don't expect to have many handlers.
    public void MapCommand(MackieCommand command, Func<MackiePacket, MackiePacketBody?> handler) =>
        MapCommand(command, (packet, cancellationToken) => Task.FromResult(handler(packet)));

    public void MapCommand(MackieCommand command, Func<MackiePacket, CancellationToken, Task<byte[]?>> handler) =>
        MapCommand(command, async (packet, cancellationToken) =>
            await handler(packet, cancellationToken).ConfigureAwait(false) is byte[] bytes
                ? new MackiePacketBody(bytes)
                : null);

    /// <summary>
    /// Adds a handler for the given command.
    /// </summary>
    /// <param name="command">The command to respond to.</param>
    /// <param name="handler">The handler, which is expected to return the body of a response asynchronously.
    /// If the task returns a null reference, the next matching handler is tried.</param>
    public void MapCommand(MackieCommand command, Func<MackiePacket, CancellationToken, Task<MackiePacketBody?>> handler)
    {
        // This isn't terribly efficient, but it's really simple.
        MapRequest(MaybeHandleRequest);

        Task<MackiePacketBody?> MaybeHandleRequest(MackiePacket packet, CancellationToken token) =>
            packet.Command == command
            ? handler(packet, token)
            : Task.FromResult<MackiePacketBody?>(null);
    }

    /// <summary>
    /// Adds a handler for broadcast packets. All handlers are executed for all broadcast packets.
    /// </summary>
    /// <param name="handler"></param>
    public void MapBroadcast(Func<MackiePacket, CancellationToken, Task> handler)
    {
        CheckState(expectedRunning: false);
        broadcastHandlers.Add(handler);
    }

    public void MapBroadcastAction(Action<MackiePacket> handler)
    {
        CheckState(expectedRunning: false);
        broadcastHandlers.Add((packet, cancellationToken) => { handler(packet); return Task.CompletedTask; });
    }

    /// <summary>
    /// Adds a handler for any request packet.
    /// </summary>
    /// <param name="handler">The handler, which is expected to return the body of a response asynchronously.
    /// If the task returns a null reference, the next matching handler is tried.</param>
    public void MapRequest(Func<MackiePacket, CancellationToken, Task<MackiePacketBody?>> handler)
    {
        CheckState(expectedRunning: false);
        requestHandlers.Add(handler);
    }

    public Task<MackiePacket> SendRequest(MackieCommand command, byte[] body, CancellationToken cancellationToken = default) =>
        SendRequest(command, new MackiePacketBody(body), cancellationToken);

    public async Task<MackiePacket> SendRequest(MackieCommand command, MackiePacketBody body, CancellationToken cancellationToken = default)
    {
        CheckState(true);

        byte seq;

        do
        {
            seq = (byte) Interlocked.Increment(ref nextSeq);
            // We never use a sequence number of 0. We don't require strict ordering between request packets,
            // so we don't need to make the whole acquisition atomic.
        } while (seq == 0);

        var packet = new MackiePacket(seq, MackiePacketType.Request, command, body);
        var outstandingRequest = new OutstandingRequest(seq);
        var oldOutstandingRequest = Interlocked.Exchange(ref outstandingRequests[seq], outstandingRequest);

        // If there was already an outstanding request for this sequence number, cancel it.
        if (oldOutstandingRequest is not null)
        {
            oldOutstandingRequest.CompletionSource.TrySetCanceled();
        }
        await SendPacket(packet, cancellationToken).ConfigureAwait(false);
        return await outstandingRequest.CompletionSource.Task.ConfigureAwait(false);
    }

    private async Task SendPacket(MackiePacket packet, CancellationToken cancellationToken)
    {
        var data = packet.ToByteArray();
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending packet: {packet}", packet);
        }
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await tcpClient!.GetStream().WriteAsync(data, 0, data.Length, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Dispose()
    {
        // TODO: Wait for all outstanding tasks?

        // Cancelling the token should stop everything else (at the end of the receiving loop).
        cts?.Cancel();
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        CheckState(expectedRunning: false);
        tcpClient = new TcpClient { NoDelay = true };
        await tcpClient.ConnectAsync(host, port, cancellationToken);
    }

    public async Task Start()
    {
        if (tcpClient is null)
        {
            throw new InvalidOperationException("Must wait for Connect to complete before calling Start");
        }

        cts = new CancellationTokenSource();
        try
        {
            var stream = tcpClient.GetStream();
            // TODO: Is this enough? In theory I guess it could be 256K...
            byte[] buffer = new byte[65536];

            // The index at which to start reading.
            int position = 0;

            while (!cts.IsCancellationRequested)
            {
                // The index of the start of the next packet. It moves through the buffer as we manage to parse.
                // Before reading data, it's always at the start of the buffer.
                int start = 0;
                var bytesRead = await stream.ReadAsync(buffer, position, buffer.Length - position, cts.Token);
                // The index at the end of where we have valid data.
                int end = position + bytesRead;
                while (MackiePacket.TryParse(buffer, start, end - start) is MackiePacket packet)
                {
                    // TODO: Remember this task somewhere...
                    _ = HandlePacketReceived(packet, cts.Token);
                    start += packet.Length;
                }
                // If we've consumed the whole buffer, reset to the start. (No copying required.)
                if (start == end)
                {
                    position = 0;
                }
                // Otherwise, copy whatever's left.
                else
                {
                    Buffer.BlockCopy(buffer, start, buffer, 0, end - start);
                    position = end - start;
                }
            }
        }
        catch (Exception e) when (cts.IsCancellationRequested)
        {
            // TODO: Work out if this is actually useful.
            logger.LogInformation("Operation aborted due to controller shutdown", e);
        }
        finally
        {
            tcpClient?.Dispose();
            cts?.Dispose();
            cts = null;
            tcpClient = null;
        }

        async Task HandlePacketReceived(MackiePacket packet, CancellationToken cancellationToken)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Received packet: {packet}", packet);
            }
            switch (packet.Type)
            {
                case MackiePacketType.Request:
                    var responseBody = await GetResponseBody(packet, cancellationToken);
                    await SendPacket(packet.CreateResponse(responseBody), cancellationToken).ConfigureAwait(false);
                    break;
                case MackiePacketType.Response:
                    var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[packet.Sequence], null);
                    if (outstandingRequest is null)
                    {
                        logger.LogError($"No outstanding request for sequence number: {packet.Sequence}");
                    }
                    else
                    {
                        outstandingRequest.CompletionSource.TrySetResult(packet);
                    }
                    break;
                case MackiePacketType.Broadcast:
                    foreach (var handler in broadcastHandlers)
                    {
                        await handler(packet, cancellationToken).ConfigureAwait(false);
                    }
                    break;
                default:
                    logger.LogError($"Unhandled packet type: {packet.Type}");
                    break;
            }
        }

        async Task<MackiePacketBody> GetResponseBody(MackiePacket request, CancellationToken cancellationToken)
        {
            foreach (var handler in requestHandlers)
            {
                var body = await handler(request, cancellationToken).ConfigureAwait(false);
                if (body is not null)
                {
                    return body;
                }
            }
            return MackiePacketBody.Empty;
        }
    }

    private void CheckState(bool expectedRunning, [CallerMemberName] string caller = "")
    {
        // TODO: Thread safety of this check...
        if (Running != expectedRunning)
        {
            throw new InvalidOperationException($"{caller} cannot be called when the controller is {(Running ? "running" : "stopped")}");
        }
    }

    private class OutstandingRequest
    {
        internal byte SequenceNumber { get; }
        internal TaskCompletionSource<MackiePacket> CompletionSource { get; }

        internal OutstandingRequest(byte sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
            CompletionSource = new TaskCompletionSource<MackiePacket>();
        }
    }
}

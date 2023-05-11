using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DigiMixer.Mackie.Core;

/// <summary>
/// Controller to handle the protocol for a Mackie DL-series mixer.
/// </summary>
public sealed class MackieController : TcpControllerBase
{
    private int nextSeq = 0;
    private readonly OutstandingRequest?[] outstandingRequests;

    // TODO: Think about thread safety. We don't write to the lists after starting, so it's probably okay...
    private readonly List<Func<MackiePacket, CancellationToken, Task<MackiePacketBody?>>> requestHandlers;
    private readonly List<Func<MackiePacket, CancellationToken, Task>> broadcastHandlers;

    private readonly MessageProcessor<MackiePacket> processor;

    public event EventHandler<MackiePacket>? PacketSent;
    public event EventHandler<MackiePacket>? PacketReceived;

    public MackieController(ILogger logger, string host, int port) : base(logger, host, port)
    {
        outstandingRequests = new OutstandingRequest[256];
        requestHandlers = new();
        broadcastHandlers = new();
        processor = new MessageProcessor<MackiePacket>(
            MackiePacket.TryParse,
            packet => packet.Length,
            HandlePacket,
            65536);
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
        CheckState(ControllerStatus.NotConnected);
        broadcastHandlers.Add(handler);
    }

    public void MapBroadcastAction(Action<MackiePacket> handler)
    {
        CheckState(ControllerStatus.NotConnected);
        broadcastHandlers.Add((packet, cancellationToken) => { handler(packet); return Task.CompletedTask; });
    }

    /// <summary>
    /// Adds a handler for any request packet.
    /// </summary>
    /// <param name="handler">The handler, which is expected to return the body of a response asynchronously.
    /// If the task returns a null reference, the next matching handler is tried.</param>
    public void MapRequest(Func<MackiePacket, CancellationToken, Task<MackiePacketBody?>> handler)
    {
        CheckState(ControllerStatus.NotConnected);
        requestHandlers.Add(handler);
    }

    public Task<MackiePacket> SendRequest(MackieCommand command, byte[] body, CancellationToken cancellationToken = default) =>
        SendRequest(command, new MackiePacketBody(body), cancellationToken);

    public async Task<MackiePacket> SendRequest(MackieCommand command, MackiePacketBody body, CancellationToken cancellationToken = default)
    {
        CheckState(ControllerStatus.Running);

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
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending packet: {packet}", packet);
        }
        await Send(data, cancellationToken);
        PacketSent?.Invoke(this, packet);
    }

    private void HandlePacket(MackiePacket packet)
    {
        HandlePacketReceived(packet, CancellationToken)
            .ContinueWith(t => Logger.LogError(t.Exception, "Error processing packet"), TaskContinuationOptions.NotOnRanToCompletion);

        async Task HandlePacketReceived(MackiePacket packet, CancellationToken cancellationToken)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace("Received packet: {packet}", packet);
            }
            PacketReceived?.Invoke(this, packet);
            switch (packet.Type)
            {
                case MackiePacketType.Request:
                    var responseBody = await GetResponseBody(packet, cancellationToken);
                    await SendPacket(packet.CreateResponse(responseBody), cancellationToken).ConfigureAwait(false);
                    break;
                case MackiePacketType.Response:
                    {
                        var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[packet.Sequence], null);
                        if (outstandingRequest is null)
                        {
                            Logger.LogError($"No outstanding request for sequence number: {packet.Sequence}");
                        }
                        else
                        {
                            outstandingRequest.CompletionSource.TrySetResult(packet);
                        }
                    }
                    break;
                case MackiePacketType.Broadcast:
                    foreach (var handler in broadcastHandlers)
                    {
                        await handler(packet, cancellationToken).ConfigureAwait(false);
                    }
                    break;
                case MackiePacketType.Error:
                    {
                        var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[packet.Sequence], null);
                        if (outstandingRequest is null)
                        {
                            Logger.LogError($"No outstanding request for sequence number: {packet.Sequence} which received an error response");
                        }
                        else
                        {
                            outstandingRequest.CompletionSource.TrySetException(new MackieResponseException(packet));
                        }
                    }
                    break;
                default:
                    Logger.LogError($"Unhandled packet type: {packet.Type}");
                    {
                        var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[packet.Sequence], null);
                        if (outstandingRequest is not null)
                        {
                            outstandingRequest.CompletionSource.TrySetException(new MackieResponseException(packet));
                        }
                    }
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

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void CheckState(ControllerStatus expectedStatus, [CallerMemberName] string caller = "")
    {
        if (ControllerStatus != expectedStatus)
        {
            throw new InvalidOperationException($"{caller} cannot be called when the controller status is {ControllerStatus}");
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

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
    private readonly List<Func<MackieMessage, CancellationToken, Task<MackieMessageBody?>>> requestHandlers;
    private readonly List<Func<MackieMessage, CancellationToken, Task>> broadcastHandlers;

    private readonly MessageProcessor<MackieMessage> processor;

    public event EventHandler<MackieMessage>? MessageSent;
    public event EventHandler<MackieMessage>? MessageReceived;

    public MackieController(ILogger logger, string host, int port) : base(logger, host, port)
    {
        outstandingRequests = new OutstandingRequest[256];
        requestHandlers = new();
        broadcastHandlers = new();
        processor = new MessageProcessor<MackieMessage>(
            MackieMessage.TryParse,
            message => message.Length,
            HandleMessage,
            65536);
    }

    // Note: all the MapCommand implementations just add a general purpose handler.
    // That's not terribly efficient, but it's simple - and we don't expect to have many handlers.

    /// <summary>
    /// Convenience method to register a "handler" which never returns a message body, and is
    /// only present for side-effects.
    /// </summary>
    public void MapCommandAction(MackieCommand command, Action<MackieMessage> handler) =>
        MapCommand(command, message => { handler(message); return (MackieMessageBody?) null; });

    public void MapCommand(MackieCommand command, Func<MackieMessage, byte[]?> handler) =>
        MapCommand(command, message => handler(message) is byte[] bytes ? new MackieMessageBody(bytes) : null);

    // Note: all the MapCommand implementations just add a general purpose handler.
    // That's not terribly efficient, but it's simple - and we don't expect to have many handlers.
    public void MapCommand(MackieCommand command, Func<MackieMessage, MackieMessageBody?> handler) =>
        MapCommand(command, (message, cancellationToken) => Task.FromResult(handler(message)));

    public void MapCommand(MackieCommand command, Func<MackieMessage, CancellationToken, Task<byte[]?>> handler) =>
        MapCommand(command, async (message, cancellationToken) =>
            await handler(message, cancellationToken).ConfigureAwait(false) is byte[] bytes
                ? new MackieMessageBody(bytes)
                : null);

    /// <summary>
    /// Adds a handler for the given command.
    /// </summary>
    /// <param name="command">The command to respond to.</param>
    /// <param name="handler">The handler, which is expected to return the body of a response asynchronously.
    /// If the task returns a null reference, the next matching handler is tried.</param>
    public void MapCommand(MackieCommand command, Func<MackieMessage, CancellationToken, Task<MackieMessageBody?>> handler)
    {
        // This isn't terribly efficient, but it's really simple.
        MapRequest(MaybeHandleRequest);

        Task<MackieMessageBody?> MaybeHandleRequest(MackieMessage message, CancellationToken token) =>
            message.Command == command
            ? handler(message, token)
            : Task.FromResult<MackieMessageBody?>(null);
    }

    /// <summary>
    /// Adds a handler for broadcast messages. All handlers are executed for all broadcast messages.
    /// </summary>
    /// <param name="handler">The handler to add to the controller.</param>
    public void MapBroadcast(Func<MackieMessage, CancellationToken, Task> handler)
    {
        CheckState(ControllerStatus.NotConnected);
        broadcastHandlers.Add(handler);
    }

    public void MapBroadcastAction(Action<MackieMessage> handler)
    {
        CheckState(ControllerStatus.NotConnected);
        broadcastHandlers.Add((message, cancellationToken) => { handler(message); return Task.CompletedTask; });
    }

    /// <summary>
    /// Adds a handler for any request message.
    /// </summary>
    /// <param name="handler">The handler, which is expected to return the body of a response asynchronously.
    /// If the task returns a null reference, the next matching handler is tried.</param>
    public void MapRequest(Func<MackieMessage, CancellationToken, Task<MackieMessageBody?>> handler)
    {
        CheckState(ControllerStatus.NotConnected);
        requestHandlers.Add(handler);
    }

    public Task<MackieMessage> SendRequest(MackieCommand command, byte[] body, CancellationToken cancellationToken = default) =>
        SendRequest(command, new MackieMessageBody(body), cancellationToken);

    public async Task<MackieMessage> SendRequest(MackieCommand command, MackieMessageBody body, CancellationToken cancellationToken = default)
    {
        CheckState(ControllerStatus.Running);

        byte seq;

        do
        {
            seq = (byte) Interlocked.Increment(ref nextSeq);
            // We never use a sequence number of 0. We don't require strict ordering between request messages,
            // so we don't need to make the whole acquisition atomic.
        } while (seq == 0);

        var message = new MackieMessage(seq, MackieMessageType.Request, command, body);
        var outstandingRequest = new OutstandingRequest(seq);
        var oldOutstandingRequest = Interlocked.Exchange(ref outstandingRequests[seq], outstandingRequest);

        // If there was already an outstanding request for this sequence number, cancel it.
        if (oldOutstandingRequest is not null)
        {
            oldOutstandingRequest.CompletionSource.TrySetCanceled();
        }
        await SendMessage(message, cancellationToken).ConfigureAwait(false);
        return await outstandingRequest.CompletionSource.Task.ConfigureAwait(false);
    }

    private async Task SendMessage(MackieMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
        MessageSent?.Invoke(this, message);
    }

    private void HandleMessage(MackieMessage message)
    {
        HandleMessageReceived(message, CancellationToken)
            .ContinueWith(t => Logger.LogError(t.Exception, "Error processing message"), TaskContinuationOptions.NotOnRanToCompletion);

        async Task HandleMessageReceived(MackieMessage message, CancellationToken cancellationToken)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace("Received message: {message}", message);
            }
            MessageReceived?.Invoke(this, message);
            switch (message.Type)
            {
                case MackieMessageType.Request:
                    var responseBody = await GetResponseBody(message, cancellationToken);
                    await SendMessage(message.CreateResponse(responseBody), cancellationToken).ConfigureAwait(false);
                    break;
                case MackieMessageType.Response:
                    {
                        var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[message.Sequence], null);
                        if (outstandingRequest is null)
                        {
                            Logger.LogError($"No outstanding request for sequence number: {message.Sequence}");
                        }
                        else
                        {
                            outstandingRequest.CompletionSource.TrySetResult(message);
                        }
                    }
                    break;
                case MackieMessageType.Broadcast:
                    foreach (var handler in broadcastHandlers)
                    {
                        await handler(message, cancellationToken).ConfigureAwait(false);
                    }
                    break;
                case MackieMessageType.Error:
                    {
                        var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[message.Sequence], null);
                        if (outstandingRequest is null)
                        {
                            Logger.LogError($"No outstanding request for sequence number: {message.Sequence} which received an error response");
                        }
                        else
                        {
                            outstandingRequest.CompletionSource.TrySetException(new MackieResponseException(message));
                        }
                    }
                    break;
                default:
                    Logger.LogError($"Unhandled message type: {message.Type}");
                    {
                        var outstandingRequest = Interlocked.Exchange(ref outstandingRequests[message.Sequence], null);
                        if (outstandingRequest is not null)
                        {
                            outstandingRequest.CompletionSource.TrySetException(new MackieResponseException(message));
                        }
                    }
                    break;
            }
        }

        async Task<MackieMessageBody> GetResponseBody(MackieMessage request, CancellationToken cancellationToken)
        {
            foreach (var handler in requestHandlers)
            {
                var body = await handler(request, cancellationToken).ConfigureAwait(false);
                if (body is not null)
                {
                    return body;
                }
            }
            return MackieMessageBody.Empty;
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
        internal TaskCompletionSource<MackieMessage> CompletionSource { get; }

        internal OutstandingRequest(byte sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
            CompletionSource = new TaskCompletionSource<MackieMessage>();
        }
    }
}

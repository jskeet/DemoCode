using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DigiMixer.UCNet.Core;

public class UCNetClient : IDisposable
{
    private readonly string host;
    private readonly int port;
    private readonly ILogger logger;
    private readonly SemaphoreSlim semaphore;

    private TcpClient? tcpClient;
    private CancellationTokenSource? cts;

    public event EventHandler<UCNetMessage>? MessageReceived;

    public UCNetClient(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        semaphore = new SemaphoreSlim(1);
    }

    public async Task Send(UCNetMessage message, CancellationToken cancellationToken)
    {
        if (tcpClient is null)
        {
            throw new InvalidOperationException("Client isn't running.");
        }

        var data = message.ToByteArray();
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending message: {message}", message);
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

    public async Task Connect(CancellationToken cancellationToken)
    {
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
            byte[] buffer = new byte[65542];

            // The index at which to start reading.
            int position = 0;

            while (!cts.IsCancellationRequested)
            {
                // The index of the start of the next message. It moves through the buffer as we manage to parse.
                // Before reading data, it's always at the start of the buffer.
                int start = 0;
                var bytesRead = await stream.ReadAsync(buffer, position, buffer.Length - position, cts.Token);
                // The index at the end of where we have valid data.
                int end = position + bytesRead;
                while (UCNetMessage.TryParse(buffer.AsSpan()[start..end]) is UCNetMessage message)
                {
                    MessageReceived?.Invoke(this, message);
                    start += message.Length;
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
    }

    public void Dispose()
    {
        cts?.Cancel();
        tcpClient?.Dispose();
    }
}

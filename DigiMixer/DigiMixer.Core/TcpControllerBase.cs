using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DigiMixer.Core;

/// <summary>
/// Base class for controllers using TCP.
/// </summary>
public abstract class TcpControllerBase : IDisposable
{
    private readonly CancellationTokenSource cts;
    private readonly TcpClient tcpClient;
    private readonly string host;
    private readonly int port;

    public ControllerStatus ControllerStatus { get; private set; }

    protected CancellationToken CancellationToken => cts.Token;
    protected ILogger Logger { get; }

    protected TcpControllerBase(ILogger logger, string host, int port)
    {
        Logger = logger;
        this.host = host;
        this.port = port;
        cts = new CancellationTokenSource();
        tcpClient = new TcpClient { NoDelay = true };
    }

    protected async Task Send(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        if (ControllerStatus != ControllerStatus.Running)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(Send)}: {ControllerStatus}");
        }
        // TODO: Do we need any sort of protection against concurrent writes?
        // (Even within the same original thread, that could happen. Not sure what NetworkStream does...)
        await tcpClient.GetStream().WriteAsync(data, cancellationToken);
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        if (ControllerStatus != ControllerStatus.NotConnected)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(Connect)}: {ControllerStatus}");
        }
        await tcpClient.ConnectAsync(host, port, cancellationToken);
        ControllerStatus = ControllerStatus.Connected;
    }

    public void Start()
    {
        if (ControllerStatus != ControllerStatus.Connected)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(Start)}: {ControllerStatus}");
        }
        ControllerStatus = ControllerStatus.Running;
        StartAsync().ContinueWith(t =>
        {
            ControllerStatus = ControllerStatus.Faulted;
            Logger.LogCritical(t.Exception, "Unhandled error in TcpControllerBase");
        }, TaskContinuationOptions.NotOnRanToCompletion);
    }

    private async Task StartAsync()
    {
        try
        {
            var stream = tcpClient.GetStream();
            byte[] buffer = new byte[32768];
            while (!cts.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, CancellationToken);
                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("Unexpected TCP stream termination");
                }
                ProcessData(buffer.AsSpan().Slice(0, bytesRead));
            }
        }
        catch (Exception e)
        {
            if (ControllerStatus != ControllerStatus.Disposed)
            {
                Logger.LogError(e, "TCP reading loop failed");
                ControllerStatus = ControllerStatus.Faulted;
            }
        }
    }

    protected abstract void ProcessData(ReadOnlySpan<byte> data);

    public void Dispose()
    {
        ControllerStatus = ControllerStatus.Disposed;
        cts.Cancel();
        tcpClient.Dispose();
    }
}

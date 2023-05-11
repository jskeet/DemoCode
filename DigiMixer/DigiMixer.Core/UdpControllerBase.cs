using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.Core;

public abstract class UdpControllerBase
{
    private readonly CancellationTokenSource cts;
    private readonly UdpClient udpClient;

    public ControllerStatus ControllerStatus { get; private set; }

    protected CancellationToken CancellationToken => cts.Token;
    protected ILogger Logger;

    protected UdpControllerBase(ILogger logger, string host, int remotePort, int? localPort)
    {
        Logger = logger;
        udpClient = localPort is null ? new UdpClient() : new UdpClient(localPort.Value);
        udpClient.Connect(host, remotePort);
        cts = new CancellationTokenSource();
    }

    protected UdpControllerBase(ILogger logger, int localPort)
    {
        Logger = logger;
        udpClient = new UdpClient(localPort);
        cts = new CancellationTokenSource();
    }

    public async Task Send(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        if (ControllerStatus != ControllerStatus.Running || udpClient.Client.RemoteEndPoint is null)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(Send)}: {ControllerStatus}");
        }
        await udpClient.SendAsync(data, cancellationToken);
    }

    public async Task Send(ReadOnlyMemory<byte> data, IPEndPoint endPoint, CancellationToken cancellationToken)
    {
        if (ControllerStatus != ControllerStatus.Running)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(Send)}: {ControllerStatus}");
        }
        await udpClient.SendAsync(data, endPoint, cancellationToken);
    }

    public void Start()
    {
        if (ControllerStatus != ControllerStatus.NotConnected)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(Start)}: {ControllerStatus}");
        }
        ControllerStatus = ControllerStatus.Running;
        StartAsync().ContinueWith(t =>
        {
            ControllerStatus = ControllerStatus.Faulted;
            Logger.LogCritical(t.Exception, "Unhandled error in UdpControllerBase");
        }, TaskContinuationOptions.NotOnRanToCompletion);
    }

    private async Task StartAsync()
    {
        try
        {
            byte[] buffer = new byte[65536];
            while (!cts.IsCancellationRequested)
            {
                int bytesRead = await udpClient.Client.ReceiveAsync(buffer, SocketFlags.None, CancellationToken);
                ProcessData(buffer, bytesRead);
            }
        }
        catch (Exception e)
        {
            if (ControllerStatus != ControllerStatus.Disposed)
            {
                Logger.LogError(e, "UDP reading loop failed");
                ControllerStatus = ControllerStatus.Faulted;
            }
        }
    }

    /// <summary>
    /// Handle a UDP packet as a span. This is only called by the base
    /// class in the default <see cref="ProcessData(byte[], int)"/> implementation.
    /// </summary>
    protected abstract void ProcessData(ReadOnlySpan<byte> data);

    /// <summary>
    /// Handle a UDP packet as a byte array and a length. Where possible,
    /// clients should override <see cref="ProcessData(ReadOnlySpan{byte})"/> and use
    /// that instead.
    /// </summary>
    protected virtual void ProcessData(byte[] data, int length) =>
        ProcessData(data.AsSpan().Slice(0, length));

    public void Dispose()
    {
        ControllerStatus = ControllerStatus.Disposed;
        cts.Cancel();
        udpClient.Dispose();
    }

    protected static int FindAvailableUdpPort()
    {
        // TODO: Check that we really need this. Using port 0 doesn't prompt Windows for
        // appropriate rights, which are needed to actually receive packets - we
        // need a specific port for that.
        using var temporaryClient = new UdpClient();
        temporaryClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        return ((IPEndPoint) temporaryClient.Client.LocalEndPoint!).Port;
    }
}

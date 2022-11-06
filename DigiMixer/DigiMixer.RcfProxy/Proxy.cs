using Microsoft.Extensions.Logging;
using NodaTime;
using OscCore;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.RcfProxy;

internal class Proxy
{
    private static readonly Duration ClientIdlePrune = Duration.FromSeconds(10);
    private static readonly Duration AdminPeriod = Duration.FromSeconds(1);

    private readonly ConcurrentDictionary<IPEndPoint, Instant> clientEndpointToLastPacketTime = new();
    private readonly UdpClient udpClientSendingToMixer;
    private readonly UdpClient udpClientReceivingFromMixer;
    private readonly UdpClient udpClientReceivingFromClients;
    private readonly CancellationTokenSource cts;
    private readonly ILogger logger;
    private readonly IClock clock;

    private long packetsFromMixer = 0;
    private long packetsFromClients = 0;

    private Proxy(string mixerAddress, int mixerPort, int localPortForMixer, int localPortForClients, ILogger logger)
    {
        this.logger = logger;
        udpClientSendingToMixer = new UdpClient();
        udpClientSendingToMixer.Connect(mixerAddress, mixerPort);
        udpClientReceivingFromMixer = new UdpClient(localPortForMixer);
        PreventIcmpUnreachableFromClosingSocket(udpClientReceivingFromMixer);
        udpClientReceivingFromClients = new UdpClient(localPortForClients);
        PreventIcmpUnreachableFromClosingSocket(udpClientReceivingFromClients);
        logger.LogInformation("Connecting to mixer at {address}:{port}", mixerAddress, mixerPort);
        logger.LogInformation("Listening for mixer traffic on port {port}", localPortForMixer);
        logger.LogInformation("Listening for clients on port {port}", localPortForClients);
        cts = new CancellationTokenSource();
        clock = SystemClock.Instance;
    }

    // See https://stackoverflow.com/questions/74327225
    private static void PreventIcmpUnreachableFromClosingSocket(UdpClient client)
    {
        // This is only an issue on Windows, and on other operating systems trying
        // to "fix" it will cause issues.
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        const uint IOC_IN = 0x80000000U;
        const uint IOC_VENDOR = 0x18000000U;
        const int SIO_UDP_CONNRESET = unchecked((int) (IOC_IN | IOC_VENDOR | 12));

        client.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0x00 }, null);
    }

    private async Task ListenAndWait()
    {
        Task clientListeningTask = ListenForClients();
        Task mixerListeningTask = ListenForMixer();
        Task adminTask = StartAdministrativeTasks();

        await Task.WhenAll(clientListeningTask, mixerListeningTask, adminTask);
    }

    private async Task ListenForClients()
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                var result = await udpClientReceivingFromClients.ReceiveAsync(cts.Token);
                var endpoint = result.RemoteEndPoint;
                var buffer = result.Buffer;
                var packet = OscPacket.Read(buffer, 0, buffer.Length);
                Interlocked.Increment(ref packetsFromClients);
                clientEndpointToLastPacketTime[endpoint] = clock.GetCurrentInstant();
                if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
                {
                    logger.LogTrace("Received OSC message from client {endpoint}: {message}", endpoint, message.ToLogFormat());
                }
                await udpClientSendingToMixer.SendAsync(buffer);
                if (packet is OscMessage msg && ShouldReflectMessage(msg))
                {
                    foreach (var otherClientEndpoint in clientEndpointToLastPacketTime.Keys)
                    {
                        if (!otherClientEndpoint.Equals(endpoint))
                        {
                            await udpClientReceivingFromClients.SendAsync(buffer, buffer.Length, otherClientEndpoint);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in client receive loop");
                await Task.Delay(500);
            }
        }

        bool ShouldReflectMessage(OscMessage message) =>
            message.Address.EndsWith("/mt_000") || message.Address.EndsWith("/fd_000");
    }

    private async Task ListenForMixer()
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                var result = await udpClientReceivingFromMixer.ReceiveAsync(cts.Token);
                var buffer = result.Buffer;
                var packet = OscPacket.Read(buffer, 0, buffer.Length);
                Interlocked.Increment(ref packetsFromMixer);
                if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
                {
                    logger.LogTrace("Received OSC message from mixer: {message}", message.ToLogFormat());
                }
                foreach (var endpoint in clientEndpointToLastPacketTime.Keys)
                {
                    await udpClientReceivingFromClients.SendAsync(buffer, buffer.Length, endpoint);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in mixer receive loop");
                await Task.Delay(500);
            }
        }
    }

    private async Task StartAdministrativeTasks()
    {
        while (!cts.IsCancellationRequested)
        {
            var now = clock.GetCurrentInstant();
            var cutoff = now - ClientIdlePrune;
            foreach (var pair in clientEndpointToLastPacketTime)
            {
                if (pair.Value < cutoff)
                {
                    clientEndpointToLastPacketTime.Remove(pair.Key, out _);
                }
            }
            logger.LogDebug("Packets from mixer: {packets}", Interlocked.Read(ref packetsFromMixer));
            logger.LogDebug("Packets from clients: {packets}", Interlocked.Read(ref packetsFromClients));
            logger.LogDebug("Active clients: {clients}", clientEndpointToLastPacketTime.Count);
            await Task.Delay((int) AdminPeriod.TotalMilliseconds);
        }
    }

    internal static Task Start(string mixerAddress, int mixerPort, int localPortForMixer, int localPortForClients, ILogger logger)
    {
        var proxy = new Proxy(mixerAddress, mixerPort, localPortForMixer, localPortForClients, logger);
        return proxy.ListenAndWait();
    }
}

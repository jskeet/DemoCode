using DigiMixer.Core;
using DigiMixer.QuSeries.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.QuSeries;

public class QuMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 51326) =>
        new AutoReceiveMixerApi(new QuMixerApi(logger, host, port));
}

internal class QuMixerApi : IMixerApi
{
    /// <summary>
    /// The output channel ID for the left side of the main output.
    /// </summary>
    internal static ChannelId MainOutputLeft { get; } = ChannelId.Output(100);

    /// <summary>
    /// The output channel ID for the right side of the main output.
    /// </summary>
    internal static ChannelId MainOutputRight { get; } = ChannelId.Output(101);

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;
    private CancellationTokenSource? cts;
    private QuClient? client;
    private Task? clientTask;

    internal QuMixerApi(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
    }

    public async Task Connect()
    {
        Dispose();

        cts = new CancellationTokenSource();
        client = new QuClient(logger, host, port);
        client.ControlPacketReceived += HandleControlPacket;
        client.MeterPacketReceived += HandleMeterPacket;
        clientTask = client.Start();

        await client.SendAsync(QuPackets.RequestControlPackets, cts.Token);
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration()
    {
        var dataPacket = await RequestData(QuPackets.RequestFullData, QuPackets.FullDataType);
        return null!;
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public Task RequestAllData(IReadOnlyList<ChannelId> channelIds) => SendPacket(QuPackets.RequestFullData);

    public async Task SendKeepAlive()
    {
        if (client is not null && cts is not null)
        {
            await client.SendKeepAliveAsync(cts.Token);
        }
    }

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        throw new NotImplementedException();
    }

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        throw new NotImplementedException();
    }

    public Task SetMuted(ChannelId channelId, bool muted)
    {
        throw new NotImplementedException();
    }

    private async Task<QuControlPacket> RequestData(QuControlPacket requestPacket, byte expectedResponseType)
    {
        if (client is null || cts is null)
        {
            throw new InvalidOperationException("Not connected");
        }
        // FIXME
        return null!;
    }

    private async Task SendPacket(QuControlPacket packet)
    {
        if (client is not null)
        {
            await client.SendAsync(packet, cts?.Token ?? default);
        }
    }


    private void HandleControlPacket(object? sender, QuControlPacket e)
    {
    }

    private void HandleMeterPacket(object? sender, QuMeterPacket e)
    {
    }

    public void Dispose()
    {
        client?.Dispose();
        clientTask = null;
    }
}

using IconPlatform.Model;
using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;

namespace DigiMixer.Controls;

internal class IconPlatformMixerController : IAsyncDisposable
{
    private readonly List<ChannelMapping> channelMappings;
    private readonly ILogger logger;
    private readonly PlatformMXController platformController;

    private bool lastConnected = false;

    public IconPlatformMixerController(ILogger logger, DigiMixerViewModel mixerVm, PlatformMXController platformController, int channelsToSkip, bool controlMain)
    {
        (this.logger, this.platformController) = (logger, platformController);
        // TODO: Use the knob for something?
        platformController.ButtonChanged += HandleButtonChanged;
        platformController.FaderMoved += ChangeChannelVolume;
        channelMappings = mixerVm.InputChannels
            .Skip(channelsToSkip)
            .Select((channel, index) => new ChannelMapping(index + 1, channel))
            .Take(8)
            .ToList();

        if (controlMain)
        {
            channelMappings.Add(new ChannelMapping(9, mixerVm.OutputChannels.FirstOrDefault(o => o.ChannelId.IsMainOutput)));
        }

        foreach (var mapping in channelMappings)
        {
            var mixerChannel = mapping.MixerChannel;
            var fader = mapping.Fader;
            var platformChannel = mapping.PlatformChannel;
            // We don't have a 9th mute light for the overall volume
            if (platformChannel < 9)
            {
                Notifications.Subscribe(mixerChannel, nameof(mixerChannel.Muted), (sender, args) =>
                     platformController.SetLight(platformChannel, ButtonType.Mute, mixerChannel.Muted));
            }
            Notifications.Subscribe(fader, nameof(fader.FaderLevel), (sender, args) =>
                 platformController.MoveFader(platformChannel, 1023 * fader.FaderLevel / fader.MaxFaderLevel));
        }
    }

    public async Task<bool> CheckConnectionAsync()
    {
        await platformController.MaybeReconnect();
        bool nowConnected = platformController.Connected;
        if (lastConnected != nowConnected)
        {
            logger.LogDebug($"IconPlatform controller '{{port}}' {(nowConnected ? "connected" : "disconnected")}", platformController.PortName);
            if (nowConnected)
            {
                platformController.ClearText();
                platformController.ClearChannelButtons();
                platformController.WriteChannelDelimiters('|');
                foreach (var mapping in channelMappings)
                {
                    var platformChannel = mapping.PlatformChannel;
                    // Don't try to write the Main channel name - it'll overflow.
                    if (platformChannel > 8)
                    {
                        continue;
                    }
                    var mixerChannel = mapping.MixerChannel;
                    var fader = mapping.Fader;

                    // Show the right text
                    var width = platformController.GetChannelTextWidth(platformChannel);
                    var text = mapping.MixerChannel.ShortName;
                    var padded = text.PadRight(width * 2);
                    platformController.SetChannelText(platformChannel, padded[0..width], padded[width..(width * 2)].Trim());

                    // Set the light and fader position
                    platformController.SetLight(platformChannel, ButtonType.Mute, mixerChannel.Muted);
                    platformController.MoveFader(platformChannel, 1023 * fader.FaderLevel / fader.MaxFaderLevel);
                }
            }
            lastConnected = nowConnected;
        }
        return nowConnected;
    }

    private void HandleButtonChanged(object sender, ButtonEventArgs e)
    {
        if (e.Button == ButtonType.Mute && e.Down)
        {
            var mapping = GetMappingOrNull(e.Channel);
            if (mapping?.MixerChannel is InputChannelViewModel mixerChannel)
            {
                mixerChannel.Muted = !mixerChannel.Muted;
            }
        }
    }

    private void ChangeChannelVolume(object sender, FaderEventArgs e)
    {
        if (GetMappingOrNull(e.Channel)?.Fader is FaderViewModel fader)
        {
            fader.FaderLevel = e.Position * fader.MaxFaderLevel / 1023;
        }
    }

    // TODO: Maybe make this more efficient...
    private ChannelMapping GetMappingOrNull(int platformChannel) =>
        channelMappings.FirstOrDefault(mapping => mapping.PlatformChannel == platformChannel);

    public ValueTask DisposeAsync() => platformController.DisposeAsync();

    /// <summary>
    /// Mapping between an Icon Platform channel (1-8, or 9 for main output) and a mixer input channel.
    /// </summary>
    private class ChannelMapping
    {
        public int PlatformChannel { get; }
        public IChannelViewModelBase MixerChannel { get; }
        public FaderViewModel Fader { get; }

        public ChannelMapping(int platformChannel, InputChannelViewModel mixerChannel) =>
            (PlatformChannel, MixerChannel, Fader) = (platformChannel, mixerChannel, mixerChannel.Faders.FirstOrDefault(fader => fader.OutputChannelId.IsMainOutput));

        public ChannelMapping(int platformChannel, OutputChannelViewModel mixerChannel) =>
            (PlatformChannel, MixerChannel, Fader) = (platformChannel, mixerChannel, mixerChannel.OverallFader);
    }
}

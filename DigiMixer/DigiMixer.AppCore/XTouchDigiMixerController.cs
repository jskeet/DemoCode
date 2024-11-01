using Microsoft.Extensions.Logging;
using XTouchMini.Model;

namespace DigiMixer.AppCore;

internal class XTouchDigiMixerController : IAsyncDisposable
{
    // Main channels
    private readonly List<XTouchDigiMixerControlledChannel> channels;

    // Secondary channels associated with output for the input channels with faders for non-main output.
    private readonly List<XTouchDigiMixerControlledChannel> secondaryChannels;

    private readonly ILogger logger;
    private readonly FaderViewModel mainFader;
    private readonly XTouchMiniMackieController xtouchController;

    private bool lastConnected = false;

    public XTouchDigiMixerController(ILogger logger, DigiMixerViewModel mixerVm, XTouchMiniMackieController xtouchController, int sensitivity, bool mainVolumeEnabled)
    {
        (this.logger, this.xtouchController) = (logger, xtouchController);
        xtouchController.ButtonDown += HandleButtonDown;
        xtouchController.KnobTurned += HandleKnobTurned;
        xtouchController.KnobUp += HandleKnobPushRelease;
        xtouchController.KnobDown += HandleKnobPushRelease;
        channels = mixerVm.InputChannels
            .Select((channel, index) => new XTouchDigiMixerControlledChannel(logger, channel, xtouchController, index + 1, sensitivity, true))
            // First 8 channels are knob+button (fader and mute); last 8 are button-only (mute).
            .Take(16)
            .ToList();

        secondaryChannels = mixerVm.InputChannels
            .Take(8)
            .Select((channel, index) => new XTouchDigiMixerControlledChannel(logger, channel, xtouchController, index + 1, sensitivity, false))
            .ToList();

        if (mainVolumeEnabled)
        {
            mainFader = mixerVm.OverallOutputChannels.FirstOrDefault(channel => channel.ChannelId.IsMainOutput)?.Faders[0];
            if (mainFader is not null)
            {
                xtouchController.FaderMoved += ChangeMainVolume;
            }
        }
    }

    public async Task<bool> CheckConnectionAsync()
    {
        await xtouchController.MaybeReconnect();
        bool nowConnected = xtouchController.Connected;
        if (lastConnected != nowConnected)
        {
            lastConnected = nowConnected;
            logger.LogDebug($"XTouch controller {(nowConnected ? "connected" : "disconnected")}");
            if (nowConnected)
            {
                xtouchController.Reset();
                channels.ForEach(channel => channel.OnReconnection());
            }
        }
        return nowConnected;
    }

    private void HandleKnobPushRelease(object sender, KnobPressEventArgs e)
    {
        GetChannelOrNull(e.Knob)?.SetKnobEnabled(!e.Down);
        GetSecondaryChannelOrNull(e.Knob)?.SetKnobEnabled(e.Down);
    }

    private void HandleKnobTurned(object sender, KnobTurnedEventArgs e)
    {
        GetChannelOrNull(e.Knob)?.HandleKnobTurned(e.Value);
        GetSecondaryChannelOrNull(e.Knob)?.HandleKnobTurned(e.Value);
    }

    private void HandleButtonDown(object sender, ButtonEventArgs e) =>
        GetChannelOrNull(e.Button)?.HandleButtonPressed();

    private void ChangeMainVolume(object sender, FaderEventArgs e) =>
        mainFader.FaderLevel = e.Position * mainFader.MaxFaderLevel / 127;

    private XTouchDigiMixerControlledChannel GetChannelOrNull(int index) =>
        index > 0 && index <= channels.Count ? channels[index - 1] : null;

    private XTouchDigiMixerControlledChannel GetSecondaryChannelOrNull(int index) =>
        secondaryChannels.Count >= index ? secondaryChannels[index - 1] : null;

    public ValueTask DisposeAsync() => xtouchController.DisposeAsync();
}

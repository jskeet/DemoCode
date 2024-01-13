// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;
using XTouchMini.Model;

namespace DigiMixer.Controls;

/// <summary>
/// Connection between an X-Touch Mini controller and a mixer channel.
/// </summary>
internal class XTouchDigiMixerControlledChannel
{
    private readonly int sensitivity;
    private readonly ILogger logger;
    private readonly InputChannelViewModel mixerChannel;
    private readonly FaderViewModel fader;

    // Knob/button index (1-based)
    private readonly int controllerIndex;

    private readonly XTouchMiniMackieController xtouchController;

    /// <summary>
    /// True for channels which are controlled by unpushed knobs and/or buttons;
    /// false for secondary channels which are only controlled by pushed knobs.
    /// </summary>
    private bool primaryChannel;
    private bool knobEnabled = true;

    internal XTouchDigiMixerControlledChannel(
        ILogger logger, InputChannelViewModel mixerChannel, XTouchMiniMackieController xtouchController, int controllerIndex, int sensitivity, bool primaryChannel)
    {
        (this.logger, this.mixerChannel, this.xtouchController, this.controllerIndex, this.sensitivity, this.primaryChannel) =
        (logger, mixerChannel, xtouchController, controllerIndex, sensitivity, primaryChannel);
        fader =
            primaryChannel
            ? mixerChannel.Faders.FirstOrDefault(fader => fader.OutputChannelId.IsMainOutput)
            : mixerChannel.Faders.FirstOrDefault(fader => !fader.OutputChannelId.IsMainOutput);
        knobEnabled = primaryChannel && fader is not null;
        Notifications.Subscribe(mixerChannel, nameof(InputChannelViewModel.Muted), (sender, args) => HandleChannelMutedChanged());
        if (knobEnabled)
        {
            Notifications.Subscribe(fader, nameof(FaderViewModel.FaderLevel), (sender, args) => HandleChannelLevelChanged());
        }
    }

    internal void SetKnobEnabled(bool enabled)
    {
        knobEnabled = enabled;
        HandleChannelLevelChanged();
    }

    public void OnReconnection()
    {
        knobEnabled = primaryChannel;
        HandleChannelLevelChanged();
        HandleChannelMutedChanged();
    }

    private void HandleChannelLevelChanged()
    {
        if (controllerIndex < 9 && knobEnabled)
        {
            xtouchController.SetKnobRingState(
                controllerIndex, KnobRingStyle.Fan, fader.FaderLevel * 11 / fader.MaxFaderLevel);
        }
    }

    private void HandleChannelMutedChanged() =>
        xtouchController.SetButtonLedState(controllerIndex, mixerChannel.Muted ? LedState.Off : LedState.On);

    internal void HandleKnobTurned(int value)
    {
        if (!knobEnabled)
        {
            return;
        }
        int velocity = value >= 0x41 ? -(value - 0x40) : value;
        var currentLevel = fader.FaderLevel;
        var newLevel = Math.Max(Math.Min(currentLevel + velocity * sensitivity, fader.MaxFaderLevel), 0);
        logger.LogTrace("Knob {knob} changing channel '{channel}' fader level from {old} to {new}",
            controllerIndex, mixerChannel.DisplayName, currentLevel, newLevel);
        fader.FaderLevel = newLevel;
    }

    public void HandleButtonPressed() => mixerChannel.Muted = !mixerChannel.Muted;
}

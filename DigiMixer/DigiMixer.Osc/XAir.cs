// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer;
using System;

namespace OscMixerControl;

/// <summary>
/// Factors methods and constants for working with XAir mixers
/// (e.g. XR12, XR16, XR18).
/// </summary>
public static class XAir
{
    internal const string XRemoteAddress = "/xremote";
    internal const string InfoAddress = "/info";
    internal const string InputChannelLevelsMeter = "/meters/1";
    internal const string OutputChannelLevelsMeter = "/meters/5";

    /// <summary>
    /// The output channel ID to use for the main output. This also receives the left-hand
    /// meter reading from the stereo main output.
    /// </summary>
    public static OutputChannelId MainOutput { get; } = new OutputChannelId(100);

    /// <summary>
    /// The output channel ID which receives the right-hand meter reading from the stereo
    /// main output.
    /// </summary>
    public static OutputChannelId MainOutputRightMeter { get; } = new OutputChannelId(101);

    /// <summary>
    /// The input channel ID for the "aux" input. This also receives the left-hand meter reading
    /// from the aux input.
    /// </summary>
    public static InputChannelId AuxInput { get; } = new InputChannelId(17);

    /// <summary>
    /// The output channel ID which receives the right-hand meter reading from the stereo
    /// main output.
    /// </summary>
    public static InputChannelId AuxInputRightMeter { get; } = new InputChannelId(18);

    internal static string GetFaderAddress(InputChannelId inputId, OutputChannelId outputId)
    {
        string prefix = GetInputPrefix(inputId);
        return prefix + (outputId == MainOutput ? "/mix/fader" : $"/mix/{outputId.Value:00}/level");
    }

    internal static string GetFaderAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + "/mix/fader";

    internal static string GetMuteAddress(InputChannelId inputId) => GetInputPrefix(inputId) + "/mix/on";

    internal static string GetMuteAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + "/mix/on";

    internal static string GetNameAddress(InputChannelId inputId) => GetInputPrefix(inputId) + "/config/name";

    internal static string GetNameAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + "/config/name";

    private static string GetInputPrefix(InputChannelId inputId) =>
        inputId == AuxInput ? "/rtn/aux" : $"/ch/{inputId.Value:00}";

    private static string GetOutputPrefix(OutputChannelId outputId) =>
        outputId == MainOutput ? "/lr" : $"/bus/{outputId.Value}";

    /*
    /// <summary>
    /// Creates a main input channel representation for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="index">The index of the channel, in the range 1-18.</param>
    /// <param name="stereo">True for stereo inputs (expecting the index to be the lower one); false for mono</param>
    /// <returns>The new channel.</returns>
    public static Channel CreateInputChannel(Mixer mixer, int index, bool stereo = false)
    {
        var prefix = $"/ch/{index:00}";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/fader",
            InputChannelLevelsMeter,
            meterIndex: index - 1,
            meterIndex2: stereo ? index : default(int?),
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a bus input channel representation for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="busIndex">The index of the bus, in the range 1-6.</param>
    /// <param name="channelIndex">The index of the channel, in the range 1-18.</param>
    /// <param name="stereo">True for stereo inputs (expecting the index to be the lower one); false for mono</param>
    /// <returns>The new channel.</returns>
    public static Channel CreateBusInputChannel(Mixer mixer, int busIndex, int channelIndex, bool stereo = false)
    {
        var prefix = $"/ch/{channelIndex:00}";
        return new Channel(mixer,
            // Channel has one name overall
            $"{prefix}/config/name",
            $"{prefix}/mix/{busIndex:00}/level",
            // This is the raw input, so doesn't depend on bus.
            InputChannelLevelsMeter,
            meterIndex: channelIndex - 1,
            meterIndex2: stereo ? channelIndex : default(int?),
            // This is the main mute; that's still
            // generally what's wanted.
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a stereo aux input channel the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <returns>The new channel.</returns>
    public static Channel CreateAuxInputChannel(Mixer mixer)
    {
        var prefix = $"/rtn/aux";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/fader",
            InputChannelLevelsMeter,
            meterIndex: 16, // Aux is effectively channels 17 and 18 for meters
            meterIndex2: 17,
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a stereo aux input channel the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="busIndex">The index of the bus, in the range 1-6.</param>
    /// <returns>The new channel.</returns>
    public static Channel CreateBusAuxInputChannel(Mixer mixer, int busIndex)
    {
        var prefix = $"/rtn/aux";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/{busIndex:00}/level",
            InputChannelLevelsMeter,
            meterIndex: 16, // Aux is effectively channels 17 and 18 for meters
            meterIndex2: 17,
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a representation of an aux output bus
    /// for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer the output channel belongs to.</param>
    /// <param name="index">The index of the bus, in the range 1-6.</param>
    /// <returns>The output channel.</returns>
    public static Channel CreateAuxOutputChannel(Mixer mixer, int index)
    {
        var prefix = $"/bus/{index}";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/fader",
            OutputChannelLevelsMeter,
            meterIndex: index - 1,
            meterIndex2: null,
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a representation of the main (stereo) output channel
    /// for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer the output channel belongs to.</param>
    /// <returns>The output channel.</returns>
    public static Channel CreateMainOutputChannel(Mixer mixer) =>
        new Channel(mixer,
            "/lr/config/name",
            "/lr/mix/fader",
            OutputChannelLevelsMeter,
            meterIndex: 6,
            meterIndex2: 7,
            "/lr/mix/on");*/
}

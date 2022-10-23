// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer;
using System;

namespace OscMixerControl;

/*
 * X-Air discovery:
 * /node config/chlink - gets stereo links and number of input channels
 * /node config/buslink - gets stereo links and number of output channels
 * Names with /ch/xx/config/name, /rtn/aux/config/name, /bus/xx/config/name, /lr/config/name
 * Does XR-16 have /rtn/aux?
 */

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
    internal const string InputChannelLinkAddress = "/config/chlink";
    internal const string BusChannelLinkAddress = "/config/buslink";

    /// <summary>
    /// The output channel ID for the left side of the main output.
    /// </summary>
    public static ChannelId MainOutputLeft { get; } = new ChannelId(100, false);

    /// <summary>
    /// The output channel ID for the right side of the main output.
    /// </summary>
    public static ChannelId MainOutputRight { get; } = new ChannelId(101, false);

    /// <summary>
    /// The input channel ID for the left "aux" input.
    /// </summary>
    public static ChannelId AuxInputLeft { get; } = new ChannelId(17, input: true);

    /// <summary>
    /// The input channel ID for the right "aux" input.
    /// </summary>
    public static ChannelId AuxInputRight { get; } = new ChannelId(18, input: true);

    internal static string GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        string prefix = GetInputPrefix(inputId);
        return prefix + (outputId == MainOutputLeft ? "/mix/fader" : $"/mix/{outputId.Value:00}/level");
    }

    internal static string GetFaderAddress(ChannelId outputId) => GetOutputPrefix(outputId) + "/mix/fader";

    internal static string GetMuteAddress(ChannelId channelId) => GetPrefix(channelId) + "/mix/on";

    internal static string GetNameAddress(ChannelId inputId) => GetPrefix(inputId) + "/config/name";

    private static string GetInputPrefix(ChannelId inputId) =>
        inputId == AuxInputLeft ? "/rtn/aux" : $"/ch/{inputId.Value:00}";

    private static string GetOutputPrefix(ChannelId outputId) =>
        outputId == MainOutputLeft ? "/lr" : $"/bus/{outputId.Value}";

    private static string GetPrefix(ChannelId channelId) =>
        channelId.IsInput ? GetInputPrefix(channelId) : GetOutputPrefix(channelId);
}

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
}

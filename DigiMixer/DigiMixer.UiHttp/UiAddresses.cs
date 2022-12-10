﻿namespace DigiMixer.UiHttp;

public static class UiAddresses
{
    public static OutputChannelId MainOutput = new OutputChannelId(100);
    public static OutputChannelId MainOutputRightMeter = new OutputChannelId(101);

    internal static string GetFaderAddress(InputChannelId inputId, OutputChannelId outputId)
    {
        string prefix = GetInputPrefix(inputId);
        return prefix + (outputId == MainOutput ? ".mix" : $".aux.{outputId.Value - 1}.value");
    }

    internal static string GetFaderAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + ".mix";

    internal static string GetMuteAddress(InputChannelId inputId) => GetInputPrefix(inputId) + ".mute";

    internal static string GetMuteAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + ".mute";

    internal static string GetNameAddress(InputChannelId inputId) => GetInputPrefix(inputId) + ".name";

    internal static string GetNameAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + ".name";

    private static string GetInputPrefix(InputChannelId inputId) => $"i.{inputId.Value - 1}";

    private static string GetOutputPrefix(OutputChannelId outputId) =>
        outputId == MainOutput ? "m" : $"a.{outputId.Value - 1}";

    internal const string Firmware = "firmware";
    internal const string Model = "model";
}

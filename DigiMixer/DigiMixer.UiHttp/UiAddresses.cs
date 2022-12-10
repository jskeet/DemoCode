﻿using DigiMixer.Core;

namespace DigiMixer.UiHttp;

public static class UiAddresses
{
    public static ChannelId MainOutputLeft { get; } = ChannelId.Output(100);
    public static ChannelId MainOutputRight { get; } = ChannelId.Output(101);

    private const int LineInLeftValue = 100;
    private const int LineInRightValue = 101;
    private const int PlayerLeftValue = 102;
    private const int PlayerRightValue = 103;

    public static ChannelId LineInLeft { get; } = ChannelId.Input(LineInLeftValue);
    public static ChannelId LineInRight { get; } = ChannelId.Input(LineInRightValue);
    public static ChannelId PlayerLeft { get; } = ChannelId.Input(PlayerLeftValue);
    public static ChannelId PlayerRight { get; } = ChannelId.Input(PlayerRightValue);

    internal const string StereoIndex = "stereoIndex";
    
    internal const string AuxPrefix = "a.";
    private const string PlayerLeftPrefix = "p.0";
    private const string PlayerRightPrefix = "p.1";
    private const string LineInLeftPrefix = "l.0";
    private const string LineInRightPrefix = "l.1";

    internal static string GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        string prefix = GetInputPrefix(inputId);
        bool isMainOutput = outputId == MainOutputLeft || outputId == MainOutputRight;
        return prefix + (isMainOutput ? ".mix" : $".aux.{outputId.Value - 1}.value");
    }

    private static string GetOutputPrefix(ChannelId outputId) =>
        outputId == MainOutputLeft || outputId == MainOutputRight ? "m" : $"a.{outputId.Value - 1}";

    internal static string GetFaderAddress(ChannelId outputId) => GetOutputPrefix(outputId) + ".mix";

    internal static string GetMuteAddress(ChannelId channelId) => GetPrefix(channelId) + ".mute";

    internal static string GetNameAddress(ChannelId channelId) => GetPrefix(channelId) + ".name";

    private static string GetPrefix(ChannelId channelId) =>
        channelId.IsInput ? GetInputPrefix(channelId) : GetOutputPrefix(channelId);

    private static string GetInputPrefix(ChannelId inputId) => inputId.Value switch
    {
        LineInLeftValue => LineInLeftPrefix,
        LineInRightValue => LineInRightPrefix,
        PlayerLeftValue => PlayerLeftPrefix,
        PlayerRightValue => PlayerRightPrefix,
        int val => $"i.{val - 1}"
    };

    // TODO: Just use dictionaries for this?
    // TODO: Handle main?
    internal static ChannelId? TryGetChannelId(string address)
    {
        if (address.Length < 3 || address[1] != '.')
        {
            return null;
        }
        Func<int, ChannelId> channelIdFactory = address[0] == 'a' ? ChannelId.Output : ChannelId.Input;
        for (int index = 2; index < address.Length; index++)
        {
            if (address[index] == '.')
            {
                return int.TryParse(address[2..index], out var zeroBasedId) ? channelIdFactory(zeroBasedId + 1) : null;
            }
        }
        return null;
    }

    internal const string Firmware = "firmware";
    internal const string Model = "model";
}

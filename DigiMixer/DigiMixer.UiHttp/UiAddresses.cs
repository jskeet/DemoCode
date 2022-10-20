namespace DigiMixer.UiHttp;

public static class UiAddresses
{
    public static OutputChannelId MainOutput { get; } = new OutputChannelId(100);
    public static OutputChannelId MainOutputRightMeter { get; } = new OutputChannelId(101);

    private const int LineInLeftValue = 100;
    private const int LineInRightValue = 101;
    private const int PlayerLeftValue = 102;
    private const int PlayerRightValue = 103;

    public static InputChannelId LineInLeft { get; } = new InputChannelId(LineInLeftValue);
    public static InputChannelId LineInRight { get; } = new InputChannelId(LineInRightValue);
    public static InputChannelId PlayerLeft { get; } = new InputChannelId(PlayerLeftValue);
    public static InputChannelId PlayerRight { get; } = new InputChannelId(PlayerRightValue);

    internal const string StereoIndex = "stereoIndex";
    
    internal const string AuxPrefix = "a.";
    private const string PlayerLeftPrefix = "p.0";
    private const string PlayerRightPrefix = "p.1";
    private const string LineInLeftPrefix = "l.0";
    private const string LineInRightPrefix = "l.1";

    internal static string GetFaderAddress(InputChannelId inputId, OutputChannelId outputId)
    {
        string prefix = GetInputPrefix(inputId);
        return prefix + (outputId == MainOutput || outputId == MainOutputRightMeter ? ".mix" : $".aux.{outputId.Value - 1}.value");
    }

    internal static string GetFaderAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + ".mix";

    internal static string GetMuteAddress(InputChannelId inputId) => GetInputPrefix(inputId) + ".mute";

    internal static string GetMuteAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + ".mute";

    internal static string GetNameAddress(InputChannelId inputId) => GetInputPrefix(inputId) + ".name";

    internal static string GetNameAddress(OutputChannelId outputId) => GetOutputPrefix(outputId) + ".name";

    private static string GetInputPrefix(InputChannelId inputId) => inputId.Value switch
    {
        LineInLeftValue => LineInLeftPrefix,
        LineInRightValue => LineInRightPrefix,
        PlayerLeftValue => PlayerLeftPrefix,
        PlayerRightValue => PlayerRightPrefix,
        int val => $"i.{val - 1}"
    };

    internal static InputChannelId? TryGetInputChannelId(string address)
    {
        if (address.Length < 3 || address[1] != '.')
        {
            return null;
        }
        for (int index = 2; index < address.Length; index++)
        {
            if (address[index] == '.')
            {
                // TODO: Use spans etc
                string start = address[..index];
                return start switch
                {
                    LineInLeftPrefix => LineInLeft,
                    LineInRightPrefix => LineInRight,
                    PlayerLeftPrefix => PlayerLeft,
                    PlayerRightPrefix => PlayerRight,
                    string st when st[0] == 'i' => int.TryParse(start[2..], out var zeroBasedId) ? new InputChannelId(zeroBasedId + 1) : null,
                    _ => null
                };
            }
        }
        return null;
    }

    internal static OutputChannelId? TryGetAuxOutputChannelId(string address)
    {
        // Note: we allow for more than 10 auxes, even though there may not be any hardware that currently supports it...
        if (address.Length < 3 || address[0] != 'a' || address[1] != '.')
        {
            return null;
        }
        for (int index = 2; index < address.Length; index++)
        {
            if (address[index] == '.')
            {
                return int.TryParse(address[2..index], out var zeroBasedId) ? new OutputChannelId(zeroBasedId + 1) : null;
            }
        }
        return null;
    }

    private static string GetOutputPrefix(OutputChannelId outputId) =>
        outputId == MainOutput || outputId == MainOutputRightMeter ? "m" : $"a.{outputId.Value - 1}";

    internal const string Firmware = "firmware";
    internal const string Model = "model";
}

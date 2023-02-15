using OscCore;

namespace DigiMixer.Osc;

internal static class OscMessageExtensions
{
    internal static string ToLogFormat(this OscMessage message) =>
        message.Count == 0 ? message.Address : $"{message.Address} {string.Join(", ", message.Select(FormatParameter))}";

    private static string FormatParameter(object param) => param switch
    {
        string x => $"\"{x}\"",
        float f => $"{f}F",
        double d => $"{d}D",
        int i => $"{i}I",
        long l => $"{l}L",
        byte b => $"0x{b:X2}V",
        byte[] array => $"(bytes) {Convert.ToBase64String(array)}",
        null => "null",
        _ => param.ToString() ?? "null"
    };
}

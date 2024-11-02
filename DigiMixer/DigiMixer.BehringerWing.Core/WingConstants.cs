using System.Text;

namespace DigiMixer.BehringerWing.Core;

internal static class WingConstants
{
    internal const byte Escape = 0xdf;
    internal const byte EscapedEscape = 0xde;
    internal const byte MinProtocolChannelChange = 0xd0;
    internal const byte MaxProtocolChannelChange = 0xdd;

    internal static Encoding Encoding { get; } = Encoding.ASCII;
}

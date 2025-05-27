using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace DigiMixer.Yamaha.Core;

/// <summary>
/// The type of a message.
/// </summary>
public sealed class YamahaMessageType
{
    public static YamahaMessageType D000 { get; } = new("d000", 2);
    public static YamahaMessageType D010 { get; } = new("d010", 2);
    public static YamahaMessageType D020 { get; } = new("d020", 2);
    public static YamahaMessageType D030 { get; } = new("d030", 2);
    public static YamahaMessageType D040 { get; } = new("d040", 2);
    public static YamahaMessageType DL_A { get; } = new("DL_A", 4);
    public static YamahaMessageType DL_B { get; } = new("DL_B", 4);
    public static YamahaMessageType DL_C { get; } = new("DL_C", 4);
    public static YamahaMessageType DL_D { get; } = new("DL_D", 4);
    public static YamahaMessageType DS_A { get; } = new("DS_A", 4);
    public static YamahaMessageType DS_B { get; } = new("DS_B", 4);
    public static YamahaMessageType DS_C { get; } = new("DS_C", 4);
    public static YamahaMessageType DS_D { get; } = new("DS_D", 4);
    public static YamahaMessageType EEVT { get; } = new("EEVT", 3);
    public static YamahaMessageType MCST { get; } = new("MCST", 1);
    public static YamahaMessageType MFX { get; } = new("MFX", 1);
    public static YamahaMessageType MMIX { get; } = new("MMIX", 1);
    public static YamahaMessageType MPRC { get; } = new("MPRC", 1);
    public static YamahaMessageType MPRO { get; } = new("MPRO", 1);
    public static YamahaMessageType MSCL { get; } = new("MSCL", 1);
    public static YamahaMessageType MSCS { get; } = new("MSCS", 1);
    public static YamahaMessageType MSTS { get; } = new("MSTS", 1);
    public static YamahaMessageType MSUP { get; } = new("MSUP", 1);
    public static YamahaMessageType MVOL { get; } = new("MVOL", 1);

    private static readonly ImmutableList<YamahaMessageType> MessageTypes = [D000, D010, D020, D030, D040, DL_A, DL_B, DL_C, DL_D, DS_A, DS_B, DS_C, DS_D, EEVT, MCST, MFX, MMIX, MPRC, MPRO, MSCL, MSCS, MSTS, MSUP, MVOL];

    /// <summary>
    /// 1-4 character textual representation, always ASCII.
    /// This is used as the first four bytes of the message.
    /// </summary>
    public string Text { get; }

    public byte HeaderByte { get; }

    private readonly uint magicNumber;

    private YamahaMessageType(string text, byte headerByte)
    {
        Text = text;
        var bytes = new byte[4];
        Encoding.ASCII.GetBytes(text, bytes);
        magicNumber = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        HeaderByte = headerByte;
    }

    internal static YamahaMessageType? TryParse(ReadOnlySpan<byte> bytes)
    {
        var magicNumber = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        foreach (var type in MessageTypes)
        {
            if (magicNumber == type.magicNumber)
            {
                return type;
            }
        }
        return null;
    }

    internal void WriteTo(Span<byte> bytes) => BinaryPrimitives.WriteUInt32LittleEndian(bytes, magicNumber);

    public override string ToString() => Text;
}

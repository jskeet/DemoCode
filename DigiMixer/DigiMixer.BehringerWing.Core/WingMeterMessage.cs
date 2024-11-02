using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.BehringerWing.Core;

/// <summary>
/// A message containing meter data, sent via UDP.
/// </summary>
public record WingMeterMessage(uint ReportId, ImmutableList<short> Data)
{
    public ChannelV2Data GetChannelV2(int startOffset, int index) => new(this, startOffset + index * ChannelV2Data.Size);

    public static WingMeterMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            return null;
        }
        var reportId = BinaryPrimitives.ReadUInt32BigEndian(data);
        data = data[4..];

        var array = new short[data.Length / 2];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = BinaryPrimitives.ReadInt16BigEndian(data[(i * 2)..]);
        }
        return new(reportId, [.. array]);
    }

    /// <summary>
    /// A view over a portion of a <see cref="WingMeterMessage"/>.
    /// </summary>
    public readonly struct ChannelV2Data
    {
        // The number of 16-bit entries in this struct.
        public const int Size = 11;

        private readonly WingMeterMessage message;
        private readonly int offset;

        public short InputLeft => message.Data[offset];
        public short InputRight => message.Data[offset + 1];
        public short OutputLeft => message.Data[offset + 2];
        public short OutputRight => message.Data[offset + 3];
        public short GateKey => message.Data[offset + 4];
        public short GateGain => message.Data[offset + 5];
        public short GateLed => message.Data[offset + 6];
        public short DynKey => message.Data[offset + 7];
        public short DynGain => message.Data[offset + 8];
        public short DynState => message.Data[offset + 9];
        public short AutoMixGain => message.Data[offset + 10];

        internal ChannelV2Data(WingMeterMessage message, int offset)
        {
            this.message = message;
            this.offset = offset;
        }
    }
}

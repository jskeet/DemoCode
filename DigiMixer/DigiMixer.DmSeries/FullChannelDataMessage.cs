using DigiMixer.Core;
using DigiMixer.DmSeries.Core;
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.DmSeries;

internal sealed class FullChannelDataMessage
{
    // TODO: Stereo In handling

    private readonly DmBinarySegment data;

    internal FullChannelDataMessage(DmMessage message)
    {
        data = (DmBinarySegment) message.Segments[7];
    }

    private static int GetStartOffset(ChannelId channel) => channel switch
    {
        { IsInput: true, Value: int ch } => (ch - 1) * 0x1cf + 0x6088,
        { IsMainOutput: true, Value: int ch } => (ch - ChannelId.MainOutputLeft.Value) * 0x190 + 0x959e,
        { Value: int ch } => (ch - 1) * 0x19d + 0x881b
    };

    internal string GetChannelName(ChannelId channel)
    {
        int start = GetStartOffset(channel);
        return Encoding.ASCII.GetString(data.Data.Slice(start, 8)).TrimEnd('\0');
    }

    internal FaderLevel GetFaderLevel(ChannelId input, ChannelId output)
    {
        int inputStart = GetStartOffset(input);
        int faderOffset = output switch
        {
            { IsMainOutput: true } => 0x17b,
            { Value: int ch } => (ch - 1) * 5 + 0x143
        };
        var raw = BinaryPrimitives.ReadInt16LittleEndian(data.Data.Slice(inputStart + faderOffset, 2));
        return DmConversions.RawToFaderLevel(raw);
    }

    internal FaderLevel GetFaderLevel(ChannelId output)
    {
        // TODO: Check this...
        int outputStart = GetStartOffset(output);
        int faderOffset = output switch
        {
            { IsMainOutput: true } => 0x13c,
            _ => 0x13e
        };
        var raw = BinaryPrimitives.ReadInt16LittleEndian(data.Data.Slice(outputStart + faderOffset, 2));
        return DmConversions.RawToFaderLevel(raw);
    }

    internal bool IsMuted(ChannelId channel)
    {
        int channelStart = GetStartOffset(channel);
        int muteOffset = channel switch
        {
            { IsInput: true } => 0x17d,
            { IsMainOutput: true } => 0x13e,
            _ => 0x140
        };
        return data.Data[channelStart + muteOffset] == 0;
    }
}

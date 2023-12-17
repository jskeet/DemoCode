using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries;

internal sealed class SingleChannelValueMessage
{
    internal string Description { get; }

    /// <summary>
    /// The new value being represented.
    /// </summary>
    internal short Value { get; }

    /// <summary>
    /// The channel number specified in the first of the values in the segment after the description.
    /// </summary>
    internal int PrimaryChannel { get; }

    /// <summary>
    /// The channel number specified in the second of the values in the segment after the description.
    /// </summary>
    internal int SecondaryChannel { get; }

    // TODO: Handle number-based (instead of string-based) descriptions.
    internal SingleChannelValueMessage(DmMessage message)
    {
        Description = ((DmTextSegment) message.Segments[4]).Text;

        Value = (short) (message.Segments[7] switch
        {
            DmInt32Segment int32 => (long) int32.Values[0],
            DmUInt32Segment uint32 => uint32.Values[0],
            DmUInt16Segment uint16 => uint16.Values[0],
            _ => throw new ArgumentException("Incorrect segments for single channel value message")
        });

        var channelSegment = (DmUInt32Segment) message.Segments[5];
        PrimaryChannel = (int) channelSegment.Values[0];
        SecondaryChannel = (int) channelSegment.Values[1];
    }
}

namespace DigiMixer.DmSeries.Core;

/// <summary>
/// A segment within a <see cref="DmMessage"/>.
/// </summary>
public abstract class DmSegment
{
    /// <summary>
    /// The format of the segment.
    /// </summary>
    public abstract DmSegmentFormat Format { get; }

    /// <summary>
    /// The length of the segment, including the format.
    /// </summary>
    public abstract int Length { get; }

    public abstract void WriteTo(Span<byte> buffer);
}

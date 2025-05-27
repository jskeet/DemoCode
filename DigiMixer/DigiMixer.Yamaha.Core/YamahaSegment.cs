namespace DigiMixer.Yamaha.Core;

/// <summary>
/// A segment within a <see cref="YamahaMessage"/>.
/// </summary>
public abstract class YamahaSegment
{
    internal YamahaSegment()
    {
    }

    /// <summary>
    /// The length of the segment, including the format.
    /// </summary>
    internal abstract int Length { get; }

    internal abstract void WriteTo(Span<byte> buffer);
}

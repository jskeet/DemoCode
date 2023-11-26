namespace DigiMixer.Core;

public sealed class StereoPair
{
    public ChannelId Left { get; }
    public ChannelId Right { get; }
    public bool IsInput { get; }
    public StereoFlags Flags { get; }

    public StereoPair(ChannelId left, ChannelId right, StereoFlags flags)
    {
        if (left == right)
        {
            throw new ArgumentException("A channel cannot be paired with itself");
        }
        if (left.IsInput != right.IsInput)
        {
            throw new ArgumentException("Stereo pairs must be either input or output");
        }
        Left = left;
        Right = right;
        IsInput = left.IsInput;
        Flags = flags;
    }

    /// <summary>
    /// For the common case where "right" is always "left + 1", construct a pair from just the left.
    /// </summary>
    public static StereoPair FromLeft(ChannelId left, StereoFlags flags) =>
        new StereoPair(left, left.WithValue(left.Value + 1), flags);

    public override string ToString() => $"{(IsInput ? "Input" : "Output")}: {Left.Value}/{Right.Value}";
}

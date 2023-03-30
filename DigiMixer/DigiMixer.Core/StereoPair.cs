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

    public override string ToString() => $"{(IsInput ? "Input" : "Output")}: {Left.Value}/{Right.Value}";    
}

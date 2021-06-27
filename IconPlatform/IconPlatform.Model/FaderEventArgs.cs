namespace IconPlatform.Model
{
    public sealed record FaderEventArgs
    {
        public int Channel { get; }

        public int Position { get; }

        public FaderEventArgs(int channel, int position) => (Channel, Position) = (channel, position);
    }
}

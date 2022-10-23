namespace DigiMixer;

// TODO: Find a better name than this!
// TODO: How do we validate the inputs? Possible limitation of records.
// TODO: Expose this as ChannelBase rather than ChannelId? Would be more useful. (Possibly a separate type?)
public record MonoOrStereoPairChannelId(
    ChannelId MonoOrLeftChannelId,
    ChannelId? RightChannelId,
    StereoFlags Flags)
{
}


public record MonoOrStereoPairChannel<T>(
    T MonoOrLeftChannel,
    T? RightChannel,
    StereoFlags Flags)
    where T : ChannelBase    
{
    internal static MonoOrStereoPairChannel<T> Map(MonoOrStereoPairChannelId ids, IReadOnlyDictionary<ChannelId, T> map)
    {
        var left = map[ids.MonoOrLeftChannelId];
        var right = ids.RightChannelId is ChannelId rightId ? map[rightId] : null;
        var flags = ids.Flags;
        return new MonoOrStereoPairChannel<T>(left, right, flags);
    }
}

public record MonoOrStereoPairInputOutputMapping(
    MonoOrStereoPairChannel<InputChannel> Input,
    MonoOrStereoPairChannel<OutputChannel> Output) : IFader
{
    public FaderLevel FaderLevel => throw new NotImplementedException();

    public Task SetFaderLevel(FaderLevel level)
    {
        throw new NotImplementedException();
    }
}
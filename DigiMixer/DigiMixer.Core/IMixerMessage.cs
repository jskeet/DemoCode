namespace DigiMixer.Core;

public interface IMixerMessage<TSelf> where TSelf : class, IMixerMessage<TSelf>
{
    static abstract TSelf? TryParse(ReadOnlySpan<byte> data);
    int Length { get; }
    void CopyTo(Span<byte> buffer);
}

namespace DigiMixer.UCNet.Core.Messages;

// TODO: Validate these against different sources...
public enum MeterStage
{
    Raw = 0,
    PostHpf = 1,
    PostGate = 2,
    PostCompressor = 3,
    PostEQ = 4,
    PostLimiter = 5,
    PostFader = 6,
    Unknonw = 7
}

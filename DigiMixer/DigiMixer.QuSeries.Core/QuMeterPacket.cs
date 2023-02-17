namespace DigiMixer.QuSeries.Core;

/// <summary>
/// A UDP packet broadcast to all clients by a Qu mixer,
/// for metering.
/// </summary>
public class QuMeterPacket
{
    private readonly byte[] data;

    public ReadOnlySpan<byte> Data => data;

    public QuMeterPacket(byte[] data)
    {
        this.data = data;
    }
}

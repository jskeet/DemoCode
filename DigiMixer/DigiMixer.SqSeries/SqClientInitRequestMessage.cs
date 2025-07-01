using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries;

internal class SqClientInitRequestMessage : SqMessage
{
    // We don't know what this means at the moment, but it's always 1...
    public ushort ClientValue => GetUInt16(0);

    internal SqClientInitRequestMessage() : base(SqMessageType.ClientInitRequest, [2, 0])
    {
    }

    internal SqClientInitRequestMessage(AHRawMessage rawMessage) : base(rawMessage)
    {
    }

    public override string ToString() => $"Type={Type}; ClientValue={ClientValue}";
}

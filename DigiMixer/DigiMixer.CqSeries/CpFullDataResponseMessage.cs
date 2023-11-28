using DigiMixer.Core;

namespace DigiMixer.CqSeries.Core;

// Channel modelling:
// Inputs 1-16: regular inputs
// Inputs 17/18: St1 (can't unlink)
// Inputs 19-20: St2 (can't unlink)
// Inputs 21/22: USB (can't unlink)
// Inputs 23/24: BT (can't unlink)
// Outputs 1-6: Out1-6
// Outputs 100/101: MainLR

// TODO: Non-CQ20B modelling...


public class CqFullDataResponseMessage : CqMessage
{
    private static readonly ChannelId UsbLeft = ChannelId.Input(21);
    private static readonly ChannelId UsbRight = ChannelId.Input(22);
    private static readonly ChannelId BluetoothLeft = ChannelId.Input(23);
    private static readonly ChannelId BluetoothRight = ChannelId.Input(24);

    public CqFullDataResponseMessage(byte[] data) : base(CqMessageFormat.VariableLength, CqMessageType.FullDataResponse, data)
    {
    }

    internal CqFullDataResponseMessage(CqRawMessage message) : base(message)
    {
    }

    public string? GetChannelName(ChannelId channelId)
    {
        return channelId switch
        {
            { IsInput: true, Value: 21 or 22 } => "USB",
            { IsInput: true, Value: 23 or 24 } => "BT",
            { IsMainOutput: true } => "Main",
            _ => GetString(GetOffset(), 6)
        };

        int GetOffset () => channelId switch
        {
            { IsInput: true, Value: int ch } when ch < 17 => 0x0180 + (ch - 1) * 0x118,
            { IsInput: true, Value: 17 or 18 } => 0x1bc0,
            { IsInput: true, Value: 19 or 20 } => 0x1df0,
            { IsOutput: true, Value: int ch } when ch < 7 => 0x3600 + (ch - 1) * 0x118,
            _ => throw new ArgumentException($"Cannot get name for channel {channelId}")
        };
    }

    public bool IsMuted(ChannelId channelId)
    {
        int offset = channelId switch
        {
            { IsInput: true, Value: int ch } when ch < 17 => 0x0297 + (ch - 1) * 0x118,
            { IsInput: true, Value: 17 or 18 } => 0x1cd7,
            { IsInput: true, Value: 19 or 20 } => 0x1f07,
            { IsInput: true, Value: 21 or 22 } => 0x2137,
            { IsInput: true, Value: 23 or 24 } => 0x2367,
            { IsOutput: true, Value: int ch } when ch < 7 => 0x3717 + (ch - 1) * 0x118,
            { IsMainOutput: true } => 0x3fd7,
            _ => throw new ArgumentException($"Cannot get mute for channel {channelId}")
        };
        return (Data[offset] & 2) == 2;
    }

    private bool IsStereoLinked(ChannelId channelId) => channelId switch
    {
        // TODO: Check these
        { IsInput: true, Value: int ch } when ch < 17 => (Data[0x44] & (1 << ((ch - 1) / 2))) != 0,
        { IsOutput: true, Value: int ch } => (Data[0x40] & (1 << ((ch - 1) / 2))) != 0,
        _ => throw new ArgumentException($"Cannot get stereo link for channel {channelId}")
    };

    public FaderLevel GetOutputFaderLevel(ChannelId channelId)
    {
        int offset = channelId switch
        {
            { IsOutput: true, Value: int ch } when ch < 7 => (ch - 1) * 0xa8 + 0x6d4c,
            { IsMainOutput: true } => 0x728c,
            _ => throw new ArgumentException($"Cannot get fader level for channel {channelId}")
        };
        return CqConversions.RawToFaderLevel(GetUInt16(offset));
    }

    public FaderLevel GetInputFaderLevel(ChannelId inputChannel, ChannelId outputChannel)
    {
        int inputOffset = inputChannel switch
        {
            { Value: int ch } when ch < 17 => (ch - 1) * 0xa8 + 0x4d9c,
            { Value: 17 or 18 } => 0x5d5c,
            { Value: 19 or 20 } => 0x5eac,
            { Value: 21 or 22 } => 0x5ffc,
            { Value: 23 or 24 } => 0x614c,
            _ => throw new ArgumentException($"Cannot get fader level for channel {inputChannel}")
        };

        int outputOffset = outputChannel switch
        {
            { IsOutput: true, Value: int ch } when ch < 7 => (ch - 1) * 6,
            { IsMainOutput: true } => 0x30,
            _ => throw new ArgumentException($"Cannot get fader level for channel {outputChannel}")
        };
        return CqConversions.RawToFaderLevel(GetUInt16(inputOffset + outputOffset));
    }

    public MixerChannelConfiguration ToMixerChannelConfiguration()
    {
        var inputs = Enumerable.Range(1, 24)
            .Select(ChannelId.Input)
            .Where(id => GetChannelName(id) is not null);
        var outputs = Enumerable.Range(1, 6)
            .Select(ChannelId.Output)
            .Where(id => GetChannelName(id) is not null)
            .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);
        var stereoInputs = Enumerable.Range(0, 8)
            .Select(i => ChannelId.Input(i * 2 + 1))
            .Where(IsStereoLinked)
            .Select(left => StereoPair.FromLeft(left, StereoFlags.None))
            .Append(StereoPair.FromLeft(ChannelId.Input(17), StereoFlags.None))
            .Append(StereoPair.FromLeft(ChannelId.Input(19), StereoFlags.None))
            .Append(StereoPair.FromLeft(UsbLeft, StereoFlags.None))
            .Append(StereoPair.FromLeft(BluetoothLeft, StereoFlags.None));

        var stereoOutputs = Enumerable.Range(0, 3)
            .Select(i => ChannelId.Output(i * 2 + 1))
            .Where(IsStereoLinked)
            .Select(left => StereoPair.FromLeft(left, StereoFlags.None))
            .Append(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));
        return new MixerChannelConfiguration(inputs, outputs, stereoInputs.Concat(stereoOutputs));
    }
}

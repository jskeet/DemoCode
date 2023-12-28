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
            { IsInput: true, Value: CqChannels.UsbLeftValue or CqChannels.UsbRightValue } => "USB",
            { IsInput: true, Value: CqChannels.BluetoothLeftValue or CqChannels.BluetoothRightValue } => "BT",
            { IsMainOutput: true } => "Main",
            _ => GetString(GetOffset(), 6)
        };

        int GetOffset() => channelId switch
        {
            { IsInput: true, Value: int ch } when ch <= CqChannels.MonoInputCount => 0x0180 + (ch - 1) * 0x118,
            { IsInput: true, Value: CqChannels.Stereo1LeftValue or CqChannels.Stereo1RightValue } => 0x1bc0,
            { IsInput: true, Value: CqChannels.Stereo2LeftValue or CqChannels.Stereo2RightValue } => 0x1df0,
            { IsOutput: true, Value: int ch } when ch <= CqChannels.MonoOutputCount => 0x3600 + (ch - 1) * 0x118,
            _ => throw new ArgumentException($"Cannot get name for channel {channelId}")
        };
    }

    public bool IsMuted(ChannelId channelId)
    {
        int offset = channelId switch
        {
            { IsInput: true, Value: int ch } when ch <= CqChannels.MonoInputCount => 0x0297 + (ch - 1) * 0x118,
            { IsInput: true, Value: CqChannels.Stereo1LeftValue or CqChannels.Stereo1RightValue } => 0x1cd7,
            { IsInput: true, Value: CqChannels.Stereo2LeftValue or CqChannels.Stereo2RightValue } => 0x1f07,
            { IsInput: true, Value: CqChannels.UsbLeftValue or CqChannels.UsbRightValue } => 0x2137,
            { IsInput: true, Value: CqChannels.BluetoothLeftValue or CqChannels.BluetoothRightValue } => 0x2367,
            { IsOutput: true, Value: int ch } when ch <= CqChannels.MonoOutputCount => 0x3717 + (ch - 1) * 0x118,
            { IsMainOutput: true } => 0x3fd7,
            _ => throw new ArgumentException($"Cannot get mute for channel {channelId}")
        };
        return (Data[offset] & 2) == 2;
    }

    private bool IsStereoLinked(ChannelId channelId) => channelId switch
    {
        { IsInput: true, Value: CqChannels.Stereo1LeftValue } => true,
        { IsInput: true, Value: CqChannels.Stereo2LeftValue } => true,
        { IsInput: true, Value: CqChannels.UsbLeftValue } => true,
        { IsInput: true, Value: CqChannels.BluetoothLeftValue } => true,
        { IsInput: true, Value: int ch } when ch < 17 => (Data[0x44] & (1 << ((ch - 1) / 2))) != 0,

        { IsMainOutput: true } => true,
        { IsOutput: true, Value: int ch } => (Data[0x4c] & (1 << ((ch - 1) / 2) + 4)) != 0,
        _ => throw new ArgumentException($"Cannot get stereo link for channel {channelId}")
    };

    public FaderLevel GetOutputFaderLevel(ChannelId channelId)
    {
        int offset = channelId switch
        {
            { IsOutput: true, Value: int ch } when ch <= CqChannels.MonoOutputCount => (ch - 1) * 0xa8 + 0x6d4c,
            { IsMainOutput: true } => 0x728c,
            _ => throw new ArgumentException($"Cannot get fader level for channel {channelId}")
        };
        return CqConversions.RawToFaderLevel(GetUInt16(offset));
    }

    public FaderLevel GetInputFaderLevel(ChannelId inputChannel, ChannelId outputChannel)
    {
        int inputOffset = inputChannel switch
        {
            { Value: int ch } when ch <= CqChannels.MonoInputCount => (ch - 1) * 0xa8 + 0x4d9c,
            { IsInput: true, Value: CqChannels.Stereo1LeftValue or CqChannels.Stereo1RightValue } => 0x5d5c,
            { IsInput: true, Value: CqChannels.Stereo2LeftValue or CqChannels.Stereo2RightValue } => 0x5eac,
            { IsInput: true, Value: CqChannels.UsbLeftValue or CqChannels.UsbRightValue } => 0x5ffc,
            { IsInput: true, Value: CqChannels.BluetoothLeftValue or CqChannels.BluetoothRightValue } => 0x614c,
            _ => throw new ArgumentException($"Cannot get fader level for channel {inputChannel}")
        };

        int outputOffset = outputChannel switch
        {
            { IsOutput: true, Value: int ch } when ch <= CqChannels.MonoOutputCount => (ch - 1) * 6,
            { IsMainOutput: true } => 0x30,
            _ => throw new ArgumentException($"Cannot get fader level for channel {outputChannel}")
        };
        return CqConversions.RawToFaderLevel(GetUInt16(inputOffset + outputOffset));
    }

    public MixerChannelConfiguration ToMixerChannelConfiguration()
    {
        var stereoLinks = CqChannels.LeftInputs.Concat(CqChannels.LeftOutputs)
            .Where(IsStereoLinked)
            .Select(left => StereoPair.FromLeft(left, StereoFlags.None));
        return new MixerChannelConfiguration(CqChannels.AllInputs, CqChannels.AllOutputs, stereoLinks);
    }
}

using DigiMixer.Core;

namespace DigiMixer.CqSeries;

internal static class CqChannels
{
    internal const int MonoInputCount = 16;
    internal const int MonoOutputCount = 6;

    internal const byte Stereo1LeftValue = 17;
    internal const byte Stereo1RightValue = 18;
    internal const byte Stereo2LeftValue = 19;
    internal const byte Stereo2RightValue = 20;
    internal const byte UsbLeftValue = 21;
    internal const byte UsbRightValue = 22;
    internal const byte BluetoothLeftValue = 23;
    internal const byte BluetoothRightValue = 24;

    internal static ChannelId Stereo1Left = ChannelId.Input(Stereo1LeftValue);
    internal static ChannelId Stereo1Right = ChannelId.Input(Stereo1RightValue);
    internal static ChannelId Stereo2Left = ChannelId.Input(Stereo2LeftValue);
    internal static ChannelId Stereo2Right = ChannelId.Input(Stereo2RightValue);
    internal static ChannelId UsbLeft { get; } = ChannelId.Input(UsbLeftValue);
    internal static ChannelId UsbRight { get; } = ChannelId.Input(UsbRightValue);
    internal static ChannelId BluetoothLeft { get; } = ChannelId.Input(BluetoothLeftValue);
    internal static ChannelId BluetoothRight { get; } = ChannelId.Input(BluetoothRightValue);

    internal static IEnumerable<ChannelId> AllInputs => Enumerable.Range(1, 24).Select(ChannelId.Input);
    internal static IEnumerable<ChannelId> AllOutputs => Enumerable.Range(1, MonoOutputCount).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);

    internal static IEnumerable<ChannelId> LeftInputs => AllInputs.Where(ch => (ch.Value & 1) == 1);
    internal static IEnumerable<ChannelId> LeftOutputs => AllOutputs.Where(ch => (!ch.IsMainOutput && (ch.Value & 1) == 1) || ch == ChannelId.MainOutputLeft);

    /// <summary>
    /// The inverse of <see cref="ChannelIdToNetwork(ChannelId)"/>.
    /// </summary>
    public static ChannelId NetworkToChannelId(byte channel) => channel switch
    {
        // Mono inputs
        < MonoInputCount => ChannelId.Input(channel + 1),
        0x18 => Stereo1Left,
        0x1A => Stereo2Left,
        0x1C => UsbLeft,
        0x1E => BluetoothLeft,
        0x38 => ChannelId.MainOutputLeft,
        _ when channel >= 0x30 && channel < 0x36 => ChannelId.Output(channel - 0x2f),
        _ => throw new ArgumentException($"Can't map channel {channel}")
    };

    /// <summary>
    /// Maps a channel ID to the network representation, for values which can be input or output.
    /// </summary>
    public static byte ChannelIdToNetwork(ChannelId channel) => channel switch
    {
        { IsInput: true, Value: int ch } when ch <= MonoInputCount => (byte) (ch - 1),
        { IsInput: true, Value: Stereo1LeftValue or Stereo1RightValue } => 0x18,
        { IsInput: true, Value: Stereo2LeftValue or Stereo2RightValue } => 0x1A,
        { IsInput: true, Value: UsbLeftValue or UsbRightValue } => 0x1C,
        { IsInput: true, Value: BluetoothLeftValue or BluetoothRightValue } => 0x1E,
        { IsOutput: true, Value: int ch } when ch <= MonoOutputCount => (byte) (0x2f + ch),
        { IsMainOutput: true } => 0x38,
        _ => throw new ArgumentException($"Can't map channel {channel}")
    };

    /// <summary>
    /// Maps an output channel ID to the network representation, for values which are only ever output.
    /// </summary>
    public static byte OutputChannelIdToNetwork(ChannelId channel) => channel switch
    {
        { IsOutput: true, Value: int ch } when ch <= MonoOutputCount => (byte) (ch + 7),
        { IsMainOutput: true } => 0x10,
        _ => throw new ArgumentException($"Can't map channel {channel}")
    };
}

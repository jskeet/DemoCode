using DigiMixer.Core;
using DigiMixer.Mackie.Core;

namespace DigiMixer.Mackie;

internal abstract class MixerProfile
{
    private const int ReturnChannelOffset = 50;
    private const int FxChannelOffset = 70;

    private readonly Lazy<IReadOnlyList<MackieInputChannel>> inputChannels;
    private readonly Lazy<IReadOnlyList<MackieOutputChannel>> outputChannels;

    protected abstract int InputChannelCount { get; }
    protected abstract int ReturnChannelCount { get; }
    protected abstract int FxChannelCount { get; }
    protected abstract int AuxChannelCount { get; }

    public abstract byte ModelNameInfoRequest { get; }

    protected MixerProfile()
    {
        inputChannels = new Lazy<IReadOnlyList<MackieInputChannel>>(GetInputChannels, LazyThreadSafetyMode.PublicationOnly);
        outputChannels = new Lazy<IReadOnlyList<MackieOutputChannel>>(GetOutputChannels, LazyThreadSafetyMode.PublicationOnly);
    }

    internal static MixerProfile GetProfile(MackieMessage handshakeMessage)
    {
        if (handshakeMessage.Body.Length != 16)
        {
            return DL16SProfile.Instance;
        }

        // TODO: Work out whether this is the right way to detect a DL32R.
        // First two bytes for DL16S are 10 09; first two bytes for DL32R are 10 05.
        if (handshakeMessage.Body.Data[1] == 5)
        {
            return DL32RProfile.Instance;
        }
        return DL16SProfile.Instance;
    }

    /// <summary>
    /// Retrieves the model name given a message requested using
    /// <see cref="ModelNameInfoRequest"/>.
    /// </summary>
    internal abstract string GetModelName(MackieMessage modelInfo);

    private IReadOnlyList<MackieInputChannel> GetInputChannels()
    {
        var inputs = Enumerable.Range(1, InputChannelCount).Select(Input);
        var returns = Enumerable.Range(1, ReturnChannelCount).Select(Return);
        var fx = Enumerable.Range(1, FxChannelCount).Select(Fx);
        return inputs.Concat(returns).Concat(fx).ToList().AsReadOnly();

        MackieInputChannel Input(int index)
        {
            var zeroIndex = index - 1;
            var meterAddress = Input1MeterAddress + zeroIndex * InputMeterSize + InputMeterOffset;
            var startAddress = Input1StartAddress + zeroIndex * InputValuesSize;
            var muteAddress = startAddress + InputMuteOffset;
            var stereoLinkAddress = startAddress + InputStereoLinkOffset;
            var mainFaderAddress = startAddress + InputMainFaderOffset;
            var aux1FaderAddress = startAddress + InputAux1FaderOffset;
            var fx1FaderAddress = startAddress + InputFx1FaderOffset;
            var nameIndex = Input1NameIndex + zeroIndex;
            return new MackieInputChannel(ChannelId.Input(index), meterAddress, nameIndex, muteAddress, stereoLinkAddress, mainFaderAddress, aux1FaderAddress, fx1FaderAddress);
        }

        MackieInputChannel Return(int index)
        {
            var zeroIndex = index - 1;
            var meterAddress = Return1MeterAddress + zeroIndex * ReturnMeterSize + ReturnMeterOffset;
            var startAddress = Return1StartAddress + zeroIndex * ReturnValuesSize;
            var muteAddress = startAddress + ReturnMuteOffset;
            var stereoLinkAddress = startAddress + ReturnStereoLinkOffset;
            var mainFaderAddress = startAddress + ReturnMainFaderOffset;
            var aux1FaderAddress = startAddress + ReturnAux1FaderOffset;
            var fx1FaderAddress = startAddress + ReturnFx1FaderOffset;
            var nameIndex = Return1NameIndex + zeroIndex;
            return new MackieInputChannel(ChannelId.Input(index + ReturnChannelOffset), meterAddress, nameIndex, muteAddress, stereoLinkAddress, mainFaderAddress, aux1FaderAddress, fx1FaderAddress);
        }

        MackieInputChannel Fx(int index)
        {
            var zeroIndex = index - 1;
            var meterAddress = FxInput1MeterAddress + zeroIndex * FxInputMeterSize + FxInputMeterOffset;
            var startAddress = FxInput1StartAddress + zeroIndex * FxInputValuesSize;
            var muteAddress = startAddress + FxInputMuteOffset;
            var mainFaderAddress = startAddress + FxInputMainFaderOffset;
            var aux1FaderAddress = startAddress + FxInputAux1FaderOffset;
            var nameIndex = FxInput1NameIndex + zeroIndex;
            // FX inputs are never stereo linked, and don't feed into FX outputs (so don't have faders for that).
            return new MackieInputChannel(ChannelId.Input(index + FxChannelOffset), meterAddress, nameIndex, muteAddress, null, mainFaderAddress, aux1FaderAddress, null);
        }
    }

    private IReadOnlyList<MackieOutputChannel> GetOutputChannels()
    {
        var aux = Enumerable.Range(1, AuxChannelCount).Select(Aux);
        var fx = Enumerable.Range(1, FxChannelCount).Select(Fx);
        var mainLeft = new MackieOutputChannel(OutputGroup.Main, 1, ChannelId.MainOutputLeft, MainLeftMeterAddress, MainMuteAddress, null, MainFaderAddress, MainNameIndex);
        var mainRight = new MackieOutputChannel(OutputGroup.Main, 2, ChannelId.MainOutputRight, MainRightMeterAddress, null, null, null, null);
        return aux.Concat(fx).Append(mainLeft).Append(mainRight).ToList().AsReadOnly();

        MackieOutputChannel Aux(int index)
        {
            var zeroIndex = index - 1;
            var meterAddress = Aux1MeterAddress + zeroIndex * AuxMeterSize + AuxMeterOffset;
            var startAddress = Aux1StartAddress + zeroIndex * AuxValuesSize;
            var muteAddress = startAddress + AuxMuteOffset;
            var stereoLinkAddress = startAddress + AuxStereoLinkOffset;
            var faderAddress = startAddress + AuxFaderOffset;
            var nameIndex = Aux1NameIndex + zeroIndex;
            return new MackieOutputChannel(OutputGroup.Aux, index, ChannelId.Output(index), meterAddress, muteAddress, stereoLinkAddress, faderAddress, nameIndex);
        }

        MackieOutputChannel Fx(int index)
        {
            var zeroIndex = index - 1;
            var meterAddress = FxOutput1MeterAddress + zeroIndex * FxOutputMeterSize + FxOutputMeterOffset;
            var muteAddress = FxOutput1StartAddress + zeroIndex * FxOutputValuesSize + FxOutputMuteOffset;
            var faderAddress = FxOutput1StartAddress + zeroIndex * FxOutputValuesSize + FxOutputFaderOffset;
            var nameIndex = FxOutput1NameIndex + zeroIndex;
            // FX outputs are never stereo linked.
            return new MackieOutputChannel(OutputGroup.Fx, index, ChannelId.Output(index + FxChannelOffset), meterAddress, muteAddress, null, faderAddress, nameIndex);
        }
    }

    // TODO: Potentially optimize this?
    private MackieOutputChannel? GetOutputChannel(ChannelId channelId) => OutputChannels.FirstOrDefault(oc => oc.Id == channelId);
    private MackieInputChannel? GetInputChannel(ChannelId channelId) => InputChannels.FirstOrDefault(ic => ic.Id == channelId);

    internal int? GetMuteAddress(ChannelId channelId) =>
        GetInputChannel(channelId)?.MuteAddress ?? GetOutputChannel(channelId)?.MuteAddress;

    internal int? GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        var input = GetInputChannel(inputId);
        var output = GetOutputChannel(outputId);
        return input is not null && output is not null ? input.GetFaderAddress(output) : null;
    }

    internal int? GetFaderAddress(ChannelId outputId) => GetOutputChannel(outputId)?.FaderAddress;

    internal IReadOnlyList<MackieInputChannel> InputChannels => inputChannels.Value;
    internal IReadOnlyList<MackieOutputChannel> OutputChannels => outputChannels.Value;

    /// <summary>
    /// The start of the values for input channel 1.
    /// </summary>
    protected abstract int Input1StartAddress { get; }
    /// <summary>
    /// The number of bytes used for the value set for each input channel.
    /// </summary>
    protected abstract int InputValuesSize { get; }
    /// <summary>
    /// The offset from the start of input channel values to the mute for the channel.
    /// </summary>
    protected abstract int InputMuteOffset { get; }
    /// <summary>
    /// The offset from the start of input channel values to the stereo link value.
    /// </summary>
    protected abstract int InputStereoLinkOffset { get; }
    /// <summary>
    /// The offset from the start of input channel values to the LR fader for the channel.
    /// </summary>
    protected abstract int InputMainFaderOffset { get; }
    /// <summary>
    /// The offset from the start of input channel values to the Aux1 fader for the channel.
    /// </summary>
    protected abstract int InputAux1FaderOffset { get; }
    /// <summary>
    /// The offset from the start of input channel values to the FX1 fader for the channel.
    /// </summary>
    protected abstract int InputFx1FaderOffset { get; }
    /// <summary>
    /// The index of input channel 1 in the names message.
    /// </summary>
    protected abstract int Input1NameIndex { get; }
    /// <summary>
    /// The first address for meter values for Input1.
    /// </summary>
    protected abstract int Input1MeterAddress { get; }
    /// <summary>
    /// The offset within a set of input meter values to report.
    /// </summary>
    protected abstract int InputMeterOffset { get; }
    /// <summary>
    /// The size of meter value sets for inputs.
    /// </summary>
    protected abstract int InputMeterSize { get; }

    protected abstract int Return1StartAddress { get; }
    protected abstract int ReturnValuesSize { get; }
    protected abstract int ReturnMuteOffset { get; }
    protected abstract int ReturnStereoLinkOffset { get; }
    protected abstract int ReturnMainFaderOffset { get; }
    protected abstract int ReturnAux1FaderOffset { get; }
    protected abstract int ReturnFx1FaderOffset { get; }
    protected abstract int Return1NameIndex { get; }
    protected abstract int Return1MeterAddress { get; }
    protected abstract int ReturnMeterOffset { get; }
    protected abstract int ReturnMeterSize { get; }

    protected abstract int FxInput1StartAddress { get; }
    protected abstract int FxInputValuesSize { get; }
    protected abstract int FxInputMuteOffset { get; }
    protected abstract int FxInputMainFaderOffset { get; }
    protected abstract int FxInputAux1FaderOffset { get; }
    protected abstract int FxInput1NameIndex { get; }
    protected abstract int FxInput1MeterAddress { get; }
    protected abstract int FxInputMeterOffset { get; }
    protected abstract int FxInputMeterSize { get; }

    protected abstract int Aux1StartAddress { get; }
    protected abstract int AuxValuesSize { get; }
    protected abstract int AuxMuteOffset { get; }
    protected abstract int AuxStereoLinkOffset { get; }
    protected abstract int AuxFaderOffset { get; }
    protected abstract int Aux1NameIndex { get; }
    protected abstract int Aux1MeterAddress { get; }
    protected abstract int AuxMeterOffset { get; }
    protected abstract int AuxMeterSize { get; }

    protected abstract int FxOutput1StartAddress { get; }
    protected abstract int FxOutputValuesSize { get; }
    protected abstract int FxOutputMuteOffset { get; }
    protected abstract int FxOutputFaderOffset { get; }
    protected abstract int FxOutput1NameIndex { get; }
    protected abstract int FxOutput1MeterAddress { get; }
    protected abstract int FxOutputMeterOffset { get; }
    protected abstract int FxOutputMeterSize { get; }

    protected abstract int MainMuteAddress { get; }
    protected abstract int MainFaderAddress { get; }
    protected abstract int MainLeftMeterAddress { get; }
    protected abstract int MainRightMeterAddress { get; }
    protected abstract int MainNameIndex { get; }
}

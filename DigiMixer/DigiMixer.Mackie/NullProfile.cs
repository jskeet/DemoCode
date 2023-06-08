using DigiMixer.Mackie.Core;

namespace DigiMixer.Mackie;

internal class NullProfile : MixerProfile
{
    internal static NullProfile Instance { get; } = new NullProfile();

    public override byte ModelNameInfoRequest => 0x12;

    protected override int InputChannelCount => 0;
    protected override int ReturnChannelCount => 0;
    protected override int FxChannelCount => 0;
    protected override int AuxChannelCount => 0;

    // We shouldn't be asked for any of these, as we've said we don't have any inputs/returns/fx/aux.
    protected override int Input1StartAddress => throw new NotImplementedException();
    protected override int InputValuesSize => throw new NotImplementedException();
    protected override int InputMuteOffset => throw new NotImplementedException();
    protected override int InputStereoLinkOffset => throw new NotImplementedException();
    protected override int InputMainFaderOffset => throw new NotImplementedException();
    protected override int InputAux1FaderOffset => throw new NotImplementedException();
    protected override int InputFx1FaderOffset => throw new NotImplementedException();
    protected override int Input1NameIndex => throw new NotImplementedException();
    protected override int Input1MeterAddress => throw new NotImplementedException();
    protected override int InputMeterOffset => throw new NotImplementedException();
    protected override int InputMeterSize => throw new NotImplementedException();
    protected override int Return1StartAddress => throw new NotImplementedException();
    protected override int ReturnValuesSize => throw new NotImplementedException();
    protected override int ReturnMuteOffset => throw new NotImplementedException();
    protected override int ReturnStereoLinkOffset => throw new NotImplementedException();
    protected override int ReturnMainFaderOffset => throw new NotImplementedException();
    protected override int ReturnAux1FaderOffset => throw new NotImplementedException();
    protected override int ReturnFx1FaderOffset => throw new NotImplementedException();
    protected override int Return1NameIndex => throw new NotImplementedException();
    protected override int Return1MeterAddress => throw new NotImplementedException();
    protected override int ReturnMeterOffset => throw new NotImplementedException();
    protected override int ReturnMeterSize => throw new NotImplementedException();

    protected override int FxInput1StartAddress => throw new NotImplementedException();
    protected override int FxInputValuesSize => throw new NotImplementedException();
    protected override int FxInputMuteOffset => throw new NotImplementedException();
    protected override int FxInputMainFaderOffset => throw new NotImplementedException();
    protected override int FxInputAux1FaderOffset => throw new NotImplementedException();
    protected override int FxInput1NameIndex => throw new NotImplementedException();
    protected override int FxInput1MeterAddress => throw new NotImplementedException();
    protected override int FxInputMeterOffset => throw new NotImplementedException();
    protected override int FxInputMeterSize => throw new NotImplementedException();

    protected override int Aux1StartAddress => throw new NotImplementedException();
    protected override int AuxValuesSize => throw new NotImplementedException();
    protected override int AuxMuteOffset => throw new NotImplementedException();
    protected override int AuxStereoLinkOffset => throw new NotImplementedException();
    protected override int AuxFaderOffset => throw new NotImplementedException();
    protected override int Aux1NameIndex => throw new NotImplementedException();
    protected override int Aux1MeterAddress => throw new NotImplementedException();
    protected override int AuxMeterOffset => throw new NotImplementedException();
    protected override int AuxMeterSize => throw new NotImplementedException();

    protected override int FxOutput1StartAddress => throw new NotImplementedException();
    protected override int FxOutputValuesSize => throw new NotImplementedException();
    protected override int FxOutputMuteOffset => throw new NotImplementedException();
    protected override int FxOutputFaderOffset => throw new NotImplementedException();
    protected override int FxOutput1NameIndex => throw new NotImplementedException();
    protected override int FxOutput1MeterAddress => throw new NotImplementedException();
    protected override int FxOutputMeterOffset => throw new NotImplementedException();
    protected override int FxOutputMeterSize => throw new NotImplementedException();

    protected override int MainMuteAddress => 0;
    protected override int MainFaderAddress => 0;
    protected override int MainLeftMeterAddress => 0;
    protected override int MainRightMeterAddress => 0;
    protected override int MainNameIndex => 0;

    private NullProfile()
    {
    }

    internal override string GetModelName(MackiePacket modelInfo) => "";
}

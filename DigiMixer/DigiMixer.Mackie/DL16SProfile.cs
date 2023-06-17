using DigiMixer.Mackie.Core;
using System.Text;

namespace DigiMixer.Mackie;

internal class DL16SProfile : MixerProfile
{
    internal static DL16SProfile Instance { get; } = new DL16SProfile();

    private DL16SProfile()
    {
    }

    protected override int InputChannelCount => 16;
    protected override int ReturnChannelCount => 2;
    protected override int FxChannelCount => 4;
    protected override int AuxChannelCount => 6;

    public override byte ModelNameInfoRequest => 18;

    protected override int Input1StartAddress => 1;
    protected override int InputValuesSize => 100;
    protected override int InputMuteOffset => 7;
    protected override int InputStereoLinkOffset => 11;
    protected override int InputMainFaderOffset => 8;
    protected override int InputAux1FaderOffset => 50;
    protected override int InputFx1FaderOffset => 68;
    protected override int Input1NameIndex => 1;
    protected override int Input1MeterAddress => 34;
    protected override int InputMeterOffset => 0;
    protected override int InputMeterSize => 7;

    protected override int Return1StartAddress => 1601;
    protected override int ReturnValuesSize => 88;
    protected override int ReturnMuteOffset => 7;
    protected override int ReturnStereoLinkOffset => 11;
    protected override int ReturnMainFaderOffset => 8;
    protected override int ReturnAux1FaderOffset => 38;
    protected override int ReturnFx1FaderOffset => 56;
    protected override int Return1NameIndex => 17;
    protected override int Return1MeterAddress => 146;
    protected override int ReturnMeterOffset => 1;
    protected override int ReturnMeterSize => 4;

    protected override int FxInput1StartAddress => 1949;
    protected override int FxInputValuesSize => 66;
    protected override int FxInputMuteOffset => 2;
    protected override int FxInputMainFaderOffset => 3;
    protected override int FxInputAux1FaderOffset => 24;
    protected override int FxInput1NameIndex => 23;
    protected override int FxInput1MeterAddress => 154;
    protected override int FxInputMeterOffset => 0;
    protected override int FxInputMeterSize => 1;

    protected override int Aux1StartAddress => 2606;
    protected override int AuxValuesSize => 90;
    protected override int AuxMuteOffset => 3;
    protected override int AuxStereoLinkOffset => 7;
    protected override int AuxFaderOffset => 4;
    protected override int Aux1NameIndex => 34;
    protected override int Aux1MeterAddress => 198;
    protected override int AuxMeterOffset => 0;
    protected override int AuxMeterSize => 4;

    protected override int FxOutput1StartAddress => 1777;
    protected override int FxOutputValuesSize => 12;
    protected override int FxOutputMuteOffset => 2;
    protected override int FxOutputFaderOffset => 3;
    protected override int FxOutput1NameIndex => 19;
    protected override int FxOutput1MeterAddress => 158;
    protected override int FxOutputMeterOffset => 0; // Left
    protected override int FxOutputMeterSize => 2;

    protected override int MainMuteAddress => 2520;
    protected override int MainFaderAddress => 2519;
    protected override int MainLeftMeterAddress => 190; // Post
    protected override int MainRightMeterAddress => 191; // Post
    protected override int MainNameIndex => 33;

    internal override string GetModelName(MackieMessage modelInfo)
    {
        var data = modelInfo.Body.InSequentialOrder().Data;
        // The use of 16 here is somewhat arbitrary... there's lots we don't understand in this message.
        return Encoding.UTF8.GetString(data.Slice(8, 16)).TrimEnd('\0');
    }
}

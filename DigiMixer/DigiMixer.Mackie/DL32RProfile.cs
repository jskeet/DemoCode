using DigiMixer.Mackie.Core;
using System.Text;

namespace DigiMixer.Mackie;

internal class DL32RProfile : MixerProfile
{
    internal static DL32RProfile Instance { get; } = new DL32RProfile();

    private DL32RProfile()
    {
    }
    // Need to check *all* meters.

    protected override int InputChannelCount => 32;
    protected override int ReturnChannelCount => 4;
    protected override int AuxChannelCount => 14;
    protected override int FxChannelCount => 4;
    public override byte ModelNameInfoRequest => 7;

    protected override int Input1StartAddress => 41;
    protected override int InputValuesSize => 132;
    protected override int InputMuteOffset => 7;
    protected override int InputStereoLinkOffset => 11;
    protected override int InputMainFaderOffset => 8;
    protected override int InputAux1FaderOffset => 58;
    protected override int InputFx1FaderOffset => 100;
    protected override int Input1NameIndex => 1;
    protected override int Input1MeterAddress => 34; // FIXME
    protected override int InputMeterOffset => 0; // FIXME
    protected override int InputMeterSize => 7; // FIXME

    protected override int Return1StartAddress => 4265;
    protected override int ReturnValuesSize => 120;
    protected override int ReturnMuteOffset => 7;
    protected override int ReturnStereoLinkOffset => 11;
    protected override int ReturnMainFaderOffset => 8;
    protected override int ReturnAux1FaderOffset => 46;
    protected override int ReturnFx1FaderOffset => 88;
    protected override int Return1NameIndex => 33; // Verified
    protected override int Return1MeterAddress => 146;
    protected override int ReturnMeterOffset => 1;
    protected override int ReturnMeterSize => 4;

    // FX1 parameters: 4841-4871
    // FX2 parameters: 4872-4902
    // FX3 parameters: 4903-4933?
    // FX4 parameters... 4934-4964

    protected override int FxInput1StartAddress => 4965;
    protected override int FxInputValuesSize => 98;
    protected override int FxInputMuteOffset => 2;
    protected override int FxInputMainFaderOffset => 3;
    protected override int FxInputAux1FaderOffset => 32;
    protected override int FxInput1NameIndex => 41;
    protected override int FxInput1MeterAddress => 154;
    protected override int FxInputMeterOffset => 0;
    protected override int FxInputMeterSize => 1;

    protected override int Aux1StartAddress => 6130;
    protected override int AuxValuesSize => 120;
    protected override int AuxMuteOffset => 3;
    protected override int AuxStereoLinkOffset => 7;
    protected override int AuxFaderOffset => 4;
    protected override int Aux1NameIndex => 52;
    protected override int Aux1MeterAddress => 198;
    protected override int AuxMeterOffset => 0;
    protected override int AuxMeterSize => 4;

    protected override int FxOutput1StartAddress => 4745;
    protected override int FxOutputValuesSize => 24;
    protected override int FxOutputMuteOffset => 2;
    protected override int FxOutputFaderOffset => 3;
    protected override int FxOutput1NameIndex => 37;
    protected override int FxOutput1MeterAddress => 158;
    protected override int FxOutputMeterOffset => 0; // Left
    protected override int FxOutputMeterSize => 2;

    protected override int MainMuteAddress => 5996;
    protected override int MainFaderAddress => 5995;
    protected override int MainLeftMeterAddress => 190; // FIXME: Post
    protected override int MainRightMeterAddress => 191; // FIXME: Post
    protected override int MainNameIndex => 51; // Bit of a guess, but consistent. Matrix names before main name?

    internal override string GetModelName(MackieMessage modelInfo) =>
        Encoding.UTF8.GetString(modelInfo.Body.InSequentialOrder().Data.Slice(4)).TrimEnd('\0');
}

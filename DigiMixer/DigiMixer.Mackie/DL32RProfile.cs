using DigiMixer.Core;
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

    protected override int Input1StartAddress => 39;
    protected override int InputValuesSize => 132;
    protected override int InputMuteOffset => 9;
    protected override int InputStereoLinkOffset => 13;
    protected override int InputMainFaderOffset => 10;
    protected override int InputAux1FaderOffset => 60;
    protected override int InputFx1FaderOffset => 68; // FIXME
    protected override int Input1NameIndex => 1;
    protected override int Input1MeterAddress => 34; // FIXME
    protected override int InputMeterOffset => 0; // FIXME
    protected override int InputMeterSize => 7; // FIXME

    protected override int Return1StartAddress => 1601;
    protected override int ReturnValuesSize => 88;
    protected override int ReturnMuteOffset => 7;
    protected override int ReturnStereoLinkOffset => 11;
    protected override int ReturnMainFaderOffset => 8;
    protected override int ReturnAux1FaderOffset => 38;
    protected override int ReturnFx1FaderOffset => 56;
    protected override int Return1NameIndex => 33; // Verified
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

    protected override int Aux1StartAddress => 6130;
    protected override int AuxValuesSize => 120;
    protected override int AuxMuteOffset => 3;
    protected override int AuxStereoLinkOffset => 7; // Check this
    protected override int AuxFaderOffset => 4; // Check this
    protected override int Aux1NameIndex => 52; // Verified
    protected override int Aux1MeterAddress => 198;
    protected override int AuxMeterOffset => 0;
    protected override int AuxMeterSize => 4;

    protected override int FxOutput1StartAddress => 1777; // FIXME for all of this
    protected override int FxOutputValuesSize => 12;
    protected override int FxOutputMuteOffset => 2;
    protected override int FxOutputFaderOffset => 3;
    protected override int FxOutput1NameIndex => 19;
    protected override int FxOutput1MeterAddress => 158;
    protected override int FxOutputMeterOffset => 0; // Left
    protected override int FxOutputMeterSize => 2;

    protected override int MainMuteAddress => 5996;
    protected override int MainFaderAddress => 2519; // FIXME
    protected override int MainLeftMeterAddress => 190; // Post
    protected override int MainRightMeterAddress => 191; // Post
    protected override int MainNameIndex => 51; // Bit of a guess, but consistent. Matrix names before main name?

    internal override string GetModelName(MackiePacket modelInfo) =>
        Encoding.UTF8.GetString(modelInfo.Body.InSequentialOrder().Data.Slice(4)).TrimEnd('\0');

    // Known addresses (decimal):
    // Mute input 1: 48
    // Mute input 2: 180
    // Main fader input 1: 49
    // Main fader input 2: 181
    // Mute aux 1: 6133
    // Mute aux 2: 6253
    // Fader aux 1: 6134
    // Fader aux 2: 6254
    // Mute LR: 5996

    // Inputs *might* start at 41 - or possibly 39? (1000/2000, then at 173 we have 1001/2001 etc).
    // Current code assumes 41.
    // - That would put "first address past inputs" as 4265.



    // Names: (check routing)
    // 1-32: Inputs 1-32
    // 33: "From Zoom"?
    // 34:
    // 35: Clavinova
    // 36: Dante 2
    // 45+46: Line 1-2 out
    // 49: To zoom
    // 50: 
    // 51: LR
    // 52-77: ??
    // 78: MORNING SERVICE
    // 79: EVENING SERVICE
    // 82: Wedding
    // 83: Shing

    // Another ChannelNames - how do we differentiate? (Check first chunk?)
    // 1: FOH
    // 3: MON
    // 5: 3
    // 7: 4
    // 9: Song1
    // 10: 1
    // 11: 1

    /*
    public int GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        int inputBase = GetInputOrigin(inputId);
        int offset = outputId.IsMainOutput ? 8 : outputId.Value * 3 + 55;
        return inputBase + offset;
    }

    public IEnumerable<MackieOutputChannel> GetOutputChannels() =>
        Enumerable.Range(1, 14).Select(aux => MackieOutputChannel.Aux(aux, -999, (aux - 1) * 120 + 6133, 0, 1, aux + 51));

    public IEnumerable<MackieInputChannel> GetInputChannels() =>
        Enumerable.Empty<MackieInputChannel>();

    public int GetFaderAddress(ChannelId outputId) =>
        outputId.IsMainOutput ? 5997 : GetOutputOrigin(outputId) + 1;

    public int GetMuteAddress(ChannelId channelId) =>
        channelId.IsInput ? GetInputOrigin(channelId) + 0x07
        : channelId.IsMainOutput ? 5996
        : GetOutputOrigin(channelId) + 0;

    public int GetNameAddress(ChannelId channelId) =>
        channelId.IsInput ? channelId.Value
        : channelId.IsMainOutput ? 51
        : channelId.Value + 51;

    public int GetStereoLinkAddress(ChannelId channelId) =>
        channelId.IsInput
        ? GetInputOrigin(channelId) + 0x0b
        : GetOutputOrigin(channelId) + 0x04;

    private static int GetInputOrigin(ChannelId inputId) => (inputId.Value - 1) * 132 + 41;
    private static int GetOutputOrigin(ChannelId outputId) => (outputId.Value - 1) * 120 + 6133;*/
}

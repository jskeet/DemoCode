namespace DigiMixer.UiHttp;

internal static class UiMeters
{
    internal static class Header
    {
        internal const int Length = 8;

        internal const int Inputs = 0;
        internal const int Media = 1;
        internal const int Subgroups = 2;
        internal const int Fx = 3;
        internal const int Aux = 4;
        internal const int Mains = 5;
        internal const int LinesIn = 6;
    }

    /// <summary>
    /// Indexes into meter records for inputs, media and line-in channels.
    /// </summary>
    internal static class InputsMediaLinesIn
    {
        internal const int Length = 6;

        internal const int Pre = 0;
        internal const int Post = 1;
        internal const int PostFader = 2;
        internal const int GateIn = 3;
        internal const int CompOut = 4;
        internal const int CompMeterAndGated = 5;
    }

    /// <summary>
    /// Indexes into meter records for Subgroups and FX.
    /// </summary>
    internal static class SubgroupsFx
    {
        internal const int Length = 7;

        internal const int PostLeft = 0;
        internal const int PostRight = 1;
        internal const int PostFaderLeft = 2;
        internal const int PostFaderRight = 3;
        internal const int GateIn = 4;
        internal const int CompOut = 5;
        internal const int CompMeterAndGated = 6;
    }

    /// <summary>
    /// Indexes into meter records for Aux and Main outputs.
    /// </summary>
    internal static class AuxMains
    {
        internal const int Length = 5;

        internal const int Post = 0;
        internal const int PostFader = 1;
        internal const int GateIn = 2;
        internal const int CompOut = 3;
        internal const int CompMeterAndGated = 4;
    }
}

namespace DigiMixer.AllenAndHeath.Core;

public enum AHMessageFormat
{
    VariableLength,

    /// <summary>
    /// Total of 8 bytes: the fixed-length indicator and 7 bytes of data
    /// </summary>
    FixedLength8,

    /// <summary>
    /// Total of 9 bytes: the fixed-length indicator and 8 bytes of data
    /// </summary>
    FixedLength9
}

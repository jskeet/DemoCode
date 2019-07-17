using System;
using static System.FormattableString;

namespace VDrumExplorer.Models.Fields
{
    /// <summary>
    /// A volume value between -60DB and +6DB, or "-inf".
    /// </summary>
    public class Volume32Field : FieldBase
    {
        public override FieldValue ParseSysExData(byte[] data)
        {
            int rawValue = GetInt32Value(data);
            string text = rawValue == -601 ? "-INF"
                : rawValue >= -600 && rawValue <= 60 ? Invariant($"{rawValue / 10m}dB")
                : throw new InvalidOperationException($"Invalid volume value: {rawValue}");
            return new FieldValue(text);
        }
    }
}

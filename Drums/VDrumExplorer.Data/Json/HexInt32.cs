using System;
using System.Globalization;

namespace VDrumExplorer.Data.Json
{
    /// <summary>
    /// A wrapper for integer values written in hex.
    /// </summary>
    internal sealed class HexInt32 : IEquatable<HexInt32>
    {
        private readonly string text;
        public int Value { get; }

        public HexInt32(string text)
        {
            this.text = text;
            int value;
            string reducedText;
            int multiplier = 1;
            if (text.StartsWith("0x"))
            {
                reducedText = text.Substring(2);
            }
            else if (text.StartsWith("-0x"))
            {
                reducedText = text.Substring(3);
                multiplier = -1;
            }
            else
            {
                throw new ArgumentException($"Invalid hex string: '{text}'");
            }
            reducedText = reducedText.Replace("_", "");
            if (!int.TryParse(reducedText, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                throw new ArgumentException($"Invalid hex string: '{text}'");
            }

            Value = value * multiplier;
        }

        public override string ToString() => text;
        public bool Equals(HexInt32 other) => other != null && other.text == text;
        public override bool Equals(object obj) => Equals(obj as HexInt32);
        public override int GetHashCode() => text.GetHashCode();
    }
}

// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Globalization;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Json
{
    /// <summary>
    /// <para>
    /// A wrapper for integer values written in hex. The format requires "0x" (or "-0x")
    /// at the start of the string, and can include any number of "_" characters (anywhere after the x).
    /// Both the text and the value are retained, to simplify debugging JSON.
    /// Note that this class doesn't support Int32.MinValue at the moment, because doing so
    /// would complicate the code and we're unlikely to need it.
    /// </para>
    /// <para>
    /// Equality is checked numerically, so "0x100" and "0x1_00" are considered equal.
    /// </para>
    /// </summary>
    internal sealed class HexInt32 : IEquatable<HexInt32?>
    {
        public string Text { get; }
        public int Value { get; }

        private HexInt32(string text, int value) =>
            (Text, Value) = (text, value);

        public static HexInt32 Parse(string text)
        {
            Preconditions.CheckNotNull(text, nameof(text));
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
                throw new FormatException($"Invalid hex string: '{text}'");
            }
            reducedText = reducedText.Replace("_", "");
            if (!int.TryParse(reducedText, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value) ||
                value < 0) // Overflow (as we know the text we've just parsed is not negative)
            {
                throw new FormatException($"Invalid hex string: '{text}'");
            }
            return new HexInt32(text, value * multiplier);
        }

        public override string ToString() => Text;
        public bool Equals(HexInt32? other) => other is object && other.Value == Value;
        public override bool Equals(object? obj) => Equals(obj as HexInt32);
        public override int GetHashCode() => Value.GetHashCode();
    }
}

// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public sealed class StringField : PrimitiveFieldBase
    {
        /// <summary>
        /// The length of the field, in characters.
        /// </summary>
        public int Length { get; }

        private readonly int bytesPerChar;

        internal StringField(FieldBase.Parameters common, int length)
            : base(common)
        {
            Length = length;
            bytesPerChar = Size / length;
        }

        public override string GetText(FixedContainer context, ModuleData data)
        {
            var address = GetAddress(context);
            byte[] rawBytes = data.GetData(address, Size);
            switch (bytesPerChar)
            {
                case 1:
                    return Encoding.ASCII.GetString(rawBytes);
                case 2:
                    byte[] asciiBytes = new byte[Length];
                    for (int i = 0; i < Length; i++)
                    {
                        asciiBytes[i] = (byte) ((rawBytes[i * 2] << 4) | rawBytes[i * 2 + 1]);
                    }
                    return Encoding.ASCII.GetString(asciiBytes);
                default:
                    throw new InvalidOperationException($"Can't get a string with bytesPerChar of {bytesPerChar}");
            }
        }

        public override bool TrySetText(FixedContainer context, ModuleData data, string text)
        {
            var address = GetAddress(context);
            if (text.Length > Length || text.Any(c => c < 32 || c > 126))
            {
                return false;
            }
            text = text.PadRight(Length);
            switch (bytesPerChar)
            {
                case 1:
                    byte[] ascii = Encoding.ASCII.GetBytes(text);
                    data.SetData(address, ascii);
                    break;
                case 2:
                    byte[] rawBytes = new byte[Length * 2];
                    for (int i = 0; i < Length; i++)
                    {
                        rawBytes[i * 2] = (byte) (text[i] >> 4);
                        rawBytes[i * 2 + 1] = (byte) (text[i] & 0xf);
                    }
                    data.SetData(address, rawBytes);
                    break;
                default:
                    throw new InvalidOperationException($"Can't set a string with bytesPerChar of {bytesPerChar}");
            }
            return true;
        }

        public override void Reset(FixedContainer context, ModuleData data) => TrySetText(context, data, new string(' ', Length));

        protected override bool ValidateData(FixedContainer context, ModuleData data, out string? error)
        {
            // We could potentially validate that it contains non-control-character ASCII...
            error = null;
            return true;
        }
    }
}

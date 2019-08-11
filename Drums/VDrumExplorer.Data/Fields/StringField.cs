// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public sealed class StringField : FieldBase, IPrimitiveField
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

        public string GetText(ModuleData data)
        {
            byte[] rawBytes = data.GetData(Address, Size);
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

        public bool TrySetText(ModuleData data, string text)
        {
            if (text.Length > Length || text.Any(c => c < 32 || c > 126))
            {
                return false;
            }
            text = text.PadRight(Length);
            switch (bytesPerChar)
            {
                case 1:
                    byte[] ascii = Encoding.ASCII.GetBytes(text);
                    data.SetData(Address, ascii);
                    break;
                case 2:
                    byte[] rawBytes = new byte[Length * 2];
                    for (int i = 0; i < Length; i++)
                    {
                        rawBytes[i * 2] = (byte) (text[i] >> 4);
                        rawBytes[i * 2 + 1] = (byte) (text[i] & 0xf);
                    }
                    data.SetData(Address, rawBytes);
                    break;
                default:
                    throw new InvalidOperationException($"Can't set a string with bytesPerChar of {bytesPerChar}");
            }
            return true;
        }

        public void Reset(ModuleData data) => TrySetText(data, new string(' ', Length));
    }
}

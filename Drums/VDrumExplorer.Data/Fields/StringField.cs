// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public class StringField : FieldBase, IPrimitiveField
    {
        /// <summary>
        /// The length of the field, in characters.
        /// </summary>
        public int Length { get; }

        private readonly int bytesPerChar;

        public StringField(FieldPath path, ModuleAddress address, int length, int bytesPerChar, string description, FieldCondition? condition)
            : base(path, address, length * bytesPerChar, description, condition)
        {
            this.bytesPerChar = bytesPerChar;
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
                        asciiBytes[i] = (byte) (rawBytes[i * 2] << 4 | rawBytes[i * 2 + 1]);
                    }
                    return Encoding.ASCII.GetString(asciiBytes);
                default:
                    throw new InvalidOperationException($"Can't get a string with bytesPerChar of {bytesPerChar}");
            }

        }
    }
}

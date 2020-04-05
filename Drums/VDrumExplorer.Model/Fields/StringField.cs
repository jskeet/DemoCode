// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Text;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Fields
{
    /// <summary>
    /// A field representing textual data, such as the name of a kit.
    /// </summary>
    public sealed class StringField : FieldBase
    {
        /// <summary>
        /// The length of the field, in characters.
        /// </summary>
        public int Length { get; }

        private readonly int bytesPerChar;

        internal StringField(Parameters common, int length)
            : base(common)
        {
            Length = length;
            bytesPerChar = Size / length;
        }

        public string GetText(FieldContainerData data)
        {
            Span<byte> buffer = stackalloc byte[Size]; 
            data.ReadBytes(Offset, buffer);
            switch (bytesPerChar)
            {
                case 1:
                    return Encoding.ASCII.GetString(buffer);
                case 2:
                    Span<byte> asciiBytes = stackalloc byte[Length];
                    for (int i = 0; i < Length; i++)
                    {
                        asciiBytes[i] = (byte) ((buffer[i * 2] << 4) | buffer[i * 2 + 1]);
                    }
                    return Encoding.ASCII.GetString(asciiBytes);
                default:
                    throw new InvalidOperationException($"Can't get a string with bytesPerChar of {bytesPerChar}");
            }
        }

        internal override bool ValidateData(FieldContainerData data, out string? error)
        {
            // We could potentially validate that it contains non-control-character ASCII...
            error = null;
            return true;
        }
    }
}

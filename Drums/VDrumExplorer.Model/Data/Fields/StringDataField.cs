// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Fields
{
    // TODO: Maybe create a new encoding for 2-byte-per-char?


    public class StringDataField : DataFieldBase<StringField>
    {
        private int Length => SchemaField.Length;
        private int BytesPerChar => SchemaField.BytesPerChar;

        internal StringDataField(StringField field) : base(field)
        {
            text = "";
        }

        public override string FormattedText => Text;

        private string text;
        public string Text
        {
            get => text;
            set
            {
                if (!TrySetText(value))
                {
                    throw new ArgumentException(nameof(value));
                }
            }
        }

        public override void Reset() => Text = "";

        internal override IEnumerable<DataValidationError> Load(DataSegment segment)
        {
            // TODO: Really no validation?
            Span<byte> buffer = stackalloc byte[Size];
            segment.ReadBytes(Offset, buffer);
            switch (BytesPerChar)
            {
                case 1:
                    Text = Encoding.ASCII.GetString(buffer).Trim();
                    break;
                case 2:
                    Span<byte> asciiBytes = stackalloc byte[SchemaField.Length];
                    for (int i = 0; i < SchemaField.Length; i++)
                    {
                        asciiBytes[i] = (byte) ((buffer[i * 2] << 4) | buffer[i * 2 + 1]);
                    }
                    Text = Encoding.ASCII.GetString(asciiBytes).Trim();
                    break;
                default:
                    throw new InvalidOperationException($"Can't get a string with bytesPerChar of {BytesPerChar}");
            }
            return DataValidationError.None;
        }

        private bool TrySetText(string value)
        {
            if (value.Length > Length || value.Any(c => c > 126))
            {
                return false;
            }

            SetProperty(ref text, value);
            return true;
        }

        internal override void Save(DataSegment segment)
        {
            string padded = text.PadRight(Length);
            Span<byte> bytes = stackalloc byte[Size];
            switch (BytesPerChar)
            {
                case 1:
                    Encoding.ASCII.GetBytes(padded.AsSpan(), bytes);
                    break;
                case 2:
                    for (int i = 0; i < Length; i++)
                    {
                        bytes[i * 2] = (byte) (padded[i] >> 4);
                        bytes[i * 2 + 1] = (byte) (padded[i] & 0xf);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Can't set a string with bytesPerChar of {BytesPerChar}");
            }
            segment.WriteBytes(Offset, bytes);
        }
    }
}

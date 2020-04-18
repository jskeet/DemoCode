// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Text;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    // TODO: Maybe create a new encoding for 2-byte-per-char?


    public class StringDataField : DataFieldBase<StringField>
    {
        private int Length => SchemaField.Length;
        private int BytesPerChar => SchemaField.BytesPerChar;

        internal StringDataField(StringField field) : base(field)
        {
            // Load will be called as part of initialization.
            text = null!;
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

        internal override void Load(DataSegment segment)
        {
            Span<byte> buffer = stackalloc byte[Size];
            segment.ReadBytes(Offset, buffer);
            switch (BytesPerChar)
            {
                case 1:
                    text = Encoding.ASCII.GetString(buffer);
                    break;
                case 2:
                    Span<byte> asciiBytes = stackalloc byte[SchemaField.Length];
                    for (int i = 0; i < SchemaField.Length; i++)
                    {
                        asciiBytes[i] = (byte) ((buffer[i * 2] << 4) | buffer[i * 2 + 1]);
                    }
                    text = Encoding.ASCII.GetString(asciiBytes);
                    break;
                default:
                    throw new InvalidOperationException($"Can't get a string with bytesPerChar of {BytesPerChar}");
            }
        }

        internal bool ValidateData(out string? error)
        {
            // We could potentially validate that it contains non-control-character ASCII...
            error = null;
            return true;
        }

        private bool TrySetText(string text)
        {
            if (text.Length > Length || text.Any(c => c > 126))
            {
                return false;
            }

            SetProperty(ref text, text);
            return true;
        }

        internal override void Save(DataSegment segment)
        {
            text = text.PadRight(Length);
            Span<byte> bytes = stackalloc byte[Size];
            switch (BytesPerChar)
            {
                case 1:
                    Encoding.ASCII.GetBytes(text.AsSpan(), bytes);
                    break;
                case 2:
                    for (int i = 0; i < Length; i++)
                    {
                        bytes[i * 2] = (byte) (text[i] >> 4);
                        bytes[i * 2 + 1] = (byte) (text[i] & 0xf);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Can't set a string with bytesPerChar of {BytesPerChar}");
            }
            segment.WriteBytes(Offset, bytes);
        }
    }
}

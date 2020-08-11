﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using System;

namespace VDrumExplorer.Model.Schema.Fields
{
    internal sealed class NumericCodec
    {
        internal static NumericCodec Range8 { get; } = new NumericCodec(1, ReadRange8, WriteRange8, 0, 127);
        internal static NumericCodec Range16 { get; } = new NumericCodec(2, ReadRange16, WriteRange16, -128, 127);
        // Like Range16, but treating it as unsiged.
        internal static NumericCodec URange16 { get; } = new NumericCodec(2, ReadURange16, WriteURange16, 0, 255);
        internal static NumericCodec Full24 { get; } = new NumericCodec(3, ReadFull24, WriteFull24, 0, (1 << 21) - 1);
        internal static NumericCodec Range32 { get; } = new NumericCodec(4, ReadRange32, WriteRange32, short.MinValue, short.MaxValue);

        internal static NumericCodec Fixme32 { get; } = new NumericCodec(4, ReadFixme32, WriteFixme32, -20000, 2000);

        private delegate void Int32Writer(Span<byte> data, int value);
        private delegate int Int32Reader(ReadOnlySpan<byte> data);

        private readonly Int32Writer writer;
        private readonly Int32Reader reader;

        private NumericCodec(int size, Int32Reader reader, Int32Writer writer, int min, int max) =>
            (Size, this.reader, this.writer, Min, Max) = (size, reader, writer, min, max);

        internal int Size { get; }
        internal void WriteInt32(Span<byte> data, int value) => writer(data, value);
        internal int ReadInt32(Span<byte> data) => reader(data);
        internal int Min { get; }
        internal int Max { get; }

        private static int ReadRange8(ReadOnlySpan<byte> data) =>
            data[0];

        private static int ReadRange16(ReadOnlySpan<byte> data) =>
            (sbyte) ((data[0] << 4) | data[1]);

        private static int ReadURange16(ReadOnlySpan<byte> data) =>
            (byte) ((data[0] << 4) | data[1]);

        private static int ReadFull24(ReadOnlySpan<byte> data) =>
            (data[0] << 14) |
            (data[1] << 7) |
            (data[2] << 0);

        private static int ReadRange32(ReadOnlySpan<byte> data) =>
            (short)
            ((data[0] << 12) |
            (data[1] << 8) |
            (data[2] << 4) |
            (data[3] << 0));

        private static void WriteRange8(Span<byte> data, int value)
        {
            data[0] = (byte) value;
        }

        private static void WriteRange16(Span<byte> data, int value)
        {
            data[0] = (byte) ((value >> 4) & 0xf);
            data[1] = (byte) ((value >> 0) & 0xf);
        }

        // Same implementation as WriteRange16, but a separate method for consistency.
        private static void WriteURange16(Span<byte> data, int value)
        {
            data[0] = (byte) ((value >> 4) & 0xf);
            data[1] = (byte) ((value >> 0) & 0xf);
        }

        private static void WriteFull24(Span<byte> data, int value)
        {
            data[0] = (byte) ((value >> 14) & 0x7f);
            data[1] = (byte) ((value >> 7) & 0x7f);
            data[2] = (byte) ((value >> 0) & 0x7f);
        }

        private static void WriteRange32(Span<byte> data, int value)
        {
            data[0] = (byte) ((value >> 12) & 0xf);
            data[1] = (byte) ((value >> 8) & 0xf);
            data[2] = (byte) ((value >> 4) & 0xf);
            data[3] = (byte) ((value >> 0) & 0xf);
        }

        // FIXME: I don't really understand these, but they're the reverb values on the AE-10

        private static int ReadFixme32(ReadOnlySpan<byte> data) =>
            (short)
            ((data[0] & 0x0700_0000 << 12) |
            (data[1] << 8) |
            (data[2] << 4) |
            (data[3] << 0));

        private static void WriteFixme32(Span<byte> data, int value)
        {
            value = value | 0x0800_0000;
            data[0] = (byte) ((value >> 12) & 0xf);
            data[1] = (byte) ((value >> 8) & 0xf);
            data[2] = (byte) ((value >> 4) & 0xf);
            data[3] = (byte) ((value >> 0) & 0xf);
        }
    }
}

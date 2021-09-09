// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Text;

namespace CameraControl.Visca
{
    /// <summary>
    /// Packet within the VISCA protocol.
    /// </summary>
    internal struct ViscaPacket
    {
        // VISCA packets can contain up to 16 bytes. It's simplest to represent those as two long values.
        private readonly long head;
        private readonly long tail;
        
        /// <summary>
        /// The number of bytes in the packet, excluding the trailing 0xFF.
        /// </summary>
        public int Length { get; }

        private ViscaPacket(long head, long tail, int length) =>
            (this.head, this.tail, this.Length) = (head, tail, length);

        internal static ViscaPacket FromBytes(byte[] bytes, int start, int length)
        {
            Preconditions.CheckNotNull(bytes);
            // Start must be within the array
            Preconditions.CheckRange(start, 0, bytes.Length - 1);
            // Packets have 1-16 bytes
            Preconditions.CheckRange(length, 1, 16);
            // The end of the packet must be within the array
            Preconditions.CheckRange(length, 0, bytes.Length - start);

            long head = GetByteOrZero(0, 56) |
                GetByteOrZero(1, 48) |
                GetByteOrZero(2, 40) |
                GetByteOrZero(3, 32) |
                GetByteOrZero(4, 24) |
                GetByteOrZero(5, 16) |
                GetByteOrZero(6, 8) |
                GetByteOrZero(7, 0);

            long tail = GetByteOrZero(8, 56) |
                GetByteOrZero(9, 48) |
                GetByteOrZero(10, 40) |
                GetByteOrZero(11, 32) |
                GetByteOrZero(12, 24) |
                GetByteOrZero(13, 16) |
                GetByteOrZero(14, 8) |
                GetByteOrZero(15, 0);

            return new ViscaPacket(head, tail, length);

            long GetByteOrZero(int index, int shift) =>
                index >= length ? 0L : ((long) bytes[start + index]) << shift;
        }

        // TODO: Make this an indexer?
        internal byte GetByte(int index)
        {
            Preconditions.CheckRange(index, 0, Length);
            return index < 8
                ? (byte) (head >> (56 - (index * 8)))
                : (byte) (tail >> (120 - (index * 8)));
        }

        internal short GetInt16(int index)
        {
            Preconditions.CheckRange(index, 0, Length - 4);
            return (short)(
                (GetByte(index) << 12) |
                (GetByte(index + 1) << 8) |
                (GetByte(index + 2) << 4) |
                (GetByte(index + 3) << 0)
            );
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Length; i++)
            {
                builder.AppendFormat("{0:x2}", GetByte(i));
                builder.Append('-');
            }
            builder.Length--;
            return builder.ToString();
        }
    }
}

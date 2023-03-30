// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    /// <summary>
    /// A read buffer for VISCA packets. 
    /// </summary>
    internal class ReadBuffer
    {
        private int size;
        // This is larger than we need, given that a single packet is only ever 16 bytes at most,
        // but it allows us to read multiple packets in a single Stream.ReadAsync call.
        // We only create a single buffer per client, so the difference between this and allocating
        // only 16 bytes is trivial.
        private readonly byte[] buffer = new byte[256];

        internal void Clear()
        {
            size = 0;
        }

        internal async Task<ViscaPacket> ReadAsync(Stream stream, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (FindEnd() is int count)
                {
                    return Consume(count);
                }
                if (size == buffer.Length)
                {
                    throw new ViscaProtocolException($"Read {size} bytes without a reaching the end of a VISCA packet");
                }
                // The cancellation token in ReadAsync isn't always used, apparently - so we also close the stream
                // if we're cancelled. (We reconnect on any exception anyway, so it shouldn't be a problem to close it.)
                using (var registration = cancellationToken.Register(() => stream.Close()))
                {
                    int bytesRead = await stream.ReadAsync(buffer, size, buffer.Length - size, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        throw new ViscaProtocolException("Reached end of VISCA stream");
                    }
                    size += bytesRead;
                }
            }

            int? FindEnd()
            {
                for (int i = 0; i < size; i++)
                {
                    if (buffer[i] == 0xff)
                    {
                        // Include the trailing byte
                        return i + 1;
                    }
                }
                return null;
            }

            ViscaPacket Consume(int count)
            {
                // Exclude the trailing byte from the packet
                var packet = ViscaPacket.FromBytes(buffer, 0, count - 1);
                size -= count;
                // Shift the content of the array if we need to.
                // (Alternatively, we could construct the packet from middle of the byte array,
                // and only shift when we got near the end of the buffer. It really shouldn't matter much though.)
                if (size != 0)
                {
                    Buffer.BlockCopy(buffer, count, buffer, 0, size);
                }
                return packet;
            }
        }
    }
}

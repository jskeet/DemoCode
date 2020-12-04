// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    // TODO: This should probably contain the stream, and we should just create a new buffer when necessary.

    /// <summary>
    /// A read buffer for VISCA packets. 
    /// </summary>
    internal class ReadBuffer
    {
        private int size;
        private readonly byte[] buffer = new byte[256];

        internal void Clear()
        {
            size = 0;
        }

        internal async Task<byte[]> ReadAsync(Stream stream, CancellationToken cancellationToken)
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
                using (var registration = cancellationToken.Register(() => stream.Close()))
                {
                    int bytesRead = await stream.ReadAsync(buffer, size, buffer.Length - size);
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

            byte[] Consume(int count)
            {
                byte[] ret = new byte[count];
                Buffer.BlockCopy(buffer, 0, ret, 0, count);
                size -= count;
                // Shift the content of the array if we need to
                if (size != 0)
                {
                    Buffer.BlockCopy(buffer, count, buffer, 0, size);
                }
                return ret;
            }
        }
    }
}

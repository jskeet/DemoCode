// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

/// <summary>
/// A read buffer for VISCA packets. 
/// </summary>
internal class ReadBuffer
{
    // This is larger than we need, given that a single packet is only ever 24 bytes at most,
    // but it allows us to read multiple packets in a single Stream.ReadAsync call.
    // We only create a single buffer per client, so the difference between this and allocating
    // only 24 bytes is trivial.
    private readonly byte[] buffer = new byte[256];

    private readonly ViscaMessageFormat format;

    private int size;

    internal void Clear()
    {
        size = 0;
    }

    internal ReadBuffer(ViscaMessageFormat format)
    {
        this.format = format;
    }

    internal async Task<ViscaMessage> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (ViscaMessage.Parse(buffer.AsSpan().Slice(0, size), format) is ViscaMessage message)
            {
                Consume(message.Length);
                return message;
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

        void Consume(int count)
        {
            size -= count;
            // Shift the content of the array if we need to.
            // (Alternatively, we could construct the packet from middle of the byte array,
            // and only shift when we got near the end of the buffer. It really shouldn't matter much though.)
            if (size != 0)
            {
                Buffer.BlockCopy(buffer, count, buffer, 0, size);
            }
        }
    }
}

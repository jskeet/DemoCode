using DigiMixer.DmSeries.Core;
using System.Text;

namespace DigiMixer.DmSeries;

public sealed class DmMproMessage : DmMessage
{
    public DmMproMessage(DmRawMessage rawMessage) : base(rawMessage)
    {
    }

    public DmMproMessage(params Chunk[] chunks) : this(CreateRawMessage(chunks))
    {
    }

    public IEnumerable<Chunk> GetChunks()
    {
        int index = 0;
        while (index < Data.Length)
        {
            var chunk = Chunk.Parse(Data.Slice(index));
            yield return chunk;
            index += chunk.Length;
        }
    }

    private static DmRawMessage CreateRawMessage(params Chunk[] chunks)
    {
        var data = new byte[chunks.Sum(c => c.Length)];
        Span<byte> buffer = data;
        foreach (var chunk in chunks)
        {
            chunk.WriteTo(buffer);
            buffer = buffer.Slice(chunk.Length);
        }
        return new DmRawMessage(MproType, data);
    }

    public sealed class Chunk
    {
        private readonly ReadOnlyMemory<byte> data;

        public ReadOnlySpan<byte> Data => data.Span;

        /// <summary>
        /// Total length of the chunk, including the header.
        /// </summary>
        public int Length => data.Length + 5;

        public Chunk(ReadOnlyMemory<byte> data)
        {
            this.data = data;
        }

        public Chunk(byte[] data) : this(data.AsMemory())
        {
        }

        internal void WriteTo(Span<byte> buffer)
        {
            buffer[0] = 0x11;
            DmRawMessage.WriteInt32(buffer.Slice(1), Data.Length);
            Data.CopyTo(buffer.Slice(5));
        }

        internal static Chunk Parse(ReadOnlySpan<byte> buffer)
        {
            if (buffer[0] != 0x11)
            {
                throw new ArgumentException("Buffer didn't start with 0x11");
            }
            if (buffer.Length < 5)
            {
                throw new ArgumentException("Buffer wasn't long enough to include data length");
            }
            var dataLength = DmRawMessage.ReadInt32(buffer.Slice(1));
            if (buffer.Length < 5 + dataLength)
            {
                throw new ArgumentException("Buffer wasn't long enough to include data");
            }
            return new Chunk(buffer.Slice(5, dataLength).ToArray());
        }

        public static Chunk FromValueAndText(byte[] value, string text)
        {
            // Format:
            // Bytes from value
            // 0x31
            // 4 bytes: text length including null terminator
            // Text
            // Null terminator
            var array = new byte[value.Length + 1 + 4 + text.Length + 1];
            Span<byte> data = array;
            value.CopyTo(data);
            data[value.Length] = 0x31;
            DmRawMessage.WriteInt32(data.Slice(value.Length + 1), text.Length + 1);
            Encoding.ASCII.GetBytes(text, data.Slice(value.Length + 5));
            return new Chunk(array);
        }
    }
}

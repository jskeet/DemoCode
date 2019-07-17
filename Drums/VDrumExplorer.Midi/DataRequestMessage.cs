using MiscUtil.Conversion;
using MiscUtil.IO;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VDrumExplorer.Midi
{
    internal sealed class DataRequestMessage
    {
        private int DeviceId { get; }
        private int ModelId { get; }
        private int Address { get; }
        private int Size { get; }

        internal DataRequestMessage(int deviceId, int modelId, int address, int size) =>
            (DeviceId, ModelId, Address, Size) = (deviceId, modelId, address, size);

        public SysExMessage ToSysExMessage()
        {
            var stream = new MemoryStream();
            var writer = new EndianBinaryWriter(EndianBitConverter.Big, stream);
            writer.Write((byte) 0xf0); // Exclusive status
            writer.Write((byte) 0x41); // Roland
            writer.Write((byte) (DeviceId - 1));
            writer.Write(ModelId);
            writer.Write((byte) 0x11); // Command ID (RQ1)
            writer.Write(Address);
            Write(writer, Size);
            writer.Write((byte) 0); // Checksum placeholder
            writer.Write((byte) 0xf7); // End of exclusive message

            byte[] bytes = stream.ToArray();
            bytes[16] = SysExUtility.CalculateChecksum(bytes, 8, 8);
            return new SysExMessage(bytes);
        }

        private void Write(EndianBinaryWriter writer, int value)
        {
            writer.Write((byte) ((value >> 21) & 0x7f));
            writer.Write((byte) ((value >> 14) & 0x7f));
            writer.Write((byte) ((value >> 7) & 0x7f));
            writer.Write((byte) ((value & 0x7f)));
        }
    }

}

using MiscUtil.Conversion;
using MiscUtil.IO;
using Sanford.Multimedia.Midi;
using System;
using System.IO;

namespace VDrumExplorer.Midi
{
    internal class DataResponseMessage
    {
        public int Address { get; }
        public int DeviceId { get; }
        public int ModelId { get; }
        private readonly byte[] data;

        // Until we know whether or not we need it...
        public byte[] Data => data;
        public int Length => data.Length;
        public byte this[int index] => data[index];

        internal DataResponseMessage(int deviceId, int modelId, int address, byte[] data) =>
            (DeviceId, ModelId, Address, this.data) = (deviceId, modelId, address, data);

        internal static bool TryParse(SysExMessage message, out DataResponseMessage result)
        {
            int length = message.Length;
            byte[] packet = new byte[length];
            message.CopyTo(packet, 0);

            // Expected values
            if (packet[0] != 0xf0 ||
                packet[1] != 0x41 ||
                packet[7] != 0x12 ||
                packet[length - 1] != 0xf7)
            {
                result = null;
                return false;
            }

            byte deviceId = (byte) (packet[2] + 1);
            int modelId = (packet[3] << 24) | (packet[4] << 16) | (packet[5] << 8) | packet[6];
            int address = (packet[8] << 24) | (packet[9] << 16) | (packet[10] << 8) | packet[11];
            byte[] data = new byte[packet.Length - 14];
            Array.Copy(packet, 12, data, 0, data.Length);
            result = new DataResponseMessage(deviceId, modelId, address, data);
            return true;
        }

        public SysExMessage ToSysExMessage()
        {
            var stream = new MemoryStream();
            var writer = new EndianBinaryWriter(EndianBitConverter.Big, stream);
            writer.Write((byte) 0xf0); // Exclusive status
            writer.Write((byte) 0x41); // Roland
            writer.Write((byte) (DeviceId - 1));
            writer.Write(ModelId);
            writer.Write((byte) 0x12); // Command ID (DT1)
            writer.Write(Address);
            writer.Write(data);
            writer.Write((byte) 0); // Checksum placeholder
            writer.Write((byte) 0xf7); // End of exclusive message

            byte[] bytes = stream.ToArray();
            bytes[bytes.Length - 2] = SysExUtility.CalculateChecksum(bytes, 8, 4 + data.Length);
            return new SysExMessage(bytes);
        }
    }
}

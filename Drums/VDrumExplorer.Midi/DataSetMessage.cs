// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Midi
{
    // TODO: Remove? Move to a private class in RolandMidiClient?
    internal sealed class DataSetMessage
    {
        public int Address { get; }
        internal byte RawDeviceId { get; }
        internal int DisplayDeviceId => RawDeviceId + 1;
        public int ModelId { get; }
        private readonly byte[] data;

        // Until we know whether or not we need it...
        public byte[] Data => data;
        public int Length => data.Length;
        public byte this[int index] => data[index];

        internal DataSetMessage(byte rawDeviceId, int modelId, int address, byte[] data) =>
            (RawDeviceId, ModelId, Address, this.data) = (rawDeviceId, modelId, address, data);

        internal static bool TryParse(RawMidiMessage message, out DataSetMessage result)
        {
            var messageData = message.Data;
            int length = messageData.Length;

            // Expected values
            if (messageData.Length < 14 ||
                messageData[0] != 0xf0 ||
                messageData[1] != 0x41 ||
                messageData[7] != 0x12 ||
                messageData[length - 1] != 0xf7)
            {
                result = null;
                return false;
            }

            byte rawDeviceId = messageData[2];
            int modelId = (messageData[3] << 24) | (messageData[4] << 16) | (messageData[5] << 8) | messageData[6];
            int address = (messageData[8] << 24) | (messageData[9] << 16) | (messageData[10] << 8) | messageData[11];
            byte[] data = new byte[messageData.Length - 14];
            Array.Copy(messageData, 12, data, 0, data.Length);
            result = new DataSetMessage(rawDeviceId, modelId, address, data);
            return true;
        }
    }
}

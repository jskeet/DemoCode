// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Diagnostics.CodeAnalysis;

namespace VDrumExplorer.Model.Midi
{
    // TODO: Remove? Move to a private class in RolandMidiClient?
    // TODO: Use logical addresses instead of display addresses?
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

        internal static bool TryParse(MidiMessage message,
            int expectedModelIdLength,
            [NotNullWhen(true)] out DataSetMessage? result)
        {
            var messageData = message.Data;
            int length = messageData.Length;

            // Expected values
            if (messageData.Length < expectedModelIdLength + 10 ||
                messageData[0] != 0xf0 ||
                messageData[1] != 0x41 ||
                messageData[expectedModelIdLength + 3] != 0x12 ||
                messageData[length - 1] != 0xf7)
            {
                result = null;
                return false;
            }

            byte rawDeviceId = messageData[2];

            int modelIdOffset = expectedModelIdLength - 1;

            int modelId =
                (messageData[modelIdOffset] << 24) |
                (messageData[modelIdOffset + 1] << 16) |
                (messageData[modelIdOffset + 2] << 8) |
                messageData[modelIdOffset + 3];

            int address =
                (messageData[modelIdOffset + 5] << 24) |
                (messageData[modelIdOffset + 6] << 16) |
                (messageData[modelIdOffset + 7] << 8) |
                messageData[modelIdOffset + 8];
            byte[] data = new byte[messageData.Length - 10 - expectedModelIdLength];
            Array.Copy(messageData, modelIdOffset + 9, data, 0, data.Length);
            result = new DataSetMessage(rawDeviceId, modelId, address, data);
            return true;
        }
    }
}

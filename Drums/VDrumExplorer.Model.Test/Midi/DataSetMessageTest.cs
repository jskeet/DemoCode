// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Model.Test.Midi
{
    class DataSetMessageTest
    {
        [Test]
        public void TD50XSample()
        {
            var bytes = new byte[] { 0xF0, 0x41, 0x10, 0x00, 0x00, 0x00, 0x00, 0x07, 0x12, 0x00, 0x00, 0x00, 0x00, 0x58, 0x28, 0xF7 };
            var message = new MidiMessage(bytes);
            var parsed = DataSetMessage.TryParse(message, ModuleIdentifier.TD50X.ModelIdLength, out var dataSetMessage);
            Assert.True(parsed);
            Assert.AreEqual(0, dataSetMessage.Address);
            Assert.AreEqual(1, dataSetMessage.Data.Length);
            Assert.AreEqual(0x58, dataSetMessage.Data[0]);
        }
    }
}

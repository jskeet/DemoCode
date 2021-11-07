// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Model.Test.Midi
{
    public class RolandMidiClientTest
    {
        [Test]
        public async Task RequestData_TD50X()
        {
            var input = new FakeMidiInput();
            var output = new FakeMidiOutput();
            var client = new RolandMidiClient(input, output, "TD-50X", "TD-50X", 0x10, ModuleIdentifier.TD50X);
            // Test for "Current kit" - simplest possible message.
            var task = client.RequestDataAsync(0, 1, new CancellationTokenSource(1000).Token);

            // Reply on behalf of the module
            var responseBytes = new byte[] { 0xF0, 0x41, 0x10, 0x00, 0x00, 0x00, 0x00, 0x07, 0x12, 0x00, 0x00, 0x00, 0x00, 0x58, 0x28, 0xF7 };
            var responseMessage = new MidiMessage(responseBytes);

            // We should get the response
            input.SupplyMessage(responseMessage);
            var result = await task;
            Assert.AreEqual(new byte[] { 0x58 }, result);

            // Now check that the request was correct
            Assert.AreEqual(1, output.Messages.Count);
            var requestMessage = output.Messages[0];

            var actualRequestData = requestMessage.Data;
            var expectedRequestData = new byte[]
            {
                0xF0, 0x41, 0x10, // SYSEX, Roland, DevId
                0x00, 0x00, 0x00, 0x00, 0x07, // TD-50X
                0x11, // RQ1
                0x00, 0x00, 0x00, 0x00, // Address
                0x00, 0x00, 0x00, 0x01, // Size
                0x7f, 0xF7 // Checksum and EOX
            };
            Assert.AreEqual(expectedRequestData, actualRequestData);
        }
    }
}

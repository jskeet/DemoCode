// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Sanford.Multimedia.Midi;
using System;

namespace VDrumExplorer.Midi
{
    public class IdentityClient : IDisposable
    {
        private readonly InputDevice input;
        private readonly OutputDevice output;

        public event Action<IdentityResponse> IdentityReceived;

        public IdentityClient(int inputDeviceMidiId, int outputDeviceMidiId)
        {
            Console.WriteLine($"{inputDeviceMidiId} {outputDeviceMidiId}");
            input = new InputDevice(inputDeviceMidiId);
            output = new OutputDevice(outputDeviceMidiId);
            input.SysExMessageReceived += HandleSysExMessage;
            input.StartRecording();
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }

        public void SendRequests()
        {
            for (int deviceId = 0x10; deviceId <= 0x1f; deviceId++)
            {
                byte[] request =
                {
                    0xf0, // Exclusive status
                    0x7e, // ID number (universal non-realtime message)
                    (byte) (deviceId - 1), // Device ID
                    0x06, // General information
                    0x01, // Identity request
                    0xf7  // End of exclusive
                };
                output.Send(new SysExMessage(request));
            }
        }

        private void HandleSysExMessage(object sender, SysExMessageEventArgs e)
        {
            var message = e.Message;
            var data = new byte[message.Length];
            message.CopyTo(data, 0);

            Console.WriteLine(BitConverter.ToString(data));

            if (message.Length == 15 &&
                message[3] == 0x06 && // General information
                message[4] == 0x02 && // Identity reply
                message[5] == 0x41) // Roland
            {
                int deviceId = message[2] + 1;
                int familyCode = message[6] + (message[7] << 8);
                int familyNumberCode = message[8] + (message[9] << 8);
                int revision = message[10] + (message[11] << 8) + (message[12] << 16) + (message[13] << 24);
                var response = new IdentityResponse(deviceId, familyCode, familyNumberCode, revision);
                IdentityReceived?.Invoke(response);
            }
        }
    }
}

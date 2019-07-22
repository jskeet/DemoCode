// Copyright 2019 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using MiLight.Net.Api;
using MiLight.Net.Commands;
using System.Net;
using System.Net.Sockets;

namespace Shed.DrumKitLights
{
    public sealed class LightingController
    {
        private readonly IPEndPoint endPoint;
        private readonly UdpClient udpClient;

        internal LightingController(string host = "192.168.1.255", int port = 8899)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            udpClient = new UdpClient();
        }

        public void On()
        {
            Send(Colour.On(Zone.All));
        }

        public void Off()
        {
            Send(Colour.Off(Zone.All));
        }

        public void Brightness(int brightness)
        {
            Send(Colour.SetBrightness(brightness));
        }

        public void Hue(int hue)
        {
            Send(Colour.Hue(hue));
        }

        public void White()
        {
            Send(Colour.SetWhite(Zone.All));
        }

        // There's no MiLight controller package yet, so let's just inline the
        // socket code here...
        private void Send(byte[] command)
        {
            udpClient.Send(command, command.Length, endPoint);            
        }
    }
}

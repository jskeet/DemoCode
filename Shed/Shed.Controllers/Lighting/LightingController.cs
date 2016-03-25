// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using MiLight.Net.Api;
using MiLight.Net.Commands;
using Shed.Controllers.Reflection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Shed.Controllers.Lighting
{
    [Description("Lights")]
    public sealed class LightingController
    {
        private readonly IPEndPoint endPoint;

        internal LightingController(string host, int port = 8899)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        [Description("Turns the lights on")]
        public void On()
        {
            Send(Colour.On(Zone.All));
        }

        [Description("Turns the lights off")]
        public void Off()
        {
            Send(Colour.Off(Zone.All));
        }

        // There's no MiLight controller package yet, so let's just inline the
        // socket code here...
        private void Send(byte[] command)
        {
            // We should be able to use UdpClient. Gah.
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                var args = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };
                args.SetBuffer(command, 0, command.Length);
                using (var done = new ManualResetEvent(false))
                {
                    args.Completed += (sender, e) => done.Set();
                    if (!socket.SendToAsync(args))
                    {
                        throw new Exception("Failed to send to lighting host");
                    }
                    if (!done.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        throw new Exception("Timed out sending to lighting host");
                    }
                }
            }
        }
    }
}

// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using Shed.Controllers.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Shed.Controllers.Onkyo
{
    [Description("Amp")]
    public sealed class OnkyoController
    {
        private static readonly ImmutableDictionary<string, string> Sources = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
        {
            { "ps4", "02" },
            { "sonos", "23" },
            { "pi", "05" }
        }.ToImmutableDictionary();

        private readonly string host;
        private readonly int port;

        public OnkyoController(string host, int port = 60128)
        {
            this.host = host;
            this.port = port;
        }

        [Description("Turns the amplifier on")]
        public void On()
        {
            SendCommand("PWR01");
        }

        [Description("Turns the amplifier off")]
        public void Off()
        {
            SendCommand("PWR00");
        }

        [Description("Selects the given source")]
        public void Source(string source)
        {
            SendCommand("SLI" + Sources[source]);
        }

        [Description("Sets the master volume")]
        public void SetVolume(int percentage)
        {
            if (percentage < 0 || percentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "Percentage should be in range [0, 100]");
            }
            SendCommand($"MVL{percentage:X2}");
        }

        private void SendCommand(string command)
        {
            var data = GetCommandBytes(command);
            // Would love to use TcpClient here...
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var serverAddr = IPAddress.Parse(this.host);
                var endPoint = new IPEndPoint(serverAddr, this.port);
                var args = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };
                args.SetBuffer(data, 0, data.Length);
                using (var done = new ManualResetEvent(false))
                {
                    // When there's a buffer present, ConnectAsync sends the data as well
                    args.Completed += (sender, e) => done.Set();
                    if (!socket.ConnectAsync(args))
                    {
                        throw new Exception("Failed to send command to amplifier");
                    }
                    if (!done.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        throw new Exception("Timed out send command to amplifier");
                    }
                }
            }
        }

        private byte[] GetCommandBytes(string command)
        {
            const string Prefix = "!1";
            const string Suffix = "\u000d";
            string message = Prefix + command + Suffix;
            int size = message.Length;
            byte[] data = new byte[size + 16];
            Encoding.ASCII.GetBytes("ISCP", 0, 4, data, 0);
            data[7] = 0x10; // Header size
            // Copy the size in big-endian notation
            data[8] = (byte) (size >> 24);
            data[9] = (byte) (size >> 16);
            data[10] = (byte) (size >> 8);
            data[11] = (byte) (size);
            data[12] = 1; // Version
            Encoding.ASCII.GetBytes(message, 0, message.Length, data, 16);
            return data;
        }
    }
}

using DigiMixer.Core;
using DigiMixer.QuSeries.Core;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.QuSeries.Scratchpad;

internal class ConnectClient
{
    static async Task Main()
    {
        var address = IPAddress.Parse("192.168.1.60");
        var udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        var localUdpPort = ((IPEndPoint) udpClient.Client.LocalEndPoint!).Port;
        udpClient.Close();

        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, localUdpPort));

        bool finished = false;

        var tcpClient = new TcpClient { NoDelay = true };
        tcpClient.Connect(address, 51326);

        int? mixerUdpPort = null;
        var action = new Action<QuControlMessage>(LogMessage) + new Action<QuControlMessage>(AssignUdpPort);

        // We send keepalive messages via UDP.
        var udpLoop = StartUdpLoop();
        var loop = StartLoop(action);
        var stream = tcpClient.GetStream();

        //var message1 = QuMessage.Create(type: 0, new byte[] { (byte) (port & 0xff), (byte) (port >> 8) });
        //message1.WriteTo(stream);

        //await Task.Delay(100);
        //var message2 = QuMessage.Create(type: 4, Decode("00 01"));
        //message2.WriteTo(stream);


        var introMessages = new[]
        {
            QuControlMessage.Create(type: 0, new byte[] { (byte) (localUdpPort & 0xff), (byte) (localUdpPort >> 8) }),
            // This is required in order to get notifications from other clients.
            QuControlMessage.Create(type: 4, Decode("13 00 00 00  ff ff ff ff ff ff 9f 0f", "00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00", "00 e0 03 c0 ff ff ff 7f")),
            //QuMessage.Create(type: 4, Decode("14 00 00 00  ff 00 00 00 00 00 00 00", "00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00", "00 00 00 00 00 00 00 00")),
            //QuMessage.Create(type: 4, Decode("15 00 00 00  fa c1 23 06 04 00 00 28", "d1 04 1c 00 00 00 00 00  00 00 00 00 00 00 00 00", "00 00 00 00 00 00 00 00")),
            /*
            QuMessage.Create(type: 4, Decode("0d 00")),
            QuMessage.Create(type: 4, Decode("06 00")),
            QuMessage.Create(type: 4, Decode("0c 00")),
            QuMessage.Create(type: 4, Decode("0b 00")),
            QuMessage.Create(type: 4, Decode("08 00")),
            // This requests full data (~25K)
            QuMessage.Create(type: 4, Decode("02 00")),
            // Type 2 response to this, just 01 00
            QuMessage.Create(type: 4, Decode("01 00 00 00  09 0e 64 90")),
            // No response to this
            QuMessage.Create(type: 4, Decode("0f 00")),
            QuMessage.Create(type: 4, Decode("07 00")),*/
            //QuMessage.Create(type: 4, Decode("02 00")),
            QuControlMessage.Create(type: 4, Decode("02 00")),
        };

        foreach (var message in introMessages)
        {
            stream.Write(message.ToByteArray());
            await Task.Delay(100);
        }

        /*
        
        var p1 = QuMessage.Create(type: 4, Decode("13 00 00 00  ff ff ff ff ff ff 9f 0f", "00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00", "00 e0 03 c0 ff ff ff 7f"));
        var p2 = QuMessage.Create(type: 4, Decode("14 00 00 00  ff 00 00 00 00 00 00 00", "00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00", "00 00 00 00 00 00 00 00"));
        var p3 = QuMessage.Create(type: 4, Decode("15 00 00 00  fa c1 23 06 04 00 00 28", "d1 04 1c 00 00 00 00 00  00 00 00 00 00 00 00 00", "00 00 00 00 00 00 00 00"));

        p1.WriteTo(stream);
        p2.WriteTo(stream);
        p3.WriteTo(stream);
        
        //await Task.Delay(1000);
        var message3 = QuMessage.Create(type: 4, Decode("0d 00"));
        message3.WriteTo(stream);

        var message4 = QuMessage.Create(type: 4, Decode("08 00"));
        message4.WriteTo(stream);

        //await Task.Delay(200);
        var message5 = QuMessage.Create(type: 4, Decode("02 00"));
        message5.WriteTo(stream);
        */

        await Task.Delay(1000);
        finished = true;

        async Task StartLoop(Action<QuControlMessage> action)
        {
            var messageBuffer = new MessageProcessor<QuControlMessage>(
                QuControlMessage.TryParse,
                message => message.Length,
                action,
                100_000);
            byte[] buffer = new byte[1024];
            Console.WriteLine($"Starting reading at {DateTime.UtcNow}");
            while (!finished)
            {
                var bytesRead = await tcpClient.GetStream().ReadAsync(buffer, 0, 1024);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"Receiving stream broken at {DateTime.UtcNow}");
                    return;
                }
                messageBuffer.Process(buffer.AsSpan().Slice(0, bytesRead));
            }
        }

        async Task StartUdpLoop()
        {
            var pingGap = TimeSpan.FromSeconds(4);
            var nextPing = DateTime.UtcNow + pingGap;
            while (!finished)
            {
                var result = await udpClient.ReceiveAsync();

                //Console.WriteLine($"Received UDP packet, {result.Buffer.Length} bytes");
                var now = DateTime.UtcNow;
                if (now > nextPing)
                {
                    if (mixerUdpPort is null)
                    {
                        continue;
                    }
                    var endpoint = new IPEndPoint(address, mixerUdpPort.Value);
                    Console.WriteLine("Sending ping");
                    await udpClient.SendAsync(new byte[] { 0x7f, 0x25, 0, 0 }, 4, endpoint);
                    nextPing = now + pingGap;
                }
            }
        }

        void AssignUdpPort(QuControlMessage message)
        {
            if (message is not QuGeneralMessage { Type: 0 } qgp ||
                qgp.Data.Length != 2)
            {
                return;
            }
            mixerUdpPort = qgp.Data[0] + (qgp.Data[1] << 8);
            Console.WriteLine($"Mixer UDP port: {mixerUdpPort}");
        }
    }

    static void LogMessage(QuControlMessage message) => Console.WriteLine($"Received: {message}");

    private static byte[] Decode(params string[] hexData)
    {
        var allHex = string.Join("", hexData).Replace(" ", "");
        byte[] data = new byte[allHex.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Convert.ToByte(allHex.Substring(i * 2, 2), 16);
        }
        return data;
    }
}

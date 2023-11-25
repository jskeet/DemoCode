using DigiMixer.CqSeries.Core;
using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace DigiMixer.CqSeries.Tools;

public class StartClient : Tool
{
    // Assume the address...
    private const string Address = "192.168.1.85";
    private const int Port = 51326;

    public override async Task<int> Execute()
    {
        var loggingFactory = LoggerFactory.Create(builder => builder.AddConsole().AddSimpleConsole(options => options.SingleLine = true)
            .SetMinimumLevel(LogLevel.Trace));

        var meterClient = new CqMeterClient(NullLogger.Instance /* loggingFactory.CreateLogger("Meter")*/);
        var controlClient = new CqControlClient(NullLogger.Instance/*loggingFactory.CreateLogger("Control")*/, Address, Port);

        IPEndPoint? mixerUdpEndpoint = null;
        controlClient.MessageReceived += (sender, message) =>
        {
            if (message is CqHandshakeMessage handshake)
            {
                mixerUdpEndpoint = new IPEndPoint(IPAddress.Parse(Address), handshake.UdpPort);
            }
            else if (message is CqRegularMessage mess && (mess.X == 17 || mess.X == 34))
            {
            }
            else
            {
                Console.WriteLine(message);
            }
        };

        meterClient.Start();
        await controlClient.Connect(default);
        controlClient.Start();

        var handshake = new CqHandshakeMessage(meterClient.LocalUdpPort);
        await controlClient.SendAsync(handshake, default);

        await Task.Delay(100);

        await controlClient.SendAsync(new CqUnknownMessage(CqMessageFormat.VariableLength, CqMessageType.Type1, []), default);
        await Task.Delay(100);
        await controlClient.SendAsync(new CqUnknownMessage(CqMessageFormat.VariableLength, CqMessageType.Type12, [0x02, 0x00]), default);


        while (true)
        {
            await Task.Delay(3000);
            if (mixerUdpEndpoint is IPEndPoint target)
            {
                await meterClient.SendKeepAliveAsync(target, default);
            }
            await Task.Delay(500);
            await controlClient.SendAsync(new CqAllDataRequestMessage(), default);
        }

        //controlClient.Dispose();
        //return 0;
    }
}

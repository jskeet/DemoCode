using DigiMixer.BehringerWing.Core;
using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DigiMixer.BehringerWing.Tools;

internal class ShowMeters : Tool
{
    // Assume the address...
    private const string Address = "192.168.1.74";
    private const int Port = 2222;

    public async override Task<int> Execute()
    {
        Console.Clear();
        var loggingFactory = LoggerFactory.Create(builder => builder.AddConsole().AddSimpleConsole(options => options.SingleLine = true)
            .SetMinimumLevel(LogLevel.Debug));

        var controlClient = new WingClient(loggingFactory.CreateLogger("Control"), Address, Port);
        await controlClient.Connect(default);
        controlClient.Start();

        var meterClient = new WingMeterClient(loggingFactory.CreateLogger("Meters"));
        meterClient.MessageReceived += PrintMeter;
        meterClient.Start();

        var request = new WingMeterRequest(UdpPort: null, ReportId: 0x12345678, [new WingMeterRequest.ChannelList(WingMeterType.InputChannelV2, [1])]);
        await controlClient.SendMeterRequest(request with { UdpPort = meterClient.LocalUdpPort }, default);

        while (true)
        {
            await Task.Delay(4000);
            await controlClient.SendMeterRequest(request, default);
        }
    }

    private void PrintMeter(object? sender, WingMeterMessage message)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"Total values: {message.Data.Count}");
        Console.WriteLine();
        Console.WriteLine("Input channel 1");
        var channelV2 = message.GetChannelV2(0, 0);
        Console.WriteLine($"Input:  {channelV2.InputLeft,16} {channelV2.InputRight,16}");
        Console.WriteLine($"Output: {channelV2.OutputLeft,16} {channelV2.OutputRight,16}");

        Console.WriteLine("Main 1");
        channelV2 = message.GetChannelV2(1, 0);
        Console.WriteLine($"Input:  {channelV2.InputLeft,16} {channelV2.InputRight,16}");
        Console.WriteLine($"Output: {channelV2.OutputLeft,16} {channelV2.OutputRight,16}");
    }
}

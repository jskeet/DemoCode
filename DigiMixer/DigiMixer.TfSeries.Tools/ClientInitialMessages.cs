using DigiMixer.Diagnostics;
using DigiMixer.Yamaha.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.TfSeries.Tools;

public class ClientInitialMessages : Tool
{
    private const string Host = "192.168.1.96";
    private const int Port = 50368;

    private static readonly YamahaMessage Message1 = ConvertHexToMessage(
        "4d 50 52 4f 00 00 00 1d  11 00 00 00 18 01 01 01",
        "02 31 00 00 00 09 50 72  6f 70 65 72 74 79 00 11",
        "00 00 00 01 80");

    private static readonly YamahaMessage Message2 = ConvertHexToMessage(
        "4d 50 52 4f 00 00 00 47  11 00 00 00 42 01 10 01",
        "04 11 00 00 00 01 00 31  00 00 00 09 50 72 6f 70",
        "65 72 74 79 00 11 00 00  00 10 3a 7c 8d 4c 85 f8",
        "9f 1e aa 83 4f 96 63 0c  ec 3d 11 00 00 00 10 8b",
        "76 f3 98 78 64 6e 83 15  f5 81 7c 06 cc b6 91");

    private static readonly YamahaMessage Message3 = ConvertHexToMessage(
        "4d 50 52 4f 00 00 00 09 11  00 00 00 04 01 04 01 00");

    public override async Task<int> Execute()
    {
        YamahaClient client = new(NullLogger.Instance, Host, Port, HandleMessage);
        await client.Connect(default);
        client.Start();

        await Send(Message1);
        await Send(Message2);
        await Send(Message3);
        await Task.Delay(10000);
        return 0;

        Task HandleMessage(YamahaMessage message, CancellationToken cancellationToken)
        {
            message.DisplayStructure("<=", DecodingOptions.Simple, Console.Out);
            return Task.CompletedTask;
        }

        async Task Send(YamahaMessage message)
        {
            message.DisplayStructure("=>", DecodingOptions.Simple, Console.Out);
            await client.SendAsync(message, default);
        }
    }

    private static YamahaMessage ConvertHexToMessage(params string[] hexLines)
    {
        var hex = string.Join("", hexLines);
        byte[] data = Hex.ParseHex(hex);
        if (YamahaMessage.TryParse(data) is not YamahaMessage message)
        {
            throw new ArgumentException("Couldn't parse message");
        }
        if (data.Length != message.Length)
        {
            throw new ArgumentException($"Didn't use all of the bytes ({data.Length} vs {message.Length}");
        }
        return message;
    }
}

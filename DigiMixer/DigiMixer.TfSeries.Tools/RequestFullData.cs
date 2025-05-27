using DigiMixer.Diagnostics;
using DigiMixer.Yamaha.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.TfSeries.Tools;

public class RequestFullData : Tool
{
    private const string Host = "192.168.1.96";
    private const int Port = 50368;

    private static readonly YamahaMessage FullData = new(YamahaMessageType.MMIX, 0x01010102,
        [new YamahaTextSegment("Mixing"), YamahaBinarySegment.Empty]);

    private static readonly YamahaMessage NoHashes = new(YamahaMessageType.MMIX, 0x01100104,
        [new YamahaBinarySegment([00]), new YamahaTextSegment("Mixing"), YamahaBinarySegment.Zero16, YamahaBinarySegment.Zero16]);

    public override async Task<int> Execute()
    {
        YamahaClient client = new(NullLogger.Instance, Host, Port, HandleMessage);
        await client.Connect(default);
        client.Start();

        await Send(FullData);
        await Send(NoHashes);
        await Task.Delay(10000);
        return 0;

        Task HandleMessage(YamahaMessage message, CancellationToken cancellationToken)
        {
            message.DisplayStructure("<=", DecodingOptions.Simple,Console.Out);
            return Task.CompletedTask;
        }

        async Task Send(YamahaMessage message)
        {
            message.DisplayStructure("=>", DecodingOptions.Simple, Console.Out);
            await client.SendAsync(message, default);
        }
    }
}

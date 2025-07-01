using DigiMixer.Diagnostics;
using DigiMixer.Yamaha;
using DigiMixer.Yamaha.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.TfSeries.Tools;

/// <summary>
/// Tool expected to change over time:
/// - initializes a client
/// - sends experiment-specific messages
/// - sends keepalives automatically
/// </summary>
public class ClientExperimentation : Tool
{
    private const string TfRackHost = "192.168.1.96";
    private const string Dm3Host = "192.168.1.86";
    private const int Port = 50368;

    public override async Task<int> Execute()
    {
        List<YamahaMessage> messages =
        [
            new YamahaMessage(YamahaMessageType.MMIX, 1, RequestResponseFlag.Request, [new YamahaTextSegment("Mixing"), new YamahaBinarySegment([0x80])]),
            new MonitorDataMessage(YamahaMessageType.MMIX, RequestResponseFlag.Request).RawMessage,
            new SyncHashesMessage(YamahaMessageType.MMIX, "Mixing", 0x10, RequestResponseFlag.Request, new byte[16], new byte[16]).RawMessage,
        ];

        TimeSpan delay = TimeSpan.FromSeconds(1);
        DecodingOptions decodingOptions = new(SkipKeepAlive: true, DecodeSchema: false, DecodeData: false, ShowAllSegments: true);
        YamahaClient client = null!;
        client = new(NullLogger.Instance, TfRackHost, Port, HandleMessage);
        await client.Connect(default);
        client.Start();

        foreach (var message in messages)
        {
            await Send(message);
            await Task.Delay(delay);
            await SendWrapped(KeepAliveMessage.Request);
            await Task.Delay(delay);
        }

        while (true)
        {
            await Task.Delay(delay);
            await SendWrapped(KeepAliveMessage.Request);
        }

        async Task HandleMessage(YamahaMessage message, CancellationToken cancellationToken)
        {
            message.DisplayStructure("<=", decodingOptions, Console.Out);

            // Respond to any messages we get, if we can.
            if (message.RequestResponse == RequestResponseFlag.Request)
            {
                var response = GetResponse(message);
                if (response is not null)
                {
                    await Send(response);
                }
            }
        }

        YamahaMessage? GetResponse(YamahaMessage request) => WrappedMessage.TryParse(request) switch
        {
            SyncHashesMessage shm => new SyncHashesMessage(request.Type, shm.Subtype, request.Flag1, RequestResponseFlag.Request, new byte[16], new byte[16]).RawMessage,
            //SyncHashesMessage { RawMessage: var raw } shm => new SyncHashesMessage(raw.Type, shm.Subtype, raw.Flag1, RequestResponseFlag.Response, shm.DataHash, new byte[16]).RawMessage,
            KeepAliveMessage or MonitorDataMessage => request.AsResponse(),
            _ => null
        };

        async Task Send(YamahaMessage message)
        {
            message.DisplayStructure("=>", decodingOptions, Console.Out);
            await client.SendAsync(message, default);
        }

        async Task SendWrapped(WrappedMessage message)
        {
            message.RawMessage.DisplayStructure("=>", decodingOptions, Console.Out);
            await client.SendAsync(message, default);
        }
    }
}

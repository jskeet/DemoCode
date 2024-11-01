using DigiMixer.BehringerWing.Core;
using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigiMixer.BehringerWing.Tools;

internal class StartClient(string JsonFile) : Tool
{
    // Assume the address...
    private const string Address = "192.168.1.74";
    private const int Port = 2222;

    public override async Task<int> Execute()
    {
        string json = File.ReadAllText(JsonFile);
        var descriptions = JsonConvert.DeserializeObject<Dictionary<string, uint>>(json)!;

        var namesByNodeHash = descriptions.ToDictionary(pair => pair.Value, pair => pair.Key);

        var loggingFactory = LoggerFactory.Create(builder => builder.AddConsole().AddSimpleConsole(options => options.SingleLine = true)
            .SetMinimumLevel(LogLevel.Debug));

        var client = new WingClient(loggingFactory.CreateLogger("Wing"), Address, Port);

        uint nodeHash = 0;
        string nodeName = "";
        client.AudioEngineTokenReceived += (sender, token) =>
        {
            //Console.WriteLine($"Received {token.Type}");
            switch (token.Type)
            {
                case WingTokenType.FalseOffZero:
                    Console.WriteLine($"{nodeName}: false/off/0");
                    break;
                case WingTokenType.TrueOnOne:
                    Console.WriteLine($"{nodeName}: true/on/1");
                    break;
                case WingTokenType.Int16:
                    Console.WriteLine($"{nodeName}: {token.Int16Value} (int16)");
                    break;
                case WingTokenType.NodeIndex:
                    Console.WriteLine($"Node index: {token.NodeIndex}");
                    break;
                case WingTokenType.String:
                    Console.WriteLine($"{nodeName}: {token.StringValue}");
                    break;
                case WingTokenType.NodeName:
                    Console.WriteLine($"Node name: {token.NodeName}");
                    break;
                case WingTokenType.Int32:
                    Console.WriteLine($"{nodeName}: {token.Int32Value} (int32)");
                    break;
                case WingTokenType.Float32:
                    Console.WriteLine($"{nodeName}: {token.Float32Value}");
                    break;
                case WingTokenType.RawFloat32:
                    Console.WriteLine($"{nodeName}: {token.Float32Value} (raw)");
                    break;
                case WingTokenType.NodeHash:
                    nodeHash = token.NodeHash;
                    nodeName = namesByNodeHash.GetValueOrDefault(nodeHash) ?? nodeHash.ToString("x8");
                    break;
                case WingTokenType.Toggle:
                    Console.WriteLine($"{nodeName}: Toggle");
                    break;
                case WingTokenType.Step:
                    Console.WriteLine($"{nodeName}: Step {token.Step}");
                    break;
                case WingTokenType.RootNode:
                    Console.WriteLine($"Root node");
                    break;
                case WingTokenType.ParentNode:
                    Console.WriteLine($"Parent node");
                    break;
                case WingTokenType.DataRequest:
                    Console.WriteLine($"Data request");
                    break;
                case WingTokenType.DefinitionRequest:
                    Console.WriteLine($"Node definition request");
                    break;
                case WingTokenType.EndOfRequest:
                    Console.WriteLine($"End of request");
                    break;
                case WingTokenType.NodeDefinition:
                    Console.WriteLine($"Node definition");
                    break;
                default:
                    Console.WriteLine($"Received unknown node type {token.Type}");
                    break;
            }
        };

        await client.Connect(default);
        client.Start();

        //await client.SendAudioEngineTokens([WingToken.RootNode, WingToken.DataRequest], default);
        await client.SendAudioEngineTokens([WingToken.ForNodeHash(2273546959), WingToken.DataRequest], default);

        bool mute = true;
        while (true)
        {
            await Task.Delay(500);
            //await client.SendAudioEngineTokens([WingToken.RootNode], default);
            await client.SendAudioEngineTokens([WingToken.ForNodeHash(4111428088), WingToken.ForBool(mute)], default);
            mute = !mute;
        }

        //return 0;
    }
}

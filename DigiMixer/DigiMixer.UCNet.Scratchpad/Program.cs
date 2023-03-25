using DigiMixer.UCNet.Core;
using DigiMixer.UCNet.Core.Messages;
using Microsoft.Extensions.Logging;

var factory = LoggerFactory.Create(builder => builder.AddConsole().AddSystemdConsole(options => { options.UseUtcTimestamp = true; options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"; })
    .SetMinimumLevel(LogLevel.Trace));
var logger = factory.CreateLogger("UCNetClient");

var client = new UCNetClient(logger, "192.168.1.61", 53000);
var listener = new UCNetMeterListener(logger);
listener.Start();

client.MessageReceived += (sender, message) => logger.LogInformation("Received {type} with mode {mode}", message.Type, ((uint) message.Mode).ToString("x8"));
//client.MessageReceived += MaybeFormatJson;

await client.Connect(default);
client.Start();

await client.Send(new UdpMetersMessage(listener.LocalPort), default);
var subscribe = JsonMessage.FromObject(new SubscribeBody(), MessageMode.ClientRequest);
await client.Send(subscribe, default);
var presets = new FileRequestMessage("Listpresets/channel", 1000);
await client.Send(presets, default);


while (true)
{
    await Task.Delay(2000);
    await client.Send(new KeepAliveMessage(), default);
}

/*
var client = new UCNetClient(logger, "192.168.1.61", 53000);P
var task = client.Start();
task = task.ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.NotOnRanToCompletion);
//var hello = new HelloPacket(53002);
//await client.SendPacket(hello, default);
var subscribe = JsonMessage.FromObject(new SubscribeBody());
await client.Send(subscribe, default);

//var presets = new FileRequestPacket("Listpresets/channel", 1000);
//await client.SendPacket(presets, default);

while (true)
{
    await Task.Delay(2000);
    await client.Send(new KeepAliveMessage(), default);
}
*/

/*
void MaybeFormatJson(object? sender, UCNetMessage message)
{
    if (message is not CompressedJsonMessage cjm)
    {
        return;
    }
    string json = cjm.ToJson();
    JObject jobj = JObject.Parse(json);
    Console.WriteLine(jobj.ToString());
}*/
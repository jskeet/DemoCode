using DigiMixer.UiHttp;
using Microsoft.Extensions.Logging;

// Note: it takes about 100 milliseconds to get all the info.
var factory = LoggerFactory.Create(builder => builder.AddConsole().AddSystemdConsole(options => { options.UseUtcTimestamp = true; options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"; }).SetMinimumLevel(LogLevel.Trace));
var logger = factory.CreateLogger("Scratchpad");

//var mixer = OscMixerApi.ForUdp(logger, "192.168.1.41", 10024);
//await mixer.Connect();
//var config = await mixer.DetectConfiguration();
Console.WriteLine("Connecting...");
var mixer = new UiHttpMixerApi(logger, "192.168.1.57", 80);
//mixer.RegisterReceiver(new ConsoleReceiver());
await mixer.Connect(default);
await Task.Delay(3000);
logger.LogInformation("About to request all info");
await mixer.RequestAllData(null!);
await Task.Delay(3000);
/*
for (int i = 0; i < 100; i++)
{
    await mixer.SendKeepAlive();
    await mixer.SetMuted(new InputChannelId(1), (i & 1) == 1);
    await Task.Delay(3000);
}*/

/*
var inputs = Enumerable.Range(1, 16).Select(id => new InputChannelId(id)).Append(XAir.AuxInput);
var outputs = Enumerable.Range(1, 6).Select(id => new OutputChannelId(id)).Append(XAir.MainOutput);

foreach (var input in inputs)
{
    await mixer.RequestName(input);
    await mixer.RequestMuteStatus(input);
}

foreach (var output in outputs)
{
    await mixer.RequestName(output);
    await mixer.RequestMuteStatus(output);
    await mixer.RequestFaderLevel(output);
}

for (int i = 0; i < 10; i++)
{
    await mixer.RequestChannelUpdates();
    await Task.Delay(5000);
}*/
using DigiMixer;
using DigiMixer.Osc;
using DigiMixer.Scratchpad;
using DigiMixer.UiHttp;
using OscMixerControl;

//var mixer = OscMixerApi.ForUdp("192.168.1.41", 10024);
var mixer = UiHttpMixerApi.Start("192.168.1.57", 80);
await Task.Delay(500);
//mixer.RegisterReceiver(new ConsoleReceiver());
for (int i = 0; i < 100; i++)
{
    await mixer.RequestMixerInfo();
    await mixer.SendAlive();
    await Task.Delay(800);
}

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
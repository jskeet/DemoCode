using DigiMixer;
using DigiMixer.Osc;
using DigiMixer.Scratchpad;
using OscMixerControl;

var mixer = OscMixerApi.ForUdp("192.168.1.41", 10024);
mixer.RegisterReceiver(new ConsoleReceiver());

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
}

for (int i = 0; i < 10; i++)
{
    await mixer.RequestChannelUpdates();
    await Task.Delay(5000);
}
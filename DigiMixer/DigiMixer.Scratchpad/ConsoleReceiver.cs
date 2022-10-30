using DigiMixer.Core;

namespace DigiMixer.Scratchpad;

internal class ConsoleReceiver : IMixerReceiver
{
    public void ReceiveChannelName(ChannelId channelId, string name) =>
        Log($"{channelId} has name {name}");

    public void ReceiveFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        Log($"Fader level {inputId.Value}/{outputId.Value} has level {level.Value}");

    public void ReceiveFaderLevel(ChannelId outputId, FaderLevel level) =>
        Log($"Fader level {outputId.Value} has level {level.Value}");

    public void ReceiveMeterLevels((ChannelId channelId, MeterLevel level)[] levels)
    {
    }

    public void ReceiveMixerInfo(MixerInfo info) =>
        Log($"Info: Name: {info.Name}; Model: {info.Model}; Version: {info.Version}");

    public void ReceiveMuteStatus(ChannelId channelId, bool muted) =>
        Log($"{channelId} mute status is now {muted}");

    private void Log(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {message}");
}

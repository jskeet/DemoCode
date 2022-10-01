namespace DigiMixer.Scratchpad;

internal class ConsoleReceiver : IMixerReceiver
{
    public void ReceiveChannelName(InputChannelId channelId, string name) =>
        Log($"Input channel {channelId.Value} has name {name}");

    public void ReceiveChannelName(OutputChannelId channelId, string name) =>
        Log($"Output channel {channelId.Value} has name {name}");

    public void ReceiveFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level) =>
        Log($"Fader level {inputId.Value}/{outputId.Value} has level {level.Value}");

    public void ReceiveFaderLevel(OutputChannelId outputId, FaderLevel level) =>
        Log($"Fader level {outputId.Value} has level {level.Value}");

    public void ReceiveMeterLevel(OutputChannelId channel, MeterLevel level)
    {
    }

    public void ReceiveMeterLevel(InputChannelId channel, MeterLevel level)
    {
    }

    public void ReceiveMixerInfo(MixerInfo info) =>
        Log($"Info: Name: {info.Name}; Model: {info.Model}; Version: {info.Version}");

    public void ReceiveMuteStatus(InputChannelId channelId, bool muted) =>
        Log($"Input channel {channelId.Value} mute status is now {muted}");

    public void ReceiveMuteStatus(OutputChannelId channelId, bool muted) =>
        Log($"Output channel {channelId.Value} mute status is now {muted}");

    private void Log(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {message}");
}

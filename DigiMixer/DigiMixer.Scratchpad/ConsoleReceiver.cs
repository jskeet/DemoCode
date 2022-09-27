namespace DigiMixer.Scratchpad;

internal class ConsoleReceiver : IMixerReceiver
{
    public void ReceiveChannelName(InputChannelId channelId, string name) =>
        Log($"Input channel {channelId.Value} has name {name}");

    public void ReceiveChannelName(OutputChannelId channelId, string name) =>
        Log($"Output channel {channelId.Value} has name {name}");

    public void ReceiveFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level) =>
        Log($"Fader level {inputId.Value}/{outputId.Value} has level {level.Value}");

    public void ReceiveMeterLevel(OutputChannelId outputId, MeterLevel level)
    {
        throw new NotImplementedException();
    }

    public void ReceiveMeterLevel(InputChannelId outputId, MeterLevel level)
    {
        throw new NotImplementedException();
    }

    public void ReceiveMuteStatus(InputChannelId channelId, bool muted) =>
        Log($"Input channel {channelId.Value} mute status is now {muted}");

    public void ReceiveMuteStatus(OutputChannelId channelId, bool muted) =>
        Log($"Output channel {channelId.Value} mute status is now {muted}");

    private void Log(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {message}");
}

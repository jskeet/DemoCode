using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

public static class YamahaClientExtensions
{
    public static Task SendAsync(this YamahaClient client, WrappedMessage message, CancellationToken cancellationToken) =>
        client.SendAsync(message.RawMessage, cancellationToken);
}

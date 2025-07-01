using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.SqSeries;

public class SqMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 51326, MixerApiOptions? options = null) =>
        throw new NotImplementedException();
}

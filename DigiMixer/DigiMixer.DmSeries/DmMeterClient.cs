using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries;
internal class DmMeterClient(ILogger logger) : UdpControllerBase(logger, 50272)
{
    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        // TODO
    }
}
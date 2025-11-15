// The RCF M-18 implements OSC, but in a slightly unusual way:
//
// A client makes a request to the mixer on UDP port 8000, but then the
// mixer sends traffic to the client on its UDP port 9000. That means there
// can only be a single client on each IP address.
// (Compare this with X-Air mixers, which send traffic back to the client on
// the client's UDP port which was used to make the original request,
// allowing multiple independent clients.)
//
// This program acts as a proxy so that clients can communicate with a mixer
// in a similar way to the X-Air. The ports involved are:
//
// proxyToMixer: all traffic from the proxy to the mixer is sent on this
// mixerToProxy: a separate listening port which the mixer sends traffic to
// clientToProxy: a listening port for clients to connect to
//
// All traffic sent to clientToProxy is forwarded to proxyToMixer.
// All traffic sent to mixerToProxy is forwarded to endpoints which have
// recently sent traffic to clientToProxy. (Clients need to send keepalives
// to the proxy.)
//
// One problem: the M-18 doesn't "reflect back" changes made, so we can't tell
// multiple proxy clients about the requests from each other. Hmm.
// We could potentially automatically reflect a subset of requests (only those
// setting fader/mute values).

using DigiMixer.RcfProxy;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var mixerAddressOption = new Option<string>("--mixerAddress")
{
    Description = "The address of the mixer",
    Required = true
};
var mixerPortOption = new Option<int>("--mixerPort")
{
    DefaultValueFactory = _ => 8000,
    Description = "The port to connect to on the mixer",
    Required = false
};
var localPortForMixerOption = new Option<int>("--localPortForMixer")
{
    DefaultValueFactory = _ => 9000,
    Description = "The local port the mixer connects to",
    Required = false
};
var localPortForClientsOption = new Option<int>("--localPortForClients")
{
    DefaultValueFactory = _ => 8001,
    Description = "The local port for clients to connect to",
    Required = false
};

var factory = LoggerFactory.Create(builder => builder.AddConsole().AddSystemdConsole(options => { options.UseUtcTimestamp = true; options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"; })
    .SetMinimumLevel(LogLevel.Debug));
var logger = factory.CreateLogger("Proxy");

var rootCommand = new RootCommand
{
    mixerAddressOption, mixerPortOption, localPortForMixerOption, localPortForClientsOption
};
var parseResult = rootCommand.Parse(args);
await Proxy.Start(
    parseResult.GetRequiredValue(mixerAddressOption), parseResult.GetValue(mixerPortOption),
    parseResult.GetValue(localPortForMixerOption), parseResult.GetValue(localPortForClientsOption),
    logger);

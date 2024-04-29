// See https://aka.ms/new-console-template for more information
using AlsaSharp;

var sequencer = new AlsaSequencer(AlsaIOType.Duplex, AlsaIOMode.NonBlocking);
var ports = EnumerateMatchingPorts(sequencer, AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport).ToList();
foreach (var p in ports)
{
    Console.WriteLine($"{p.Name} {p.Id}");
}

var buffer = new byte[1024];
var sourcePort = ports.Single(p => p.Id == "24_00");
var appPort = CreateInputConnectedPort(sequencer, sourcePort);
sequencer.StartListening(appPort.Port, buffer, DumpMessage);

Thread.Sleep(100000);

AlsaPortInfo CreateInputConnectedPort(AlsaSequencer seq, AlsaPortInfo pinfo, string portName = "alsa-sharp input")
{
    var portId = seq.CreateSimplePort(portName, AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport, AlsaPortType.MidiGeneric | AlsaPortType.Application);
    var sub = new AlsaPortSubscription();
    sub.Destination.Client = (byte) seq.CurrentClientId;
    sub.Destination.Port = (byte) portId;
    sub.Sender.Client = (byte) pinfo.Client;
    sub.Sender.Port = (byte) pinfo.Port;
    seq.SubscribePort(sub);
    return seq.GetPort(sub.Destination.Client, sub.Destination.Port);
}

IEnumerable<AlsaPortInfo> EnumerateMatchingPorts(AlsaSequencer seq, AlsaPortCapabilities cap)
{
    var cinfo = new AlsaClientInfo { Client = -1 };
    while (seq.QueryNextClient(cinfo))
    {
        var pinfo = new AlsaPortInfo { Client = cinfo.Client, Port = -1 };
        while (seq.QueryNextPort(pinfo))
            if ((pinfo.PortType & AlsaPortType.MidiGeneric | AlsaPortType.Application) != 0 &&
                (pinfo.Capabilities & cap) == cap)
                yield return pinfo.Clone();
    }
}

void DumpMessage(byte[] buffer, int start, int len)
{
    var data = buffer.AsSpan().Slice(start, len);
    Console.WriteLine($"Received message: {BitConverter.ToString(data.ToArray())}");
}
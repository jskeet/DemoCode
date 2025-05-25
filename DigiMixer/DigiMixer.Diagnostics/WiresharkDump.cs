using DigiMixer.Core;
using PcapngFile;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.Diagnostics;

public class WiresharkDump
{
    private readonly IReadOnlyList<BlockBase> blocks;

    public IEnumerable<IPV4Packet> IPV4Packets => blocks.Select(IPV4Packet.TryConvert).OfType<IPV4Packet>();

    private WiresharkDump(IReadOnlyList<BlockBase> blocks)
    {
        this.blocks = blocks;
    }

    public static WiresharkDump Load(string filename)
    {
        using var reader = new Reader(filename);
        var blocks = reader.AllBlocks.ToList();
        return new WiresharkDump(blocks);
    }

    public async Task<ImmutableList<AnnotatedMessage<TMessage>>> ProcessMessages<TMessage>(string mixerIpAddress, string clientIpAddress) where TMessage : class, IMixerMessage<TMessage>
    {
        // Currently we assume a single client, a single mixer, and a single stream of messages between the two.
        IPAddress clientAddr = IPAddress.Parse(clientIpAddress);
        IPAddress mixerAddr = IPAddress.Parse(mixerIpAddress);

        DateTime? currentTimestamp = null;
        IPAddress? currentSource = null;
        IPAddress? currentDestination = null;

        List<AnnotatedMessage<TMessage>> messages = [];

        var outboundProcessor = new MessageProcessor<TMessage>(AddMessage, 1024 * 1024);
        var inboundProcessor = new MessageProcessor<TMessage>(AddMessage, 1024 * 1024);

        foreach (var packet in IPV4Packets)
        {
            if (packet.Type != ProtocolType.Tcp)
            {
                continue;
            }

            currentTimestamp = packet.Timestamp;
            currentSource = packet.Source.Address;
            currentDestination = packet.Dest.Address;
            if (packet.Source.Address.Equals(clientAddr) && packet.Dest.Address.Equals(mixerAddr))
            {
                await outboundProcessor.Process(packet.Data, default);
            }
            else if (packet.Source.Address.Equals(mixerAddr) && packet.Dest.Address.Equals(clientAddr))
            {
                await inboundProcessor.Process(packet.Data, default);
            }
        }
        return [.. messages];

        void AddMessage(TMessage message)
        {
            var direction = currentSource.OrThrow().Equals(clientAddr) ?
                MessageDirection.ClientToMixer : MessageDirection.MixerToClient;
            var annotated = new AnnotatedMessage<TMessage>(message, currentTimestamp.OrThrow(), direction,
                currentSource.OrThrow(), currentDestination.OrThrow());
            messages.Add(annotated);
        }
    }
}

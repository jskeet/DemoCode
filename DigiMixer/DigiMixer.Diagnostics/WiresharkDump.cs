using PcapngFile;

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
        using (var reader = new Reader(filename))
        {
            var blocks = reader.AllBlocks.ToList();
            return new WiresharkDump(blocks);
        }
    }
}

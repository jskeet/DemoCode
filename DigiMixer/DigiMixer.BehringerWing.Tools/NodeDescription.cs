using System.Globalization;

namespace DigiMixer.BehringerWing.Tools;

public record NodeDescription(string Name, uint Hash)
{
    public static NodeDescription FromSourceLine(string line)
    {
        // Sample line:
        // 		{     "cfg.mtr.$scopesrc",                     0xf1f55302,  I32, 0x0240, {    0} },

        var bits = line.Split(',');
        string name = bits[0].Split('"')[1];
        uint hash = uint.Parse(bits[1].Trim().Replace("0x", ""), NumberStyles.HexNumber);
        return new(name, hash);
    }
}

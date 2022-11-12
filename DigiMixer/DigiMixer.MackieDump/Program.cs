using DigiMixer.MackieDump;

if (args.Length != 1)
{
    Console.WriteLine("Please specify a text file to parse");
    return;
}

var lines = File.ReadAllLines(args[0]);

PacketBuffer clientBuffer = new PacketBuffer("Client");
PacketBuffer mixerBuffer = new PacketBuffer("Mixer ");

foreach (var line in lines)
{
    var parsed = ParseLine(line);
    if (parsed is null)
    {
        continue;
    }
    var buffer = parsed.IsMixer ? mixerBuffer : clientBuffer;
    buffer.Consume(parsed.Data);
}

Line? ParseLine(string line)
{
    if (line.Trim() == "")
    {
        return null;
    }
    bool isMixer = line.StartsWith("    ");
    if (isMixer)
    {
        // Remove the indentation
        line = line[4..];
    }

    // Remove the index within the stream
    line = line[10..];
    int indexOfText = line.IndexOf("   ");
    // Remove the text at the end
    line = line[..indexOfText];
    byte[] data = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(part => Convert.ToByte(part, 16))
        .ToArray();
    return new Line(isMixer, data);
}

record Line(bool IsMixer, byte[] Data);
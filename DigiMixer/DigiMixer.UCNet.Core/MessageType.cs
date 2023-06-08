namespace DigiMixer.UCNet.Core;

public enum MessageType : ushort
{
    KeepAlive = 'K' | ('A' << 8),
    UdpMeters = 'U' | ('M' << 8),
    Json = 'J' | ('M' << 8),
    ParameterValue = 'P' | ('V' << 8),
    ParameterString = 'P' | ('S' << 8),
    ParameterStringList = 'P' | ('L' << 8),
    FileRequest = 'F' | ('R' << 8),
    FileData = 'F' | ('D' << 8),
    BinaryObject = 'B' | ('O' << 8),
    Chunk = 'C' | ('K' << 8),
    CompressedJson = 'Z' | ('B' << 8),
    Meter16 = 'M' | ('S' << 8),
    Meter8 = 'M' | ('B' << 8),
}

using DigiMixer.MackieDump;

if (args.Length == 0)
{
    Console.WriteLine("Required command line arguments: <mode> <options>");
    Console.WriteLine("Modes: record, dump, convert, listen");
    Console.WriteLine("Record options: <IP address> <port - typically 50001> <output file>");
    Console.WriteLine("Dump options: <file>");
    Console.WriteLine("Convert options: <input file> <client address> <mixer address> <output file>");
    Console.WriteLine("Listen options: <input file> <client address> <mixer address> <output file>");
    return;
}

if (args[0] == "record")
{
    if (args.Length != 4)
    {
        Console.WriteLine("Record options: <IP address> <port - typically 50001> <output file>");
        return;
    }
    string address = args[1];
    int port = int.Parse(args[2]);
    string file = args[3];
    await Recorder.ExecuteAsync(address, port, file);
}
else if (args[0] == "dump")
{
    if (args.Length != 2)
    {
        Console.WriteLine("Dump options: <file>");
        return;
    }
    Dumper.Execute(args[1]);
}
else if (args[0] == "convert")
{
    if (args.Length != 5)
    {
        Console.WriteLine("Convert options: <input file> <client address> <mixer address> <output file>");
    }
    Converter.Execute(args[1], args[2], args[3], args[4]);
}
else if (args[0] == "listen")
{
    if (args.Length != 4)
    {
        Console.WriteLine("Listen options: <IP address> <port - typically 50001> <output file>");
        return;
    }
    string address = args[1];
    int port = int.Parse(args[2]);
    string file = args[3];
    await Listener.ExecuteAsync(address, port, file);
}
else
{
    Console.WriteLine($"Unknown mode: {args[0]}");
    return;
}


using DigiMixer.MackieDump;
using System.Net;

if (args.Length == 0)
{
    Console.WriteLine("Required command line arguments: <mode> <options>");
    Console.WriteLine("Modes: record, dump, convert, listen, info");
    Console.WriteLine("Record options: <IP address> <port - typically 50001> <output file>");
    Console.WriteLine("Dump options: <file>");
    Console.WriteLine("Convert options: <input file> <client address> <mixer address> <output file>");
    Console.WriteLine("Listen options: <input file> <client address> <mixer address> <output file>");
    Console.WriteLine("Info options: <mixer address> <port - typically 50001> <output file> <info numbers ...>");
    return;
}

switch (args[0])
{
    case "record":
        if (args.Length != 4)
        {
            Console.WriteLine("Record options: <IP address> <port - typically 50001> <output file>");
            return;
        }
        await Recorder.ExecuteAsync(args[1], int.Parse(args[2]), args[3]);
        break;
    case "dump":
        if (args.Length != 2)
        {
            Console.WriteLine("Dump options: <file>");
            return;
        }
        Dumper.Execute(args[1]);
        break;
    case "convert":
        if (args.Length != 5)
        {
            Console.WriteLine("Convert options: <input file> <client address> <mixer address> <output file>");
        }
        Converter.Execute(args[1], args[2], args[3], args[4]);
        break;
    case "listen":
        if (args.Length != 4)
        {
            Console.WriteLine("Listen options: <IP address> <port - typically 50001> <output file>");
            return;
        }
        await Listener.ExecuteAsync(args[1], int.Parse(args[2]), args[3]);
        break;
    case "info":
        if (args.Length < 4)
        {
            Console.WriteLine("Info options: <mixer address> <port - typically 50001> <output file> <info numbers ...>");
        }
        await InfoRequester.ExecuteAsync(args[1], int.Parse(args[2]), args[3], args.Skip(4).Select(byte.Parse).ToList());
        break;
    default:
    Console.WriteLine($"Unknown mode: {args[0]}");
        break;
}


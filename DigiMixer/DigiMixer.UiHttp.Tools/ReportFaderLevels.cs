using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Sockets;
using System.Text;

namespace DigiMixer.UiHttp.Tools;

public class ReportFaderLevels : Tool
{
    private const string HttpPreamble = "GET /raw HTTP1.1\n\n";

    public override async Task<int> Execute()
    {
        var client = await CreateStreamClient("192.168.1.57", 80, default);
        client.MessageReceived += MaybeLogMessage;
        _ = client.StartReading();

        while (true)
        {
            await Task.Delay(2000);
            await client.Send(UiMessage.AliveMessage, default);
        }

        void MaybeLogMessage(object? sender, UiMessage message)
        {
            if (message.Address?.EndsWith(".mix", StringComparison.Ordinal) == true)
            {
                Console.WriteLine($"{message.Address}: {message.DoubleValue}");
            }
        }
    }

    // Copied from UiHttpMixerApi. At some point we might want more of a separation.
    private async Task<IUiClient> CreateStreamClient(string host, int port, CancellationToken initialCancellationToken)
    {
        var tcpClient = new TcpClient() { NoDelay = true };
        await tcpClient.ConnectAsync(host, port, initialCancellationToken);
        var stream = tcpClient.GetStream();
        byte[] preambleBytes = Encoding.ASCII.GetBytes(HttpPreamble);
        stream.Write(preambleBytes, 0, preambleBytes.Length);
        await ReadHttpResponseHeaders();
        return new UiStreamClient(NullLogger.Instance, stream);

        async Task ReadHttpResponseHeaders()
        {
            // Read a single byte at a time to avoid causing problems for the UiStreamClient.
            byte[] buffer = new byte[1];
            byte[] doubleLineBreak = { 0x0d, 0x0a, 0x0d, 0x0a };
            int doubleLineBreakIndex = 0;
            int totalBytesRead = 0;
            // If we've read this much data, we've missed it...
            while (totalBytesRead < 256)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, 1);
                if (bytesRead == 0)
                {
                    throw new InvalidDataException("Never reached the end of the HTTP headers");
                }
                if (buffer[0] == doubleLineBreak[doubleLineBreakIndex])
                {
                    doubleLineBreakIndex++;
                    if (doubleLineBreakIndex == doubleLineBreak.Length)
                    {
                        return;
                    }
                }
                else
                {
                    doubleLineBreakIndex = 0;
                }
                totalBytesRead++;
            }
            throw new InvalidDataException($"Read {totalBytesRead} bytes without reaching the end of HTTP headers");
        }
    }
}

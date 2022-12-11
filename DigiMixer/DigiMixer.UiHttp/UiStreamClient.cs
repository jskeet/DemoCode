using Microsoft.Extensions.Logging;
using System.Text;

namespace DigiMixer.UiHttp;
internal sealed class UiStreamClient : IUiClient
{
    private const int MaxBufferSize = 256 * 1024; // This really should be enough.

    private readonly ILogger logger;
    private readonly Stream stream;
    private readonly CancellationTokenSource cts;
    private byte[] writeBuffer;

    public event EventHandler<UiMessage>? MessageReceived;

    internal UiStreamClient(ILogger logger, Stream stream)
    {
        this.logger = logger;
        this.stream = stream;
        cts = new CancellationTokenSource();
        // TODO: Check what the actual maximum buffer size is that we need.
        writeBuffer = new byte[256];
    }

    public async Task Send(UiMessage message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending message: {message}", message);
        }
        int messageSize = message.WriteTo(writeBuffer);
        await stream.WriteAsync(writeBuffer, 0, messageSize);
    }

    public async Task StartReading()
    {
        byte[] buffer = new byte[8192];
        int bufferPosition = 0;

        try
        {
            while (!cts.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, bufferPosition, buffer.Length - bufferPosition, cts.Token);
                int newBufferPosition = bufferPosition + bytesRead;
                int messageStart = 0;
                // Check if we've reached the end of one or more messages.
                for (int i = bufferPosition; i < newBufferPosition; i++)
                {
                    if (buffer[i] == '\n')
                    {
                        // Only parse the message if we have a message handler.
                        if (MessageReceived is EventHandler<UiMessage> handler)
                        {
                            var message = UiMessage.Parse(buffer, messageStart, i - messageStart);
                            if (logger.IsEnabled(LogLevel.Trace))
                            {
                                logger.LogTrace("Processing message: {message}", message);
                            }

                            handler.Invoke(this, message);
                        }
                        messageStart = i + 1;
                    }
                }

                // If we've got to the end, just start writing over the buffer from the start.
                if (messageStart == newBufferPosition)
                {
                    bufferPosition = 0;
                }
                // If we haven't found a message yet, we can keep writing without any copying.
                else if (messageStart == 0)
                {
                    bufferPosition = newBufferPosition;
                }
                else if (messageStart != 0)
                {
                    int remainingLength = newBufferPosition - messageStart;
                    Buffer.BlockCopy(buffer, messageStart, buffer, 0, remainingLength);
                    bufferPosition = remainingLength;
                }
                if (bufferPosition == buffer.Length)
                {
                    var newBufferSize = buffer.Length * 2;
                    if (newBufferSize > MaxBufferSize)
                    {
                        throw new InvalidDataException("Mixer sent too much data without a newline");
                    }
                    Array.Resize(ref buffer, newBufferSize);
                }
            }
        }
        catch when (cts.IsCancellationRequested)
        {
            // Swallow any errors due to disposal.
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        stream.Dispose();
    }
}

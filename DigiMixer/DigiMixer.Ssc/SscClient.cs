// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DigiMixer.Ssc;

public sealed class SscClient : UdpControllerBase, ISscClient
{
    public event EventHandler<SscMessage>? MessageReceived;

    public SscClient(ILogger logger, string host, int port) : base(logger, host, port, localPort: null)
    {
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        try
        {
            string json = Encoding.UTF8.GetString(data);
            MessageReceived?.Invoke(this, SscMessage.FromJson(json));
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to parse SSC message");
        }
    }

    public async Task SendMessage(SscMessage message, CancellationToken cancellationToken)
    {
        string json = message.ToJson();
        await Send(Encoding.UTF8.GetBytes(json), cancellationToken);
    }
}

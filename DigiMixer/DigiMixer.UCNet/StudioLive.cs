using DigiMixer.Core;
using DigiMixer.UCNet.Core;
using DigiMixer.UCNet.Core.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DigiMixer.UCNet;

public static class StudioLive
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 53000) =>
        new StudioLiveMixerApi(logger, host, port);

    private class StudioLiveMixerApi : IMixerApi
    {
        private readonly string clientIdentifier;

        private readonly DelegatingReceiver receiver = new();
        private readonly ILogger logger;
        private readonly string host;
        private readonly int port;
        private readonly Dictionary<string, Action<ParameterValueMessage>> parameterHandlers;

        private UCNetClient? client;
        private UCNetMeterListener? meterListener;
        private CancellationTokenSource? cts;

        internal StudioLiveMixerApi(ILogger logger, string host, int port)
        {
            this.logger = logger;
            this.host = host;
            this.port = port;
            parameterHandlers = MapParameterHandlers();
            clientIdentifier = $"DigiMixer-{Guid.NewGuid()}";
        }

        private Dictionary<string, Action<ParameterValueMessage>> MapParameterHandlers()
        {
            var map = new Dictionary<string, Action<ParameterValueMessage>>();
            var possibleInputs = Enumerable.Range(1, 32).Select(ChannelId.Input);
            var possibleOutputs = Enumerable.Range(1, 16).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft);

            foreach (var input in possibleInputs)
            {
                map.Add(Addresses.GetMuteAddress(input), message => receiver.ReceiveMuteStatus(input, message.Value != 0));
                foreach (var output in possibleOutputs)
                {
                    map.Add(Addresses.GetFaderAddress(input, output),
                        message => receiver.ReceiveFaderLevel(input, output, MessageToFaderLevel(message)));
                }
            }

            foreach (var output in possibleOutputs)
            {
                map.Add(Addresses.GetMuteAddress(output),
                    message => receiver.ReceiveMuteStatus(output, message.Value != 0));
                map.Add(Addresses.GetFaderAddress(output),
                    message => receiver.ReceiveFaderLevel(output, MessageToFaderLevel(message)));
            }
            return map;

            FaderLevel MessageToFaderLevel(ParameterValueMessage message) =>
                ToFaderLevel(BitConverter.UInt32BitsToSingle(message.Value ?? 0));
        }

        public async Task Connect(CancellationToken cancellationToken)
        {
            Dispose();

            cts = new CancellationTokenSource();
            client = new UCNetClient(logger, host, port);
            client.MessageReceived += HandleMessage;
            await client.Connect(cancellationToken);
            client.Start();
            meterListener = new UCNetMeterListener(logger);
            meterListener.MessageReceived += (sender, message) => HandleMeter16Message(message);
            meterListener.Start();

            await SendMessage(new UdpMetersMessage(meterListener.LocalPort), cancellationToken);
        }

        private void HandleMessage(object? sender, UCNetMessage message)
        {
            switch (message)
            {
                case CompressedJsonMessage compressedJson:
                    HandleCompressedJson(compressedJson);
                    break;
                case ParameterValueMessage parameterValue:
                    HandleParameterValue(parameterValue);
                    break;
                case Meter16Message meters:
                    HandleMeter16Message(meters);
                    break;
                case Meter8Message meters:
                    HandleMeter8Message(meters);
                    break;
            }
        }

        private void HandleMeter16Message(Meter16Message message)
        {
            switch (message.MeterType)
            {
                case "fdrs":
                    HandleFaderLevels();
                    break;
                case "levl":
                    HandleMeterLevels();
                    break;
            }

            void HandleFaderLevels()
            {
                foreach (var row in message.Rows)
                {
                    switch (row.Source)
                    {
                        case MeterSource.Input:
                            for (int i = 0; i < row.Count; i++)
                            {
                                var channelId = ChannelId.Input(i + 1);
                                var level = GetFaderLevel(row, i);
                                receiver.ReceiveFaderLevel(channelId, ChannelId.MainOutputLeft, level);
                            }
                            break;
                        case MeterSource.Aux:
                            for (int i = 0; i < row.Count; i++)
                            {
                                var channelId = ChannelId.Output(i + 1);
                                var level = GetFaderLevel(row, i);
                                receiver.ReceiveFaderLevel(channelId, level);
                            }
                            break;
                        case MeterSource.Main:
                            {
                                var level = GetFaderLevel(row, 0);
                                receiver.ReceiveFaderLevel(ChannelId.MainOutputLeft, level);
                            }
                            break;
                    }
                }

                FaderLevel GetFaderLevel(MeterMessageRow<ushort> row, int index) =>
                    new FaderLevel(row.GetValue(index) / 64);
            }

            void HandleMeterLevels()
            {
                var count = 0;
                foreach (var row in message.Rows)
                {
                    switch (row.Source, row.Stage)
                    {
                        case (MeterSource.Input, MeterStage.Raw):
                            count += row.Count;
                            break;
                        case (MeterSource.Aux, MeterStage.PostLimiter):
                            count += row.Count;
                            break;
                        case (MeterSource.Main, MeterStage.PostLimiter):
                            count += row.Count;
                            break;
                    }
                }
                var result = new (ChannelId, MeterLevel)[count];
                int index = 0;
                foreach (var row in message.Rows)
                {
                    switch (row.Source, row.Stage)
                    {
                        case (MeterSource.Input, MeterStage.Raw):
                            for (int i = 0; i < row.Count; i++)
                            {
                                result[index++] = (ChannelId.Input(i + 1), GetMeterLevel(row, i));
                            }
                            break;
                        case (MeterSource.Aux, MeterStage.PostLimiter):
                            for (int i = 0; i < row.Count; i++)
                            {
                                result[index++] = (ChannelId.Output(i + 1), GetMeterLevel(row, i));
                            }
                            break;
                        case (MeterSource.Main, MeterStage.PostLimiter):
                            // TODO: What if we don't have exactly 2 values? Left-only?
                            result[index++] = (ChannelId.MainOutputLeft, GetMeterLevel(row, 0));
                            result[index++] = (ChannelId.MainOutputRight, GetMeterLevel(row, 1));
                            break;
                    }
                }

                receiver.ReceiveMeterLevels(result);

                MeterLevel GetMeterLevel(MeterMessageRow<ushort> row, int index)
                {
                    var raw = row.GetValue(index) / 65535d;
                    var db = raw == 0 ? double.NegativeInfinity : 20 * Math.Log10(raw);
                    return MeterLevel.FromDb(db);
                }
            }
        }

        private void HandleMeter8Message(Meter8Message message)
        {
            //throw new NotImplementedException();
        }

        private void HandleCompressedJson(CompressedJsonMessage message)
        {
            // TODO: Ignore or merge SynchronizePart messages.
            string json = message.ToJson();
            JObject jobj = JObject.Parse(json);

            var global = (JObject) jobj["children"]!["global"]!["values"]!;
            string deviceName = (string) global["devicename"]!;
            string mixerName = (string) global["mixer_name"]!;
            string version = (string) global["mixer_version"]!;
            receiver.ReceiveMixerInfo(new MixerInfo(mixerName, deviceName, version));

            var line = (JObject) jobj["children"]!["line"]!["children"]!;
            var aux = (JObject) jobj["children"]!["aux"]!["children"]!;
            var main = (JObject) jobj["children"]!["main"]!["children"]!["ch1"]!["values"]!;

            for (int i = 1; i <= line.Count; i++)
            {
                JObject channel = (JObject) line[$"ch{i}"]!["values"]!;
                ChannelId id = ChannelId.Input(i);
                string name = (string) channel["username"]!;
                receiver.ReceiveChannelName(id, name);
                bool muted = ((double) channel["mute"]!) == 1.0;
                receiver.ReceiveMuteStatus(id, muted);
                double volume = (double) channel["volume"]!;
                receiver.ReceiveFaderLevel(id, ChannelId.MainOutputLeft, ToFaderLevel(volume));
                for (int j = 1; j <= aux.Count; j++)
                {
                    ChannelId auxId = ChannelId.Output(j);
                    double auxOutput = (double) channel[$"aux{j}"]!;
                    receiver.ReceiveFaderLevel(id, auxId, ToFaderLevel(auxOutput));
                }
            }

            for (int i = 1; i <= aux.Count; i++)
            {
                JObject channel = (JObject) aux[$"ch{i}"]!["values"]!;
                ChannelId id = ChannelId.Output(i);
                bool muted = ((double) channel["mute"]!) == 1.0;
                receiver.ReceiveMuteStatus(id, muted);
                string name = (string) channel["username"]!;
                receiver.ReceiveChannelName(id, name);
                double volume = (double) channel["volume"]!;
                receiver.ReceiveFaderLevel(id, ToFaderLevel(volume));
            }

            // Main
            {
                string name = (string) main["username"]!;
                receiver.ReceiveChannelName(ChannelId.MainOutputLeft, name);
                bool muted = ((double) main["mute"]!) == 1.0;
                receiver.ReceiveMuteStatus(ChannelId.MainOutputLeft, muted);
                double volume = (double) main["volume"]!;
                receiver.ReceiveFaderLevel(ChannelId.MainOutputLeft, ToFaderLevel(volume));
            }
        }

        private static FaderLevel ToFaderLevel(double value) => new FaderLevel((int) (value * 1024));

        private void HandleParameterValue(ParameterValueMessage parameterValue)
        {
            if (parameterHandlers.TryGetValue(parameterValue.Key, out var handler))
            {
                handler(parameterValue);
            }
        }

        public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
        {
            var localClient = client;
            var localCts = cts;
            if (localClient is null || localCts is null)
            {
                throw new InvalidOperationException("Not connected");
            }

            using var methodCts = CancellationTokenSource.CreateLinkedTokenSource(localCts.Token, cancellationToken);

            TaskCompletionSource<MixerChannelConfiguration> tcs = new();
            try
            {
                localClient.MessageReceived += LocalHandleMessage;
                await localClient.SendAsync(JsonMessage.FromObject(new SubscribeBody { ClientIdentifier = clientIdentifier }), methodCts.Token);
                return await tcs.Task.WaitAsync(methodCts.Token);
            }
            finally
            {
                localClient.MessageReceived -= LocalHandleMessage;
            }

            void LocalHandleMessage(object? sender, UCNetMessage message)
            {
                if (message is not CompressedJsonMessage cjm)
                {
                    return;
                }
                string json = cjm.ToJson();
                JObject jobj = JObject.Parse(json);
                tcs.TrySetResult(ConvertConfiguration(jobj));
            }

            MixerChannelConfiguration ConvertConfiguration(JObject obj)
            {
                var line = (JObject) obj["children"]!["line"]!["children"]!;
                var aux = (JObject) obj["children"]!["aux"]!["children"]!;

                var inputs = Enumerable.Range(1, line.Count).Select(i => ChannelId.Input(i));
                var outputs = Enumerable.Range(1, aux.Count).Select(i => ChannelId.Output(i))
                    .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);
                var stereoPairs = CreateStereoPairs(line, ChannelId.Input)
                    .Concat(CreateStereoPairs(aux, ChannelId.Output))
                    .Append(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));
                logger.LogInformation("Converted JSON");
                return new MixerChannelConfiguration(inputs, outputs, stereoPairs);

                IEnumerable<StereoPair> CreateStereoPairs(JObject parent, Func<int, ChannelId> factory)
                {
                    for (int i = 1; i <= parent.Count; i += 2)
                    {
                        JObject channel = (JObject) parent[$"ch{i}"]!["values"]!;
                        JValue value = (JValue) channel["panlinkstate"]!;
                        if ((double) value == 1.0)
                        {
                            // TODO: Get StereoFlags from elsewhere.
                            yield return new StereoPair(factory(i), factory(i + 1), StereoFlags.SplitNames);
                        }
                    }
                }
            }
        }

        public void RegisterReceiver(IMixerReceiver receiver) =>
            this.receiver.RegisterReceiver(receiver);

        public Task RequestAllData(IReadOnlyList<ChannelId> channelIds) =>
            SendMessage(JsonMessage.FromObject(new SubscribeBody { ClientIdentifier = clientIdentifier }));

        // TODO: Check this.
        public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);
        public IFaderScale FaderScale => DefaultFaderScale.Instance;

        public Task SendKeepAlive() => SendMessage(new KeepAliveMessage());

        public async Task<bool> CheckConnection(CancellationToken cancellationToken)
        {
            // TODO: Use whether or not we've received recent meter packets?
            // Propagate any existing client failures.
            if (client?.ControllerStatus != ControllerStatus.Running ||
                meterListener?.ControllerStatus != ControllerStatus.Running)
            {
                return false;
            }

            // And send a keepalive message.
            await SendMessage(new KeepAliveMessage(), cancellationToken);

            return true;
        }

        public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
            SetFaderLevelImpl(Addresses.GetFaderAddress(inputId, outputId), level);

        public Task SetFaderLevel(ChannelId outputId, FaderLevel level) =>
            SetFaderLevelImpl(Addresses.GetFaderAddress(outputId), level);

        private Task SetFaderLevelImpl(string address, FaderLevel level)
        {
            float linear = ((float) level.Value) / FaderLevel.MaxValue;
            // TODO: Try to remove BitConverter.
            uint integer = BitConverter.SingleToUInt32Bits(linear);
            return SendMessage(new ParameterValueMessage(address, 0, integer));
        }

        public Task SetMuted(ChannelId channelId, bool muted)
        {
            // TODO: Get rid of this... just use the right uint.
            uint value = muted ? BitConverter.SingleToUInt32Bits(1.0f) : 0;
            var address = Addresses.GetMuteAddress(channelId);
            return SendMessage(new ParameterValueMessage(address, 0, value));
        }

        private async Task SendMessage(UCNetMessage message, CancellationToken cancellationToken = default)
        {
            if (client is not null)
            {
                using var chained = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts?.Token ?? default);
                await client.SendAsync(message, chained.Token);
            }
        }

        public void Dispose()
        {
            client?.Dispose();
            meterListener?.Dispose();
        }
    }

    private static class Addresses
    {
        internal static string GetFaderAddress(ChannelId inputId, ChannelId outputId)
        {
            string prefix = GetInputPrefix(inputId);
            string suffix = outputId.IsMainOutput ? "/volume" : $"/aux{outputId.Value}";
            return prefix + suffix;
        }

        internal static string GetFaderAddress(ChannelId outputId) =>
            GetOutputPrefix(outputId) + "/volume";

        internal static string GetMuteAddress(ChannelId channelId) => GetPrefix(channelId) + "/mute";
        internal static string GetNameAddress(ChannelId channelId) => GetPrefix(channelId) + "/username";

        internal static string GetInputPrefix(ChannelId inputId) =>
            $"line/ch{inputId.Value}";

        internal static string GetOutputPrefix(ChannelId outputId) =>
            outputId.IsMainOutput ? "main/ch1" : $"aux/ch{outputId.Value}";

        private static string GetPrefix(ChannelId channelId) =>
            channelId.IsInput ? GetInputPrefix(channelId) : GetOutputPrefix(channelId);
    }
}

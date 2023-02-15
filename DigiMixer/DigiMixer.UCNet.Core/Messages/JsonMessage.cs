using Newtonsoft.Json;
using System.Text;

namespace DigiMixer.UCNet.Core.Messages;

public class JsonMessage : UCNetMessage
{
    private readonly string json;
    // The size of the JSON in bytes.
    private readonly int byteCount;

    private JsonMessage(string json, MessageMode mode) : base(mode)
    {
        this.json = json;
        byteCount = Encoding.UTF8.GetByteCount(json);
    }

    public override MessageType Type => MessageType.Json;
    protected override int BodyLength => byteCount + 4;

    protected override void WriteBody(Span<byte> buffer)
    {
        buffer.WriteInt32(byteCount);
        buffer.Slice(4).WriteString(json);
    }

    public static JsonMessage FromJson(string json, MessageMode mode = MessageMode.FileRequest) =>
        new JsonMessage(json, mode);

    public static JsonMessage FromObject(object body, MessageMode mode = MessageMode.FileRequest) =>
        FromJson(JsonConvert.SerializeObject(body), mode);

    internal static JsonMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        int length = body.ReadInt32();
        if (length != body.Length - 4)
        {
            throw new ArgumentException($"Message starts claiming JSON data length {length} but is {body.Length - 4}");
        }
        return FromJson(body.Slice(4).ReadString(), mode);
    }

    public override string ToString() => $"JSON: {json}";
}

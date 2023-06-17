using System.Globalization;
using System.Text;

namespace DigiMixer.UiHttp;

internal class UiMessage
{
    internal const string SetDoubleMessageType = "SETD";
    internal const string SetStringMessageType = "SETS";
    internal const string MeterType = "VU2";
    internal const string FrequencyMeterType = "RTA";
    internal const string AliveType = "ALIVE";
    internal const string InitType = "INIT";

    public string? Address { get; }

    /// <summary>
    /// Type of the message: ALIVE, SETD, VU2, RTA etc
    /// </summary>
    public string MessageType { get; }

    public string? Value { get; }
    public double DoubleValue => double.Parse(Value!, CultureInfo.InvariantCulture);
    public bool BoolValue => Value == "1";
    // The length of the message, including the terminating \n
    public int Length { get; }

    private UiMessage(string messageType, string? address, string? value, int? length)
    {
        var actualLength = length ?? ComputeLength(messageType, address, value);
        Address = address;
        MessageType = messageType;
        Value = value;
        Length = actualLength;
    }

    /// <summary>
    /// Parses a line of data, which should not include the trailing \n
    /// </summary>
    internal static UiMessage Parse(ReadOnlySpan<byte> data)
    {
        int messageLength = data.Length + 1;
        // Formats:
        // type
        // type^address^value
        // type^value

        int endOfType = data.IndexOf((byte) '^');
        string type;
        if (endOfType == -1)
        {
            type = Encoding.ASCII.GetString(data);
            return new UiMessage(type, null, null, messageLength);
        }
        type = Encoding.ASCII.GetString(data.Slice(0, endOfType));
        data = data.Slice(endOfType + 1);

        int endOfAddressOrValue = data.IndexOf((byte) '^');
        if (endOfAddressOrValue == -1)
        {
            string valueWithoutAddress = Encoding.ASCII.GetString(data);
            return new UiMessage(type, null, valueWithoutAddress, messageLength);
        }
        string address = Encoding.ASCII.GetString(data.Slice(0, endOfAddressOrValue));
        data = data.Slice(endOfAddressOrValue + 1);
        string value = Encoding.ASCII.GetString(data);
        return new UiMessage(type, address, value, messageLength);
    }

    internal static UiMessage AliveMessage { get; } = new UiMessage(AliveType, null, null, null);
    internal static UiMessage InitMessage { get; } = new UiMessage(InitType, null, null, null);

    internal static UiMessage CreateSetMessage(string address, bool value) =>
        new UiMessage(SetDoubleMessageType, address, value ? "1" : "0", null);

    internal static UiMessage CreateSetMessage(string address, double value) =>
        new UiMessage(SetDoubleMessageType, address, value.ToString("N17", CultureInfo.InvariantCulture), null);

    internal byte[] ToByteArray()
    {
        var array = new byte[Length];
        Encoding.ASCII.GetBytes(MessageType, 0, MessageType.Length, array, 0);
        int index = MessageType.Length;
        if (Address is string address)
        {
            array[index++] = (byte) '^';
            Encoding.ASCII.GetBytes(address, 0, address.Length, array, index);
            index += address.Length;
        }
        if (Value is string value)
        {
            array[index++] = (byte) '^';
            Encoding.ASCII.GetBytes(value, 0, value.Length, array, index);
            index += value.Length;
        }
        array[index++] = (byte) '\n';
        return array;
    }

    private static int ComputeLength(string messageType, string? address, string? value)
    {
        int length = messageType.Length;
        if (address is not null)
        {
            length += address.Length + 1;
        }
        if (value is not null)
        {
            length += value.Length + 1;
        }
        length++; // Trailing \n
        return length;
    }

    public override string ToString() =>
        Address is null && Value is null ? MessageType
        : Address is null ? $"{MessageType}^{Value}"
        : $"{MessageType}^{Address}^{Value}";
}

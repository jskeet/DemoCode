using System.Globalization;
using System.Text;

namespace DigiMixer.UiHttp;

internal class UiMessage
{
    internal const string SetMessageType = "SETD";
    internal const string MeterType = "VU2";
    internal const string FrequencyMeterType = "RTA";
    internal const string AliveType = "ALIVE";
    internal const string InfoType = "INFO";

    public string? Address { get; }

    /// <summary>
    /// Type of the message: ALIVE, SETD, VU2, RTA etc
    /// </summary>
    public string MessageType { get; }

    public string? Value { get; }
    public double DoubleValue => double.Parse(Value!, CultureInfo.InvariantCulture);
    public bool BoolValue => Value == "1";

    private UiMessage(string messageType, string? address, string? value)
    {
        Address = address;
        MessageType = messageType;
        Value = value;
    }

    internal static UiMessage Parse(byte[] buffer, int start, int length)
    {
        // Formats:
        // type
        // type^address^value
        // type^value

        int end = start + length;
        int endOfType = FindNextCaret(start);
        string type;
        if (endOfType == -1)
        {
            type = Encoding.ASCII.GetString(buffer, start, end - start);
            return new UiMessage(type, null, null);
        }
        type = Encoding.ASCII.GetString(buffer, start, endOfType - start);

        int startOfAddressOrValue = endOfType + 1;
        int endOfAddressOrValue = FindNextCaret(startOfAddressOrValue);
        if (endOfAddressOrValue == -1)
        {
            string valueWithoutAddress = Encoding.ASCII.GetString(buffer, startOfAddressOrValue, end - startOfAddressOrValue);
            return new UiMessage(type, null, valueWithoutAddress);
        }
        string address = Encoding.ASCII.GetString(buffer, startOfAddressOrValue, endOfAddressOrValue - startOfAddressOrValue);
        int startOfValue = endOfAddressOrValue + 1;
        string value = Encoding.ASCII.GetString(buffer, startOfValue, end - startOfValue);
        return new UiMessage(type, address, value);

        int FindNextCaret(int current)
        {
            for (int i = current; i < end; i++)
            {
                if (buffer[i] == '^')
                {
                    return i;
                }
            }
            return -1;
        }
    }

    internal static UiMessage AliveMessage { get; } = new UiMessage(AliveType, null, null);
    internal static UiMessage InitMessage { get; } = new UiMessage("INIT", null, null);

    internal static UiMessage CreateSetMessage(string address, bool value) =>
        new UiMessage(SetMessageType, address, value ? "1" : "0");

    internal static UiMessage CreateSetMessage(string address, double value) =>
        new UiMessage(SetMessageType, address, value.ToString("N17", CultureInfo.InvariantCulture));

    internal int WriteTo(byte[] buffer)
    {
        Encoding.ASCII.GetBytes(MessageType, 0, MessageType.Length, buffer, 0);
        int length = MessageType.Length;
        if (Address is string address)
        {
            buffer[length++] = (byte) '^';
            Encoding.ASCII.GetBytes(address, 0, address.Length, buffer, length);
            length += address.Length;
        }
        if (Value is string value)
        {
            buffer[length++] = (byte) '^';
            Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, length);
            length += value.Length;
        }
        buffer[length++] = (byte) '\n';
        return length;
    }

    public override string ToString() =>
        Address is null && Value is null ? MessageType
        : Address is null ? $"{MessageType}^{Value}"
        : $"{MessageType}^{Address}^{Value}";
}

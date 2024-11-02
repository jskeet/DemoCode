using System.Buffers.Binary;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace DigiMixer.BehringerWing.Core;

/// <summary>
/// Note: this does not implement IMixerMessage, as the stream contains
/// context about the current channel ID.
/// </summary>
public sealed class WingToken
{
    public static WingToken FalseOffZero { get; } = new(WingTokenType.FalseOffZero, 0);
    public static WingToken TrueOnOne { get; } = new(WingTokenType.TrueOnOne, 1);
    public static WingToken Toggle { get; } = new(WingTokenType.Toggle);

    public static WingToken RootNode { get; } = new(WingTokenType.RootNode);
    public static WingToken ParentNode { get; } = new(WingTokenType.ParentNode);
    public static WingToken DataRequest { get; } = new(WingTokenType.DataRequest);
    public static WingToken DefinitionRequest { get; } = new(WingTokenType.DefinitionRequest);
    public static WingToken EndOfRequest { get; } = new(WingTokenType.EndOfRequest);
    public static WingToken EmptyString { get; } = ForString("");

    public static WingToken ForInt16(short value) => new(WingTokenType.Int16, value);
    public static WingToken ForInt32(int value) => new(WingTokenType.Int32, value);
    public static WingToken ForNodeHash(uint value) => new(WingTokenType.NodeHash, (int) value);
    public static WingToken ForNodeIndex(ushort value) => new(WingTokenType.NodeIndex, value);
    public static WingToken ForFloat32(float value) => new(WingTokenType.Float32, value);
    public static WingToken ForRawFloat32(float value) => new(WingTokenType.RawFloat32, value);
    public static WingToken ForString(string value) => new(WingTokenType.String, value);
    public static WingToken ForNodeName(string value) => new(WingTokenType.NodeName, value);
    public static WingToken ForStep(byte value) => new(WingTokenType.Step, value);
    public static WingToken ForBool(bool value) => value ? TrueOnOne : FalseOffZero;
    public static WingToken? ForNodeDefinition(WingNodeDefinition definition) => new(WingTokenType.NodeDefinition, definition);

    public WingTokenType Type { get; }

    private readonly int int32Value;
    private readonly float float32Value;
    private readonly object? objectValue;

    private WingToken(WingTokenType type)
    {
        Type = type;
    }

    private WingToken(WingTokenType type, int int32Value) : this(type)
    {
        this.int32Value = int32Value;
    }

    private WingToken(WingTokenType type, float float32Value) : this(type)
    {
        this.float32Value = float32Value;
    }

    private WingToken(WingTokenType type, object objectValue) : this(type)
    {
        this.objectValue = objectValue;
    }

    public bool BoolValue => ValidateType(WingTokenType.FalseOffZero, WingTokenType.TrueOnOne, int32Value == 1);
    public int Int32Value => ValidateType(WingTokenType.Int32, WingTokenType.FalseOffZero, WingTokenType.TrueOnOne, int32Value);
    public short Int16Value => ValidateType(WingTokenType.Int16, (short) int32Value);
    public ushort NodeIndex => ValidateType(WingTokenType.NodeIndex, (ushort) int32Value);
    public uint NodeHash => ValidateType(WingTokenType.NodeHash, (uint) int32Value);
    public float Float32Value => ValidateType(WingTokenType.Float32, WingTokenType.RawFloat32, float32Value);
    public string StringValue => ValidateType(WingTokenType.String, objectValue as string)!;
    public string NodeName => ValidateType(WingTokenType.NodeName, objectValue as string)!;
    public WingNodeDefinition NodeDefinition => ValidateType(WingTokenType.NodeDefinition, objectValue as WingNodeDefinition)!;
    public byte Step => ValidateType(WingTokenType.Step, (byte) int32Value);

    /// <summary>
    /// Writes the message to the given span, assuming that the channel has already been set appropriately.
    /// This does not perform any escaping.
    /// </summary>
    /// <param name="span">The span to write data to.</param>
    /// <returns>The number of bytes written</returns>.
    internal int CopyTo(Span<byte> span)
    {
        switch (Type)
        {
            case WingTokenType.FalseOffZero:
                span[0] = 0;
                return 1;
            case WingTokenType.TrueOnOne:
                span[0] = 1;
                return 1;
            case WingTokenType.Int16:
                {
                    short value = Int16Value;
                    if (value >= 0 && value < 64)
                    {
                        span[0] = (byte) value;
                        return 1;
                    }
                    span[0] = 0xd3;
                    BinaryPrimitives.WriteInt16BigEndian(span[1..], value);
                    return 3;
                }
            // TODO: Should we handle small Int32 values with 2-63?
            case WingTokenType.Int32:
                span[0] = 0xd4;
                BinaryPrimitives.WriteInt32BigEndian(span[1..], Int32Value);
                return 3;
            case WingTokenType.String:
                {
                    string value = StringValue;
                    if (value == "")
                    {
                        span[0] = 0xd0;
                        return 1;
                    }
                    if (value.Length < 65)
                    {
                        span[0] = (byte) (0x7f + value.Length);
                        WingConstants.Encoding.GetBytes(value, span[1..]);
                        return 1 + value.Length;
                    }
                    span[0] = 0xd1;
                    span[1] = (byte) value.Length;
                    WingConstants.Encoding.GetBytes(value, span[2..]);
                    return 2 + value.Length;
                }
            case WingTokenType.NodeName:
                {
                    string value = NodeName;
                    span[0] = (byte) (0xbf + value.Length);
                    WingConstants.Encoding.GetBytes(value, span[1..]);
                    return 1 + value.Length;
                }
            case WingTokenType.NodeIndex:
                span[0] = 0xd2;
                BinaryPrimitives.WriteUInt16BigEndian(span[1..], NodeIndex);
                return 3;
            case WingTokenType.Float32:
                span[0] = 0xd5;
                BinaryPrimitives.WriteSingleBigEndian(span[1..], Float32Value);
                return 5;
            case WingTokenType.RawFloat32:
                span[0] = 0xd6;
                BinaryPrimitives.WriteSingleBigEndian(span[1..], Float32Value);
                return 5;
            case WingTokenType.NodeHash:
                span[0] = 0xd7;
                BinaryPrimitives.WriteUInt32BigEndian(span[1..], NodeHash);
                return 5;
            case WingTokenType.Toggle:
                span[0] = 0xd8;
                return 1;
            case WingTokenType.Step:
                span[0] = 0xd9;
                span[1] = Step;
                return 2;
            case WingTokenType.RootNode:
                span[0] = 0xda;
                return 1;
            case WingTokenType.ParentNode:
                span[0] = 0xdb;
                return 1;
            case WingTokenType.DataRequest:
                span[0] = 0xdc;
                return 1;
            case WingTokenType.DefinitionRequest:
                span[0] = 0xdd;
                return 1;
            case WingTokenType.EndOfRequest:
                span[0] = 0xde;
                return 1;
            case WingTokenType.NodeDefinition:
                return NodeDefinition.CopyTo(span);
            default:
                throw new InvalidOperationException($"Unknown token type {Type}");
        }

    }

    private T ValidateType<T>(WingTokenType expectedType1, T value, [CallerMemberName] string? caller = null) =>
        Type == expectedType1 ? value : throw new InvalidOperationException($"Invalid use of {caller} for type {Type}");

    private T ValidateType<T>(WingTokenType expectedType1, WingTokenType expectedType2, T value, [CallerMemberName] string? caller = null) =>
        Type == expectedType1 || Type == expectedType2 ? value : throw new InvalidOperationException($"Invalid use of {caller} for type {Type}");

    private T ValidateType<T>(WingTokenType expectedType1, WingTokenType expectedType2, WingTokenType expectedType3, T value, [CallerMemberName] string? caller = null) =>
        Type == expectedType1 || Type == expectedType2 || Type == expectedType3 ? value : throw new InvalidOperationException($"Invalid use of {caller} for type {Type}");

    internal static (WingToken?, int) TryParse(ReadOnlySpan<byte> span)
    {
        byte firstByte = span[0];

        return firstByte switch
        {
            0 => (FalseOffZero, 1),
            1 => (TrueOnOne, 1),
            <= 0x3f => (ForInt16(firstByte), 1),
            <= 0x7f => (ForNodeIndex((ushort) (firstByte - 0x3f)), 1),
            <= 0xbf => DecodeString(span, 1, firstByte - 0x7f) is string value ? (ForString(value), value.Length + 1) : (null, 0),
            <= 0xcf => DecodeString(span, 1, firstByte - 0xbf) is string value ? (ForNodeName(value), value.Length + 1) : (null, 0),
            0xd0 => (EmptyString, 1),
            0xd1 => DecodeString(span, 2, firstByte - 0xd0) is string value ? (ForString(value), value.Length + 2) : (null, 0),
            0xd2 => DecodeUInt16(span) is ushort value ? (ForNodeIndex(value), 3) : (null, 0),
            0xd3 => DecodeInt16(span) is short value ? (ForInt16(value), 3) : (null, 0),
            0xd4 => DecodeInt32(span) is int value ? (ForInt32(value), 5): (null, 0),
            0xd5 => DecodeFloat(span) is float value ? (ForFloat32(value), 5) : (null, 0),
            0xd6 => DecodeFloat(span) is float value ? (ForRawFloat32(value), 5) : (null, 0),
            0xd7 => DecodeInt32(span) is int value ? (ForNodeHash((uint) value), 5) : (null, 0),
            0xd8 => (Toggle, 1),
            0xd9 => span.Length < 2 ? (null, 0) : (ForStep(span[1]), 2),
            0xda => (RootNode, 1),
            0xdb => (ParentNode, 1),
            0xdc => (DataRequest, 1),
            0xdd => (DefinitionRequest, 1),
            0xde => (EndOfRequest, 1),
            0xdf => DecodeNodeDefinition(span),
            _ => throw new ArgumentException($"Unexpected first byte of token: 0x{firstByte:x2}")
        };

        string? DecodeString(ReadOnlySpan<byte> span, int offset, int length) => span.Length < offset + length ? null : WingConstants.Encoding.GetString(span.Slice(offset, length));

        ushort? DecodeUInt16(ReadOnlySpan<byte> span) => span.Length < 3 ? null : BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1, 2));
        short? DecodeInt16(ReadOnlySpan<byte> span) => span.Length < 3 ? null : BinaryPrimitives.ReadInt16BigEndian(span.Slice(1, 2));
        int? DecodeInt32(ReadOnlySpan<byte> span) => span.Length < 5 ? null : BinaryPrimitives.ReadInt32BigEndian(span.Slice(1, 4));
        float? DecodeFloat(ReadOnlySpan<byte> span) => span.Length < 5 ? null : BinaryPrimitives.ReadSingleBigEndian(span.Slice(1, 4));

        (WingToken?, int) DecodeNodeDefinition(ReadOnlySpan<byte> span)
        {
            if (DecodeUInt16(span) is not ushort shortLength)
            {
                return (null, 0);
            }
            if (shortLength == 0)
            {
                if (span.Length < 7) // DF 00 00 + (length as 4 bytes)
                {
                    return (null, 0);
                }
                int longLength = BinaryPrimitives.ReadInt32BigEndian(span.Slice(3, 4));
                return MaybeDecode(span, 7, longLength);
            }
            else
            {
                return MaybeDecode(span, 3, shortLength);
            }

            (WingToken?, int) MaybeDecode(ReadOnlySpan<byte> span, int bodyStart, int bodyLength) =>
                span.Length < bodyStart + bodyLength
                ? (null, 0)
                : (ForNodeDefinition(WingNodeDefinition.Parse(span.Slice(bodyStart, bodyLength))), bodyStart + bodyLength);
        }
    }
}

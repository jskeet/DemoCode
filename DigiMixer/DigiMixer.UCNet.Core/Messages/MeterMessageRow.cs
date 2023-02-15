namespace DigiMixer.UCNet.Core.Messages;

public class MeterMessageRow<T>
{
    private readonly Func<int, T> valueFetcher;
    public int RowNumber { get; }
    public MeterSource Source { get; }
    public MeterStage Stage { get; }
    public int Offset { get; }
    public int Count { get; }

    public T GetValue(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        return valueFetcher(Offset + index);
    }

    internal MeterMessageRow(byte[] rowData, int rowNumber, Func<int, T> valueFetcher)
    {
        RowNumber = rowNumber;
        this.valueFetcher = valueFetcher;
        var rowSpan = ((ReadOnlySpan<byte>) rowData).Slice(rowNumber * 6, 6);
        Source = (MeterSource) rowSpan[0];
        Stage = (MeterStage) rowSpan[1];
        Offset = rowSpan.Slice(2).ReadUInt16();
        Count = rowSpan.Slice(4).ReadUInt16();
    }
}

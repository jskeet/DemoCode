public sealed class ReallyObviouslyImmutable
{
    public string Name { get; }

    public int Value { get; }

    public ReallyObviouslyImmutable(string name, int value)
    {
        this.Name = name;
        this.Value = value;
    }
}
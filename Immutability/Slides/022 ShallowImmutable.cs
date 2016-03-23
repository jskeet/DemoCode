using System;
using System.Text;

public sealed class ShallowImmutable
{
    // Fine
    public int Value { get; }
    // Mutable
    public StringBuilder NameBuilder { get; }
    // See also: collections...

    public ShallowImmutable(int value, StringBuilder nameBuilder)
    {
        Value = value;
        NameBuilder = nameBuilder;
    }
}

public class UsageOfShallowMutable
{
    public static void Main()
    {
        StringBuilder x = new StringBuilder();
        var m = new ShallowImmutable(10, x);
        x.Append("foo");
        m.NameBuilder.Append("bar");
        Console.WriteLine(m.NameBuilder); // foobar
    }
}
public sealed class Mutable
{
    public int Value { get; set; }
}

public class UsageOfMutable
{
    public static void Main()
    {
        var m = new Mutable();
        m.Value = 20;
        m.Value = 30;
    }
}
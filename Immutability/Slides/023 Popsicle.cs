using System;

public sealed class Popsicle
{
    public bool Frozen { get; private set; }

    private int value;
    public int Value
    {
        get { return value; }
        set
        {
            if (Frozen)
            {
                throw new InvalidOperationException("Couldn't keep it in, heaven knows I tried!");
            }
            this.value = value;
        }
    }

    public void Freeze()
    {
        Frozen = true;
    }
}

public class UsageOfPopsicle
{
    public static void Main()
    {
        var popsicle = new Popsicle();
        popsicle.Value = 10;
        popsicle.Freeze();
        popsicle.Value = 20; // Bang!
    }
}
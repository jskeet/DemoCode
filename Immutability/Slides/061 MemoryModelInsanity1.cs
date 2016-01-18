using System;
using System.Threading;

public class MemoryModelInsanity1
{
    // How can this throw a NullReferenceException?
    public static int Foo(string x)
    {
        return x == null ? -1 : x.Length * 2;
    }
}

// In theory... (but not reality)
public class Crazy
{
    private string name;

    public static void Main()
    {
        Crazy crazy = new Crazy();
        for (int i = 0; i < 5; i++)
        {
            new Thread(crazy.BreakTheWorld).Start();
        }
        Thread.Sleep(10000);
    }

    private void BreakTheWorld()
    {
        long total = 0;
        for (int i = 0; i < 10000000; i++)
        {
            name = null;
            total += MemoryModelInsanity1.Foo(name);
            name = "ouch";
            total += MemoryModelInsanity1.Foo(name);
        }
        Console.WriteLine(total);
    }
}
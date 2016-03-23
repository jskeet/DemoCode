using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class LongRunningLoop
{
    private static readonly object monitor = new object();
    private static bool keepRunning = true;

    static bool KeepRunning
    {
        get
        {
            lock (monitor)
            {
                return keepRunning;
            }
        }
        set
        {
            lock (monitor)
            {
                keepRunning = value;
            }
        }
    }

    static void Main()
    {
        new Thread(Run).Start();
        Thread.Sleep(1000);
        keepRunning = false;
    }

    static void Run()
    {
        int count = 0;
        while (keepRunning)
        {
            count++;
        }
        Console.WriteLine(count);
    }
}

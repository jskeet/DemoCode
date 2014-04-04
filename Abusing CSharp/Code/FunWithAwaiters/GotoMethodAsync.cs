using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithAwaiters
{
    class GotoMethodAsync
    {
        static void Main()
        {
            var executioner = new GotoMethodExecutioner(Entry, Entry, Foo, Bar);
            Console.WriteLine("Starting");
            executioner.Start();
        }

        static async void Entry(MethodLineAction _, MethodGotoAction @goto)
        {
if (!await _())         await @goto("Foo:2");
        }

        static async void Foo(MethodLineAction _, MethodGotoAction @goto)
        {
#line 1
if (!await _())         Console.WriteLine("Foo1");
if (!await _())         Console.WriteLine("Foo2");
if (!await _())         await @goto("Bar:1");
if (!await _())         Console.WriteLine("Foo4");
        }

        static async void Bar(MethodLineAction _, MethodGotoAction @goto)
        {
#line 1
if (!await _())         Console.WriteLine("Bar1");
if (!await _())         Console.WriteLine("Bar2");
if (!await _())         await @goto("Foo:4");
if (!await _())         Console.WriteLine("Bar3");
        }
    }
}

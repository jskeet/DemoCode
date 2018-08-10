---
title: Adding a method that hides an extension method
---
# Adding a method that hides an extension method

The compiler only looks for extension methods if the search for an
applicable instance method has failed. As such, *every* new public
method potentially changes behavior for client code.

If we're lucky, and careful with naming, client extension methods
will match the behavior of new methods. That's far from guaranteed
though.

----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public void Method1() => Console.WriteLine("Method1");
    }
}
```
----
Library code after:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public void Method1() => Console.WriteLine("Method1");
        public void Method2() => Console.WriteLine("Method2");
    }
}
```
----
Client code:
```csharp
using System;
using Library;

namespace Client
{
    internal static class LibraryExtensions
    {
        internal static void Method2(this LibraryClass lc)
        {
            Console.WriteLine("In extension method");
            lc.Method1();
        }
    }

    public class Program
    {
        static void Main()
        {
            new LibraryClass().Method2();
        }
    }
}
```
----
Initial results:
```text
In extension method
Method1
```
----
Results of running Client.exe before recompiling:
```text
In extension method
Method1
```
----
Results of running Client.exe after recompiling:
```text
Method2
```
----
[Back to index](index.md)

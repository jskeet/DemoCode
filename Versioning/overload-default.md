---
title: Overloading: issues with default
---
# Overloading: issues with default

Adding an overload accepting `int` to a method accepting `string`
sounds like it should be fine: no argument could match both `int`
and `string`, right?

Even without conversions, the `default` literal syntax added in C#
7.1 makes this awkward. The client code here calls `Method(default)`
which will convert to both `string` and `int` with no problems -
causing ambiguity after the overload has been added.
----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static void Method(string x) => Console.WriteLine("Overload with string");
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
        public static void Method(string x) => Console.WriteLine("Overload with string");

        public static void Method(int x) => Console.WriteLine("Overload with int");
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
    public class Program
    {
        static void Main()
        {
            LibraryClass.Method(default);
        }
    }
}
```
----
Initial results:
```text
Overload with string
```
----
Results of running Client.exe before recompiling:
```text
Overload with string
```
----
Results of running Client.exe after recompiling:
```text
Client.cs(10,26): error CS0121: The call is ambiguous between the following methods or properties: 'LibraryClass.Method(string)' and 'LibraryClass.Method(int)'
```
----
[Back to index](index.md)

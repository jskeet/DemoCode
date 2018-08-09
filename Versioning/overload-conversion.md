---
title: Overloading: issues with conversions
---
# Overloading: issues with conversions

Adding an overload sounds okay when the types are unrelated, but if
there's an "equally-good" implicit conversion between both target
types, the change can break backward source compatibility.

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
    class Awkward
    {
        public static implicit operator int(Awkward input) => 0;
        public static implicit operator string(Awkward input) => "";
    }

    class Program
    {
        static void Main()
        {
            LibraryClass.Method(new Awkward());
        }
    }
}
```
----
Initial results:
```csharp
Overload with string
```
----
Results of running Client.exe before recompiling:
```csharp
Overload with string
```
----
Results of running Client.exe after recompiling:
```csharp
Client.cs(16,26): error CS0121: The call is ambiguous between the following methods or properties: 'LibraryClass.Method(string)' and 'LibraryClass.Method(int)'
```
----
[Back to index](index.md)

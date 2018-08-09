---
title: Overloading: issues with null
---
# Overloading: issues with null

Adding an overload accepting `Type` to a method accepting `string`
sounds like it should be okay, as no value can be both a `Type`
reference and a `string` reference.

Unfortunately the `null` literal can be converted to both, so a
call to `Method(null)` becomes ambiguous.

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

        public static void Method(Type x) => Console.WriteLine("Overload with Type");
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
            LibraryClass.Method(null);
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
Client.cs(10,26): error CS0121: The call is ambiguous between the following methods or properties: 'LibraryClass.Method(string)' and 'LibraryClass.Method(Type)'
```
----
[Back to index](index.md)

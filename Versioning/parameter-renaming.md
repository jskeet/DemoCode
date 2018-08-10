---
title: Renaming a public method parameter
---
# Renaming a public method parameter

Parameters are effectively part of the API in C#, due to named
arguments. Changing the name of a parameter doesn't break binary
compatibility, but it does break source compatibility.

----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static void Method(int x) => Console.WriteLine(x);
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
        public static void Method(int y) => Console.WriteLine(y);
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
            LibraryClass.Method(x: 20);
        }
    }
}
```
----
Initial results:
```text
20
```
----
Results of running Client.exe before recompiling:
```text
20
```
----
Results of running Client.exe after recompiling:
```text
Client.cs(10,33): error CS1739: The best overload for 'Method' does not have a parameter named 'x'
```
----
[Back to index](index.md)

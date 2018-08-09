---
title: Removing a private method
---
# Removing a private method

Removing a private method is not a breaking change, so long as
anything calling it before is changed to have the same result.

----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static void PublicMethod() => PrivateMethod();

        private static void PrivateMethod() => Console.WriteLine("Output");
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
        public static void PublicMethod() => Console.WriteLine("Output");
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
            LibraryClass.PublicMethod();
        }
    }
}
```
----
Initial results:
```text
Output
```
----
Results of running Client.exe before recompiling:
```text
Output
```
----
Results of running Client.exe after recompiling:
```text
Output
```
----
[Back to index](index.md)

---
title: Reordering public method parameters
---
# Reordering public method parameters

Changing the order of method parameters is in some ways worse than
simply renaming them. Client code using named arguments will still
compile, but the meaning of the call will change - unless the
reordered parameters have retained their meaning "by name", in which
case any client code using *position* arguments will change meaning.


----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static void Method(int x, int y) => Console.WriteLine(x / y);
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
        public static void Method(int y, int x) => Console.WriteLine(y / x);
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
            LibraryClass.Method(x: 20, y: 10);
            LibraryClass.Method(20, 10);
        }
    }
}
```
----
Initial results:
```text
2
2
```
----
Results of running Client.exe before recompiling:
```text
2
2
```
----
Results of running Client.exe after recompiling:
```text
0
2
```
----
[Back to index](index.md)

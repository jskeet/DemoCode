---
title: Overloading: adding a parameter
---
# Overloading: adding a parameter

Adding a method overload with a different number of parameters to
all the existing signatures is *unlikely* to break client code, but
it's still possible. (It becomes far more complicated if generics,
inheritance, optional parameters or parameter arrays get involved.)

In this example, we use a method group conversion which is valid in
the first version of the library, but ambiguous in the second
version: the new `Method` overload can be converted to
`Action<int>`, making the overload to `CallAction` accepting an
`Action<int>` applicable.

----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static void Method() => Console.WriteLine("Parameterless overload");
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
        public static void Method() => Console.WriteLine("Parameterless overload");

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
            CallAction(LibraryClass.Method);
        }

        static void CallAction(Action action) => action();
        static void CallAction(Action<int> action) => action(10);
    }
}
```
----
Initial results:
```text
Parameterless overload
```
----
Results of running Client.exe before recompiling:
```text
Parameterless overload
```
----
Results of running Client.exe after recompiling:
```text
Client.cs(10,13): error CS0121: The call is ambiguous between the following methods or properties: 'Program.CallAction(Action)' and 'Program.CallAction(Action<int>)'
```
----
[Back to index](index.md)

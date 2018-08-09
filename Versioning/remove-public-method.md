# Removing a public method

Removing a public method is a simple breaking change for both source
and binary compatibility.
----Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static void Method() => Console.WriteLine("Output");
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
            LibraryClass.Method();
        }
    }
}
```
----
Initial results:
```csharp
Output
```
----
Results of running Client.exe before recompiling:
```csharp

Unhandled Exception: System.MissingMethodException: Method not found: 'Void Library.LibraryClass.Method()'.
   at Client.Program.Main()
```
----
Results of running Client.exe after recompiling:
```csharp
Client.cs(10,26): error CS0117: 'LibraryClass' does not contain a definition for 'Method'
```
----
[Back to index](index.md)

# Changing a public constant

Any change to a public constant value will only be visible to
clients after they recompile. In most cases this is effectively a
breaking change, and a subtle one at that. However, it's reasonable
to *document* a constant as a default value that may change in
future releases, and that such a change will only be visible after
recompilation.
----Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public const string Constant = "Constant before";
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
        public const string Constant = "Constant after";
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
            Console.WriteLine(LibraryClass.Constant);
        }
    }
}
```
----
Initial results:
```csharp
Constant before
```
----
Results of running Client.exe before recompiling:
```csharp
Constant before
```
----
Results of running Client.exe after recompiling:
```csharp
Constant after
```
----
[Back to index](index.md)

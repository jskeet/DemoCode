---
title: Changing a read-only static field
---
# Changing a read-only static field

Unlike a change to a `const` field, a change to a `static readonly`
field will be detected without recompilation.

----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static readonly string StaticReadOnlyField = "Static read-only field before";
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
        public static readonly string StaticReadOnlyField = "Static read-only field after";
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
            Console.WriteLine(LibraryClass.StaticReadOnlyField);
        }
    }
}
```
----
Initial results:
```text
Static read-only field before
```
----
Results of running Client.exe before recompiling:
```text
Static read-only field after
```
----
Results of running Client.exe after recompiling:
```text
Static read-only field after
```
----
[Back to index](index.md)

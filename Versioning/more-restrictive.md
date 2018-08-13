---
title: Making a parameter more restrictive
---
# Making a parameter more restrictive

Reducing the set of values that are acceptable for a parameter (e.g.
disallowing null values that were previously allowed) will break any
code which assumes that value *is* acceptable.

----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public static int GetLength(string text) =>
            // Treat a null reference like an empty string
            text?.Length ?? 0;
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
        public static int GetLength(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return text.Length;
        }
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
            SaveText(null);
        }

        // Note: text may be null
        static void SaveText(string text)
        {
            int length = LibraryClass.GetLength(text);
            // Imagine this output was going to a file...
            Console.WriteLine($"Saving text length: {length}");
            Console.WriteLine(text ?? "(Null text)");
        }
    }
}
```
----
Initial results:
```text
Saving text length: 0
(Null text)
```
----
Results of running Client.exe before recompiling:
```text

Unhandled Exception: System.ArgumentNullException: Value cannot be null.
Parameter name: text
   at Library.LibraryClass.GetLength(String text)
   at Client.Program.SaveText(String text)
   at Client.Program.Main()
```
----
Results of running Client.exe after recompiling:
```text

Unhandled Exception: System.ArgumentNullException: Value cannot be null.
Parameter name: text
   at Library.LibraryClass.GetLength(String text)
   at Client.Program.SaveText(String text)
   at Client.Program.Main()
```
----
[Back to index](index.md)

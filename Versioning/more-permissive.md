---
title: Making a parameter more permissive
---
# Making a parameter more permissive

The example here changes a parameter from rejecting null values
to accepting null values. That sounds fine, except callers may be
relying on that aspect of behavior to validate their own parameters.

----
Library code before:
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
Library code after:
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
            // We expect this to throw
            SaveText(null);
        }

        // Note: text should not be null
        static void SaveText(string text)
        {
            int length = LibraryClass.GetLength(text);
            // Imagine this output was going to a file...
            Console.WriteLine($"Saving text length: {length}");
            Console.WriteLine(text);
        }
    }
}
```
----
Initial results:
```text

Unhandled Exception: System.ArgumentNullException: Value cannot be null.
Parameter name: text
   at Library.LibraryClass.GetLength(String text)
   at Client.Program.SaveText(String text)
   at Client.Program.Main()
```
----
Results of running Client.exe before recompiling:
```text
Saving text length: 0

```
----
Results of running Client.exe after recompiling:
```text
Saving text length: 0

```
----
[Back to index](index.md)

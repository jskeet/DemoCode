---
title: Implementing an interface
---
# Implementing an interface

When a public class implements an interface that it didn't before,
that can affect overload resolution and a host of other binding
issues.

In this example, we don't change which methods are present on
`LibraryClass` - we just make it implement `IDisposable`. This
changes which overload of `ClientMethod` is called. In this
particular case the code still compiles; in other cases we could
have introduced ambiguity in overload resolution. Likewise, in this
particular case, it could well be that the client code is fine using
the different overload - it could change behaviour in terms of which
method is being called, but not in terms of overall result.
Unfortunately, we rarely get to know that.



----
Library code before:
```csharp
using System;

namespace Library
{
    public class LibraryClass
    {
        public void Dispose() {}
    }
}
```
----
Library code after:
```csharp
using System;

namespace Library
{
    public class LibraryClass : IDisposable
    {
        public void Dispose() {}
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
            ClientMethod(new LibraryClass());
        }

        static void ClientMethod(object obj) => Console.WriteLine("Method with obj parameter");
        static void ClientMethod(IDisposable obj) => Console.WriteLine("Method with IDisposable parameter");
    }
}
```
----
Initial results:
```text
Method with obj parameter
```
----
Results of running Client.exe before recompiling:
```text
Method with obj parameter
```
----
Results of running Client.exe after recompiling:
```text
Method with IDisposable parameter
```
----
[Back to index](index.md)

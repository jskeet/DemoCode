Each file in this directory represents an example of a change which
may or may not be breaking. The format of each file is:

    before
    ----
    after
    ----
    client
    ----
    notes
    
The "before" and "after" represent code within a library, and
"client" represents code that is using that library. Notes are
written in Markdown.

The tooling runs through the following steps:

- Extract code to Before.cs, After.cs, Client.cs and notes.md
- Compile Before.cs to Library.dll
- Compile Client.cs to Client.exe, referencing Library.dll
- Run Client.exe, capturing output to before-result.txt
- Compile After.cs to Library.dll
- Run Client.exe (without recompilation), capturing output to after-result-1.txt
- Recompile Client.cs to Client.exe, referencing Library.dll
- If compilation succeeds, run Client.exe, capturing output to after-result-2.txt

All compilations other than the final one should succeed. The final
one can definitely fail, indicating a source-breaking change. The
first run should succeed; the second one may fail, indicating a
binary-breaking change.

The code extraction uses the following rules:

- Every code sample starts with `using` directives for common
namespaces
- The library code is always automatically in a `Library` namespace;
the client code is always automatically in a `Client` namespace
- A directive of `#main` in client code generates a class (`Program`) with a
`Main` entry point containing all the subsequent code

Code compilation currently always uses `/langVersion:latest`.

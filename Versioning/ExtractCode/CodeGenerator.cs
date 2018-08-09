using System;
using System.Linq;
using System.Text;

namespace ExtractCode
{
    class CodeGenerator
    {
        internal static CodeGenerator Library = new CodeGenerator("Library", "LibraryClass");
        internal static CodeGenerator Client = new CodeGenerator("Client", "Program", "Library");

        internal static readonly string[] DefaultUsings = { "System" };

        internal string Namespace { get; }
        internal string DefaultClass { get; }
        internal string[] Usings { get; }

        internal CodeGenerator(string ns, string defaultClass, params string[] usings)
        {
            Namespace = ns;
            DefaultClass = defaultClass;
            Usings = usings;
        }

        internal string Generate(string source)
        {
            Action ending = null;
            var lines = source.Split('\n').ToList();
            if (lines.Last() == "")
            {
                lines.RemoveAt(lines.Count - 1);
            }
            var builder = new CodeBuilder();
            foreach (var ns in DefaultUsings.Concat(Usings))
            {
                builder.WriteLine($"using {ns};");
            }
            builder.WriteLine();
            builder.WriteLine($"namespace {Namespace}");
            builder.OpenBlock();
            foreach (var line in lines)
            {
                if (line == "#main")
                {
                    builder.WriteLine($"public class {DefaultClass}");
                    builder.OpenBlock();
                    builder.WriteLine("static void Main()");
                    builder.OpenBlock();
                    ending += () => { builder.CloseBlock(); builder.CloseBlock(); };
                }
                else if (line == "#class")
                {
                    builder.WriteLine($"public class {DefaultClass}");
                    builder.OpenBlock();
                    ending += () => { builder.CloseBlock(); };
                }
                else
                {
                    builder.WriteLine(line);
                }
            }
            ending?.Invoke();
            builder.CloseBlock();
            return builder.ToString();
        }

        private class CodeBuilder
        {
            private readonly StringBuilder builder = new StringBuilder();
            private int indentation = 0;

            internal void WriteLine()
            {
                // No indentation for blank lines
                builder.Append('\n');
            }

            internal void WriteLine(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    WriteLine();
                    return;
                }
                builder.Append(new string(' ', indentation));
                builder.Append(text);
                builder.Append('\n');
            }

            internal void Indent() => indentation += 4;
            internal void Outdent() => indentation -= 4;

            internal void OpenBlock()
            {
                WriteLine("{");
                Indent();
            }

            internal void CloseBlock()
            {
                Outdent();
                WriteLine("}");
            }

            public override string ToString() => builder.ToString();
        }
    }
}

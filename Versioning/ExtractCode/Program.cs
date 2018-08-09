using System;
using System.IO;

namespace ExtractCode
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Required arguments: source-file output-root-directory");
                return 1;
            }

            string sourceFile = args[0];
            string outputRoot = args[1];

            string outputDirectory = Path.Combine(outputRoot, Path.GetFileNameWithoutExtension(sourceFile));
            if (Directory.Exists(outputDirectory))
            {
                Console.WriteLine($"{outputDirectory} already exists. Aborting.");
                return 1;
            }
            Directory.CreateDirectory(outputDirectory);

            // Normalize line endings for simplicity
            string sourceText = File.ReadAllText(sourceFile).Replace("\r\n", "\n");
            string[] sections = sourceText.Split("----\n", 4);
            if (sections.Length != 4)
            {
                Console.WriteLine($"Expected 4 sections in source code; got {sections.Length}");
                return 1;
            }
            string before = CodeGenerator.Library.Generate(sections[0]);
            string after = CodeGenerator.Library.Generate(sections[1]);
            string client = CodeGenerator.Client.Generate(sections[2]);
            string notes = sections[3];

            WriteFile("Before.cs", before);
            WriteFile("After.cs", after);
            WriteFile("Client.cs", client);
            WriteFile("notes.md", notes);            
            return 0;

            void WriteFile(string outputFile, string text) =>
                File.WriteAllText(Path.Combine(outputDirectory, outputFile), text);
        }        
    }
}

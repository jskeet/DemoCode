using System.Reflection;

namespace DigiMixer.Diagnostics;

/// <summary>
/// Simple tool processing. A tool class with a name of X and a public constructor with
/// string parameters Y and Z will be executed by passing { "X", "y-value", "z-value" } to 
/// <see cref="ExecuteFromCommandLine(string[], Type)"/>.
/// </summary>
public abstract class Tool
{
    public static async Task<int> ExecuteFromCommandLine(string[] args, Type typeInAssembly)
    {
        var tools = typeInAssembly.Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(Tool)) && !t.IsAbstract);
        if (args.Length == 0)
        {
            DisplayTools(tools);
            return 0;
        }
        var toolType = tools.SingleOrDefault(tool => tool.Name == args[0]);
        if (toolType is null)
        {
            Console.WriteLine($"Tool '{args[0]}' not found");
            DisplayTools(tools);
            return 1;
        }
        var ctor = toolType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .Where(ctor => ctor.GetParameters().Length == args.Length - 1)
            .FirstOrDefault();
        if (ctor is null)
        {
            Console.WriteLine($"Tool '{args[0]}' has no constructor of that length. Constructors:");
            foreach (var candidate in toolType.GetConstructors(BindingFlags.Instance | BindingFlags.Public))
            {
                var parameters = candidate.GetParameters();
                Console.WriteLine($"Length {parameters.Length}: {string.Join(", ", parameters.Select(p => p.Name))}");
            }
            return 1;
        }
        var tool = (Tool) ctor.Invoke(args.Skip(1).ToArray());
        return await tool.Execute();
    }

    private static void DisplayTools(IEnumerable<Type> tools)
    {
        Console.WriteLine("Available tools:");
        foreach (var toolType in tools)
        {
            var ctor = toolType.GetConstructors().Single();
            Console.WriteLine($"{toolType.Name}: {string.Join(", ", ctor.GetParameters().Select(p => p.Name))}");
        }
    }

    public abstract Task<int> Execute();
}

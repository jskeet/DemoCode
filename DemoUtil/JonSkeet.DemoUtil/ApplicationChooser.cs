// Copyright 2017 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JonSkeet.DemoUtil
{
    /// <summary>
    /// Class allowing a user to choose a console application to run, after
    /// examining an assembly for all classes containing a static Main method, either
    /// parameterless or with a string array parameter.
    /// The options are presented in a list ordered first by those with a Description
    /// attribute (by that description) and then those without (ordered by name).
    /// </summary>
    public class ApplicationChooser
    {
        const string Keys = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Displays entry points and prompts the user to choose one.
        /// The assembly to find types in is the entry assembly.
        /// </summary>
        /// <param name="args">Arguments to pass in for methods which have a single string array parameter.</param>
        public static void Run(string[] args) => Run(Assembly.GetEntryAssembly(), args);

        /// <summary>
        /// Displays entry points and prompts the user to choose one.
        /// The assembly to find types in is the entry assembly.
        /// An empty set of arguments will be passed to any Main method
        /// with a string array parameter
        /// </summary>
        public static void Run() => Run(Assembly.GetEntryAssembly(), new string[0]);

        /// <summary>
        /// Displays entry points and prompts the user to choose one.
        /// </summary>
        /// <param name="type">Type within the assembly containing the applications. This type is
        /// not included in the list of entry points to run.</param>
        /// <param name="args">Arguments to pass in for methods which have a single string array parameter.</param>
        public static void Run(Type type, string[] args) =>
            Run(type.GetTypeInfo().Assembly, args);

        private static void Run(Assembly assembly, string[] args)
        {
            var options = assembly.DefinedTypes
                .Select(t => GetEntryPoint(t))
                .Where(ep => ep != null && ep != assembly.EntryPoint)
                .OrderBy(ep => GetDescription(ep.DeclaringType))
                .Zip(Keys, (ep, k) => new { Key = k, EntryPoint = ep })
                .ToList();

            if (options.Count == 0)
            {
                Console.WriteLine("No entry points found. Press return to exit.");
                Console.ReadLine();
                return;
            }

            while (true)
            {
                foreach (var option in options)
                {
                    Console.WriteLine($"{option.Key}: {GetEntryPointName(option.EntryPoint)}");
                }
                Console.WriteLine();
                Console.Write("Entry point to run (or hit return to quit)? ");

                MethodBase entryPoint = null;
                do
                {
                    Console.Out.Flush();
                    char key = char.ToUpperInvariant(Console.ReadKey().KeyChar);
                    Console.WriteLine();

                    if (key == '\r' || key == '\n')
                    {
                        return;
                    }

                    var selected = options.FirstOrDefault(o => o.Key == key);
                    if (selected == null)
                    {
                        Console.Write("Invalid choice; please select again: ");
                    }
                    else
                    {
                        entryPoint = selected.EntryPoint;
                    }
                } while (entryPoint == null);
                try
                {
                    MethodBase main = entryPoint;
                    Task task = main.Invoke(null, main.GetParameters().Length == 0 ? null : new object[] { args }) as Task;
                    if (task != null)
                    {
                        task.GetAwaiter().GetResult();
                    }
                }
                catch (Exception e)
                {
                    // Normally we fail due to an exception within the
                    // code invoked via reflection.
                    // Unwrap the TargetInvocationException that would otherwise
                    // be wrapped in.
                    if (e is TargetInvocationException tie)
                    {
                        e = tie.InnerException;
                    }
                    Console.WriteLine("Exception: {0}", e);
                }
                Console.WriteLine();
                Console.WriteLine("(Finished; press return.)");
                Console.ReadLine();
            }
        }

        private static int CompareMethods(MethodBase first, MethodBase second)
        {
            Type firstType = first.DeclaringType;
            Type secondType = second.DeclaringType;

            string firstDescription = GetDescription(firstType);
            string secondDescription = GetDescription(secondType);

            if (firstDescription == null && secondDescription == null)
            {
                return firstType.Name.CompareTo(secondType.Name);
            }
            return firstDescription == null ? 1
                 : secondDescription == null ? -1
                 : StringComparer.Ordinal.Compare(firstDescription, secondDescription);
        }

        private static object GetEntryPointName(MethodBase methodBase)
        {
            Type type = methodBase.DeclaringType;
            string description = GetDescription(type);
            return description == null ? type.Name : $"[{description}] {type.Name}";
        }

        private static string GetDescription(Type type) =>
            type.GetTypeInfo()
                .GetCustomAttributes<DescriptionAttribute>()
                .FirstOrDefault()?.Description;

        /// <summary>
        /// Returns the entry point for a method, or null if no entry points can be used.
        /// An entry point taking string[] is preferred to one with no parameters.
        /// </summary>
        internal static MethodBase GetEntryPoint(TypeInfo type)
        {
            if (type.IsGenericTypeDefinition || type.IsGenericType)
            {
                return null;
            }

            var methods = type.DeclaredMethods
                .Where(m => m.IsStatic && m.Name == "Main" && !m.IsGenericMethodDefinition);

            MethodInfo parameterless = null;
            MethodInfo stringArrayParameter = null;

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    parameterless = method;
                }
                else
                {
                    if (parameters.Length == 1 &&
                        !parameters[0].IsOut &&
                        !parameters[0].IsOptional &&
                        parameters[0].ParameterType == typeof(string[]))
                    {
                        stringArrayParameter = method;
                    }
                }
            }

            // Prefer the version with parameters, return null if neither have been found
            return stringArrayParameter ?? parameterless;
        }
    }
}

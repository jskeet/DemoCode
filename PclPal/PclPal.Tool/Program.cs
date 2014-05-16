// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using PclPal.Model;

namespace PclPal.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: pclpal <command> [options]. Valid commands:");
                Console.WriteLine("  list: Lists all profiles");
                Console.WriteLine("  dll <file>: displays the profile in the given DLL");
                Console.WriteLine("  show <profile>: displays the details of the named profile");
                return;
            }
            switch (args[0])
            {
                case "list":
                    foreach (var profile in Profile.LoadAll())
                    {
                        Console.WriteLine("{0}: ({1})", profile.Name, string.Join(", ", profile.SupportedRuntimes));
                    }
                    break;
                case "dll":
                    if (args.Length != 2)
                    {
                        Console.WriteLine("dll command takes a single parameter; the name of the DLL to load");
                        return;
                    }
                    DisplayDllProfile(args[1]);
                    break;
                case "show":
                    if (args.Length != 2)
                    {
                        Console.WriteLine("show command takes a single parameter; the name of the profile to display");
                        return;
                    }
                    DisplayNamedProfile(args[1]);
                    break;
                default:
                    Console.WriteLine("Unknown command: '{0}'", args[0]);
                    break;
            }
        }

        static void DisplayDllProfile(string dll)
        {
            if (!File.Exists(dll))
            {
                Console.WriteLine("File '{0}' not found", dll);
                return;
            }
            var assembly = Assembly.ReflectionOnlyLoadFrom(dll);
            var frameworkNameText = assembly.GetCustomAttributesData()
                                            .Where(c => c.AttributeType == typeof(TargetFrameworkAttribute))
                                            .Select(c => (string) c.ConstructorArguments[0].Value).FirstOrDefault();
            if (frameworkNameText == null)
            {
                Console.WriteLine("Assembly '{0}' does not contain TargetFrameworkAttribute");
                return;
            }
            var profileName = new FrameworkName(frameworkNameText).Profile;
            Console.WriteLine("Profile name: {0}", profileName);
            DisplayNamedProfile(profileName);
        }

        static void DisplayNamedProfile(string profileName)
        {
            var profile = Profile.LoadAll().FirstOrDefault(p => p.Name == profileName);
            if (profile == null)
            {
                Console.WriteLine("Unable to find profile.");
            }
            Console.WriteLine("Supported runtimes:");
            foreach (var runtime in profile.SupportedRuntimes)
            {
                Console.WriteLine(runtime.Id);
                foreach (var attribute in runtime.AttributeNames.Where(attr => attr != "Identifier"))
                {
                    Console.WriteLine("  {0} = {1}", attribute, runtime[attribute]);
                }
            }
        }
    }
}

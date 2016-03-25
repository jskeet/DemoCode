// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using Shed.Controllers;
using System;
using System.Linq;

namespace Shed.CommandLine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // General help
            if (args.Length == 0 || (args[0] == "help" && args.Length == 1))
            {
                Console.WriteLine("Shed automation CLI. Available controllers and commands:");
                ListControllers();
                return;
            }
            else if (args[0] == "help") // Controller specific help...
            {
                var controller = GetController(args[1]);
                if (controller == null)
                {
                    Console.WriteLine($"No such controller: {args[1]}");
                    ListControllers();
                    return;
                }
                ListCommands(controller);
                return;
            }
            else
            {
                var controller = GetController(args[0]);
                if (controller == null)
                {
                    Console.WriteLine($"No such controller: {args[0]}");
                    ListControllers();
                    return;
                }

                if (args.Length == 1)
                {
                    ListCommands(controller);
                    return;
                }
                
                var command = controller.Commands.SingleOrDefault(c => c.Name.Equals(args[1], StringComparison.CurrentCultureIgnoreCase));
                if (command == null)
                {
                    Console.WriteLine($"No such command: {args[1]}");
                    ListCommands(controller);
                    return;
                }

                // Great, we can actually do something...
                try
                {
                    command.Execute(args.Skip(2).ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                }
            }
        }

        private static IController GetController(string name)
        {
            return Factory.AllControllers.SingleOrDefault(c => c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        private static void ListControllers()
        {
            foreach (var controller in Factory.AllControllers)
            {
                Console.WriteLine($"{controller.Name}: {string.Join(", ", controller.Commands.Select(cmd => cmd.Name))}");
            }
        }

        private static void ListCommands(IController controller)
        {
            Console.WriteLine($"Commands for {controller.Name}");
            foreach (var command in controller.Commands)
            {
                Console.WriteLine($"{command.Name}: {command.Description}");
            }
        }
    }
}

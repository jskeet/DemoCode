// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Extension methods for Command objects.
    /// </summary>
    internal static class Commands
    {
        internal static void AddOptionalOption()
        {
        }

        internal static Command AddRequiredOption<T>(this Command command, string name, string description)
        {
            command.Add(new Option<T>(name) { Description = description, Required = true });
            return command;
        }

        internal static Command AddOptionalOption<T>(this Command command, string name, string description, T defaultValue)
        {
            command.Add(new Option<T>(name) { Description = description, Required = false });
            return command;
        }
    }
}

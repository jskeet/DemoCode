// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

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

        internal static Command AddRequiredOption<T>(this Command command, string alias, string description)
        {
            command.AddOption(new Option(alias, description) { Argument = new Argument<T>(), Required = true });
            return command;
        }

        internal static Command AddOptionalOption<T>(this Command command, string alias, string description, T defaultValue)
        {
            command.AddOption(new Option(alias, description) { Argument = new Argument<T>(() => defaultValue), Required = false });
            return command;
        }
    }
}

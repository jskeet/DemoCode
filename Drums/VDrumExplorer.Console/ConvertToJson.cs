// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Json;

namespace VDrumExplorer.Console;

internal sealed class ConvertToJsonCommand : SynchronousCommandLineAction
{
    internal static Command Command { get; } = new Command("convert-to-json")
    {
        Description = "Converts a kit or module from the binary file, saving it as JSON",
        Action = new ConvertToJsonCommand(),
    }
    .AddRequiredOption<string>("--input", "File to load")
    .AddRequiredOption<string>("--output", "File to save");

    public override int Invoke(ParseResult parseResult)
    {
        var input = parseResult.GetRequiredValue<string>("input");
        var output = parseResult.GetRequiredValue<string>("output");

        using var inputStream = File.OpenRead(input);
        var model = Proto.ProtoIo.ReadModel(inputStream, NullLogger.Instance);
        using var writer = File.CreateText(output);

        switch (model)
        {
            case Kit kit:
                kit.SaveAsJson(writer);
                break;
            case Module module:
                module.SaveAsJson(writer);
                break;
            default:
                throw new Exception("Not a kit or module file");
        }

        return 0;
    }
}

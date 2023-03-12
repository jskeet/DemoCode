// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Json;

namespace VDrumExplorer.Console;

internal sealed class ConvertToJsonCommand : ICommandHandler
{
    internal static Command Command { get; } = new Command("convert-to-json")
    {
        Description = "Converts a kit or module from the binary file, saving it as JSON",
        Handler = new ConvertToJsonCommand(),
    }
    .AddRequiredOption<string>("--input", "File to load")
    .AddRequiredOption<string>("--output", "File to save");

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var input = context.ParseResult.ValueForOption<string>("input");
        var output = context.ParseResult.ValueForOption<string>("output");

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

        return Task.FromResult(0);
    }
}

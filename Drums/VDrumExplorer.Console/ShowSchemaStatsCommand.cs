// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Command to show diagnostic stats about schemas - total number of field
    /// containers, overlays etc.
    /// </summary>
    internal sealed class ShowSchemaStatsCommand : SynchronousCommandLineAction
    {
        internal static Command Command { get; } = new Command("show-schema-stats")
        {
            Description = "Shows statistics about a module schema",
            Action = new ShowSchemaStatsCommand()
        }
        .AddRequiredOption<string>("--schema", "Name of schema to show, e.g. TD-27, or 'all'")
            ;

        public override int Invoke(ParseResult parseResult)
        {
            var schemas = GetSchemasToDisplay(parseResult);
            if (schemas.Length == 0)
            {
                return 1;
            }
            var console = parseResult.InvocationConfiguration.Output;
            foreach (var schema in schemas)
            {
                var allContainers = schema.PhysicalRoot.DescendantsAndSelf();
                var fieldContainers = allContainers.OfType<FieldContainer>();
                var containerContainers = allContainers.OfType<ContainerContainer>();
                var allFields = fieldContainers.SelectMany(fc => fc.Fields);
                var distinctFields = allFields.Distinct();
                var allOverlayFields = allFields.OfType<OverlayField>().SelectMany(overlay => overlay.FieldLists.Values.SelectMany(fl => fl.Fields));
                var allDistinctOverlayFields = allOverlayFields.Distinct();

                console.WriteLine($"Statistics for {schema.Identifier}:");
                console.WriteLine($"Containers: {allContainers.Count()}");
                console.WriteLine($"Field containers: {fieldContainers.Count()}");
                console.WriteLine($"Container containers: {containerContainers.Count()}");
                console.WriteLine($"All (non-overlaid) fields: {allFields.Count()}");
                console.WriteLine($"Distinct (non-overlaid) fields: {distinctFields.Count()}");
                console.WriteLine($"All overlaid fields: {allOverlayFields.Count()}");
                console.WriteLine($"Distinct overlaid fields: {allDistinctOverlayFields.Count()}");
                console.WriteLine($"Logical node descendants: {schema.LogicalRoot.DescendantsAndSelf().Count()}");
                console.WriteLine();
            }

            return 0;
        }

        private ModuleSchema[] GetSchemasToDisplay(ParseResult parseResult)
        {
            var console = parseResult.InvocationConfiguration.Output;
            var schemaName = parseResult.GetRequiredValue<string>("schema");
            if (schemaName == "all")
            {
                return ModuleSchema.KnownSchemas.Values.Select(v => v.Value).ToArray();
            }
            var identifier = ModuleSchema.KnownSchemas.Keys.FirstOrDefault(id => id.Name == schemaName);
            if (identifier is null)
            {
                console.WriteLine($"Unknown schema name: '{schemaName}'");
                return new ModuleSchema[0];
            }
            var schema = ModuleSchema.KnownSchemas[identifier].Value;
            return new[] { schema };
        }
    }
}

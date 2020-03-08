// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    internal static class KitUtilities
    {
        /// <summary>
        /// Reads a kit from a device.
        /// </summary>
        internal static async Task<Kit> ReadKit(ModuleSchema schema, RolandMidiClient client, int kitNumber, IStandardStreamWriter console)
        {
            // Allow up to 30 seconds in total, and 1 second per container.
            var overallToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

            var moduleData = new ModuleData();
            var kitRoot = schema.KitRoots[kitNumber];
            var containers = kitRoot.Context.AnnotateDescendantsAndSelf().Where(c => c.Container.Loadable).ToList();
            console.WriteLine($"Reading {containers.Count} containers from device {schema.Identifier.Name} kit {kitNumber}");
            foreach (var container in containers)
            {
                await PopulateSegment(client, moduleData, container, overallToken, console);
            }
            var clonedData = kitRoot.Context.CloneData(moduleData, schema.KitRoots[1].Context.Address);
            return new Kit(schema, clonedData, kitNumber);
        }

        private static async Task PopulateSegment(RolandMidiClient client, ModuleData data, AnnotatedContainer annotatedContainer, CancellationToken token, IStandardStreamWriter console)
        {
            var timerToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(token, timerToken).Token;
            try
            {
                var segment = await client.RequestDataAsync(annotatedContainer.Context.Address.Value, annotatedContainer.Container.Size, effectiveToken);
                data.Populate(annotatedContainer.Context.Address, segment);
            }
            catch (OperationCanceledException) when (timerToken.IsCancellationRequested)
            {
                console.WriteLine($"Device didn't respond for container {annotatedContainer.Path}; skipping.");
            }
        }

        /// <summary>
        /// Writes a kit to a device.
        /// </summary>
        internal static async Task WriteKit(RolandMidiClient client, Kit kit, int kitNumber, IStandardStreamWriter console)
        {
            var targetKitRoot = kit.Schema.KitRoots[kitNumber];
            var clonedData = kit.KitRoot.Context.CloneData(kit.Data, targetKitRoot.Context.Address);
            var segments = clonedData.GetSegments();
            console.WriteLine($"Writing {segments.Count} containers to device {kit.Schema.Identifier.Name} kit {kitNumber}");

            foreach (var segment in segments)
            {
                client.SendData(segment.Start.Value, segment.CopyData());
                await Task.Delay(40);
            }
        }
    }
}

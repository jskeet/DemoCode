// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Device
{
    /// <summary>
    /// "Normal" implementation of <see cref="IDeviceController"/>, using a <see cref="RolandMidiClient"/>.
    /// </summary>
    public class DeviceController : IDeviceController
    {
        /// <summary>
        /// How long we wait after each write operation for the module to catch up.
        /// TODO: Test this further.
        /// </summary>
        private static readonly TimeSpan WriteDelay = TimeSpan.FromMilliseconds(40);

        /// <summary>
        /// The address for the "current kit" field. Currently the same for all schemas,
        /// but we always refer to it via this constant so we can adjust if necessary.
        /// </summary
        private static readonly ModuleAddress CurrentKitAddress = ModuleAddress.FromLogicalValue(0);

        /// <summary>
        /// How long we're prepared to wait for a single data segment to load.
        /// </summary>
        private readonly TimeSpan loadSegmentTimeout;

        /// <summary>
        /// The underlying client.
        /// </summary>
        private readonly RolandMidiClient client;

        public ModuleSchema Schema { get; }

        public string InputName => client.InputName;

        public string OutputName => client.OutputName;

        public DeviceController(RolandMidiClient client) : this(client, TimeSpan.FromSeconds(1))
        {
        }

        private DeviceController(RolandMidiClient client, TimeSpan loadSegmentTimeout) =>
            (this.client, this.loadSegmentTimeout, Schema) =
            (client, loadSegmentTimeout, ModuleSchema.KnownSchemas[client.Identifier].Value);

        public async Task<int> GetCurrentKitAsync(CancellationToken cancellationToken)
        {
            var segment = await LoadSegment(CurrentKitAddress, 1, cancellationToken);
            return segment.ReadInt32(ModuleOffset.Zero, 1) + 1;
        }

        public Task SetCurrentKitAsync(int kit, CancellationToken cancellationToken)
        {
            var segment = new DataSegment(CurrentKitAddress, new[] { (byte) (kit - 1) });
            return SaveSegment(segment, cancellationToken);
        }

        public async Task<Kit> LoadKitAsync(int kit, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken)
        {
            var kitRoot = Schema.KitRoots[kit - 1];
            var snapshot = await LoadDescendantsAsync(kitRoot, progressHandler, cancellationToken);
            snapshot = snapshot.Relocated(kitRoot, Schema.KitRoots[0]);
            return Kit.FromSnapshot(Schema, snapshot, kit);
        }

        public async Task<Module> LoadModuleAsync(IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken)
        {
            var snapshot = await LoadDescendantsAsync(Schema.LogicalRoot, progressHandler, cancellationToken);
            return Module.FromSnapshot(Schema, snapshot);
        }

        public void PlayNote(int channel, int note, int velocity) => client.PlayNote(channel, note, velocity);

        public void Silence(int channel) => client.Silence(channel);

        public Task SaveDescendants(DataTreeNode node, ModuleAddress? targetAddress, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken)
        {
            var containers = node.SchemaNode.DescendantFieldContainers().OrderBy(fc => fc.Address).ToList();
            var snapshot = node.Data.CreatePartialSnapshot(node.SchemaNode);
            if (targetAddress is ModuleAddress target)
            {
                snapshot = snapshot.Relocated(node.SchemaNode.Container.Address, target);
            }
            return SaveSnapshot(snapshot, containers, progressHandler, cancellationToken);
        }

        public async Task LoadDescendants(DataTreeNode node, ModuleAddress? targetAddress, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken)
        {
            var snapshot = await LoadDescendantsAsync(node.SchemaNode, progressHandler, cancellationToken);
            if (targetAddress is ModuleAddress target)
            {
                snapshot = snapshot.Relocated(node.SchemaNode.Container.Address, target);
            }
            node.Data.LoadPartialSnapshot(snapshot);
        }

        private async Task<ModuleDataSnapshot> LoadDescendantsAsync(TreeNode root, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken)
        {
            var containers = root.DescendantFieldContainers().ToList();
            var snapshot = new ModuleDataSnapshot();
            int completed = 0;
            foreach (var container in containers)
            {
                progressHandler?.Report(new TransferProgress(completed, containers.Count, $"Copying {container.Path}"));
                var segment = await LoadSegment(container.Address, container.Size, cancellationToken);
                completed++;
                snapshot.Add(segment);
            }
            progressHandler?.Report(new TransferProgress(containers.Count, containers.Count, "Complete"));
            return snapshot;
        }

        private async Task<DataSegment> LoadSegment(ModuleAddress address, int size, CancellationToken cancellationToken)
        {
            var timerToken = new CancellationTokenSource(loadSegmentTimeout).Token;
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timerToken).Token;
            var data = await client.RequestDataAsync(address.DisplayValue, size, effectiveToken);
            return new DataSegment(address, data);
        }

        // Assumption: the list of containers is exactly the same as the segments in the snapshot.
        // We just use this so that we can report the field path instead of the address.
        // (An alternative would be a map from address to path...)
        private async Task SaveSnapshot(ModuleDataSnapshot snapshot, IReadOnlyList<FieldContainer> containers, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken)
        {
            int completed = 0;
            foreach (var container in containers)
            {
                var segment = snapshot[container.Address];
                progressHandler?.Report(new TransferProgress(completed, containers.Count, $"Copying {container.Path}"));
                await SaveSegment(segment, cancellationToken);
                completed++;
            }
            progressHandler?.Report(new TransferProgress(containers.Count, containers.Count, "Complete"));
        }

        private async Task SaveSegment(DataSegment segment, CancellationToken cancellationToken)
        {
            client.SendData(segment.Address.DisplayValue, segment.CopyData());
            await Task.Delay(WriteDelay, cancellationToken);
        }

        public void Dispose() => client.Dispose();
    }
}

// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Model.Schema.Fields;
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

        public async Task<Kit> LoadKitAsync(int kit, IProgress<TransferProgress>? progressHandler, CancellationToken cancellationToken)
        {
            var kitRoot = Schema.GetKitRoot(kit);
            var snapshot = await LoadDescendantsAsync(kitRoot, progressHandler, cancellationToken);
            snapshot = snapshot.Relocated(kitRoot, Schema.Kit1Root);
            return Kit.FromSnapshot(Schema, snapshot, kit);
        }

        public async Task<Module> LoadModuleAsync(IProgress<TransferProgress>? progressHandler, CancellationToken cancellationToken)
        {
            var snapshot = await LoadDescendantsAsync(Schema.LogicalRoot, progressHandler, cancellationToken);
            return Module.FromSnapshot(Schema, snapshot);
        }

        public void PlayNote(int channel, int note, int velocity) => client.PlayNote(channel, note, velocity);

        public void Silence(int channel) => client.Silence(channel);

        public Task SaveDescendants(DataTreeNode node, ModuleAddress? targetAddress, IProgress<TransferProgress>? progressHandler, CancellationToken cancellationToken)
        {
            var containers = node.SchemaNode.DescendantFieldContainers().OrderBy(fc => fc.Address).ToList();
            var snapshot = node.Data.CreatePartialSnapshot(node.SchemaNode);
            int offset = 0;
            if (targetAddress is ModuleAddress target)
            {
                var source = node.SchemaNode.Container.Address;
                snapshot = snapshot.Relocated(source, target);
                offset = target.LogicalValue - source.LogicalValue;
            }
            var addressPaths = containers.ToDictionary(c => c.Address.PlusLogicalOffset(offset), c => c.Path);
            return SaveSnapshot(snapshot, addressPaths, progressHandler, cancellationToken);
        }

        public async Task LoadDescendants(DataTreeNode node, ModuleAddress? targetAddress, IProgress<TransferProgress>? progressHandler, CancellationToken cancellationToken)
        {
            var snapshot = await LoadDescendantsAsync(node.SchemaNode, progressHandler, cancellationToken);
            if (targetAddress is ModuleAddress target)
            {
                snapshot = snapshot.Relocated(node.SchemaNode.Container.Address, target);
            }
            node.Data.LoadPartialSnapshot(snapshot);
        }

        private async Task<ModuleDataSnapshot> LoadDescendantsAsync(TreeNode root, IProgress<TransferProgress>? progressHandler, CancellationToken cancellationToken)
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

        public async Task SetInstrumentAsync(int kit, int trigger, Instrument instrument, CancellationToken cancellationToken)
        {
            var field = Schema.GetMainInstrumentField(kit, trigger);
            var segment = await LoadSegment(field.Parent!.Address, field.Parent!.Size, cancellationToken);
            var dataField = new InstrumentDataField(field, Schema);
            dataField.Instrument = instrument;
            dataField.Save(segment);
            await SaveSegment(segment, cancellationToken);
        }

        private async Task<DataSegment> LoadSegment(ModuleAddress address, int size, CancellationToken cancellationToken)
        {
            var timerToken = new CancellationTokenSource(loadSegmentTimeout).Token;
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timerToken).Token;
            var data = await client.RequestDataAsync(address.DisplayValue, size, effectiveToken);
            return new DataSegment(address, data);
        }

        public async Task<string> LoadKitNameAsync(int kit, CancellationToken cancellationToken)
        {
            var kitRoot = Schema.GetKitRoot(kit);
            var rootContainer = kitRoot.Container;
            var nameField = rootContainer.ResolveField(Schema.KitNamePath);
            var subNameField = rootContainer.ResolveField(Schema.KitSubNamePath);

            var containersToLoad = new[] { nameField.Parent, subNameField.Parent }.Where(c => c is object).Select(c => c!).Distinct();
            var snapshot = new ModuleDataSnapshot();
            foreach (var container in containersToLoad)
            {
                var segment = await LoadSegment(container.Address, container.Size, cancellationToken);
                snapshot.Add(segment);
            }
            var data = ModuleData.FromLogicalRootNode(kitRoot);
            data.LoadPartialSnapshot(snapshot);

            return Kit.GetKitName(data, kitRoot);
        }

        // Assumption: the list of containers is exactly the same as the segments in the snapshot.
        // We just use this so that we can report the field path instead of the address.
        // (An alternative would be a map from address to path...)
        private async Task SaveSnapshot(ModuleDataSnapshot snapshot, Dictionary<ModuleAddress, string> addressPaths, IProgress<TransferProgress>? progressHandler, CancellationToken cancellationToken)
        {
            int completed = 0;
            foreach (var segment in snapshot.Segments)
            {
                progressHandler?.Report(new TransferProgress(completed, snapshot.SegmentCount, $"Copying {addressPaths[segment.Address]}"));
                await SaveSegment(segment, cancellationToken);
                completed++;
            }
            progressHandler?.Report(new TransferProgress(snapshot.SegmentCount, snapshot.SegmentCount, "Complete"));
        }

        private async Task SaveSegment(DataSegment segment, CancellationToken cancellationToken)
        {
            client.SendData(segment.Address.DisplayValue, segment.CopyData());
            await Task.Delay(WriteDelay, cancellationToken);
        }

        public void Dispose()
        {
            try
            {
                client.Dispose();
            }
            catch
            {
                // It's rare to get errors when disposing of the client, but it can happen - and it's not worth reporting.
            }
        }
    }
}

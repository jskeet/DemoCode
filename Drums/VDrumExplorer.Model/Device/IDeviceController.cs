// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Data.Logical;

namespace VDrumExplorer.Model.Device
{
    public interface IDeviceController : IDisposable
    {
        ModuleSchema Schema { get; }
        string InputName { get; }
        string OutputName { get; }

        /// <summary>
        /// Returns the 1-based current kit number on the device.
        /// </summary>
        Task<int> GetCurrentKitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sets the current kit number on the device.
        /// </summary>
        /// <param name="kit">1-based kit number</param>
        Task SetCurrentKitAsync(int kit, CancellationToken cancellationToken);

        /// <summary>
        /// Loads complete module data from the device.
        /// </summary>
        Task<Module> LoadModuleAsync(IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken);

        /// <summary>
        /// Loads the specified kit from the device.
        /// </summary>
        Task<Kit> LoadKitAsync(int kit, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken);

        /// <summary>
        /// Play the specified MIDI note on the device.
        /// </summary>
        /// <param name="channel">The MIDI channel, typically 10.</param>
        /// <param name="note"></param>
        /// <param name="attack">The attack, between 0 and 127.</param>
        void PlayNote(int channel, int note, int attack);

        /// <summary>
        /// Silences all notes on the given MIDI channel.
        /// </summary>
        /// <param name="channel">The MIDI channel, typically 10.</param>
        void Silence(int channel);

        /// <summary>
        /// Copies data from the given node and its descendants from memory onto the device.
        /// </summary>
        Task SaveDescendants(DataTreeNode node, ModuleAddress? targetAddress, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken);

        /// <summary>
        /// Copies data from the given node and its descendants from the device into memory.
        /// </summary>
        Task LoadDescendants(DataTreeNode node, ModuleAddress? targetAddress, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken);
    }
}

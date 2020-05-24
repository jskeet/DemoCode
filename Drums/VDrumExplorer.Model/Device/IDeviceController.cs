// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Threading;
using System.Threading.Tasks;

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

        Task<Module> LoadModuleAsync(IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken);
        Task<Kit> LoadKitAsync(int kit, IProgress<TransferProgress> progressHandler, CancellationToken cancellationToken);

        Task PlayNoteAsync(int channel, int note, int attack);
        Task SilenceAsync(int channel);
    }
}

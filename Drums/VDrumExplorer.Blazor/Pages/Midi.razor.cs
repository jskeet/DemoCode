// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Blazor.WebMidi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.Pages
{
    public partial class Midi
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private string status = "";
        private string inputDevices = "MIDI not initialized";
        private string outputDevices = "MIDI not initialized";
        private List<string> messages = new List<string>();
        private ILogger logger;

        public Midi()
        {
            logger = new SimpleLogger(Log);
        }

        private async Task<bool> MaybeInitializeMidi()
        {
            if (MidiDevices.Manager is object)
            {
                return true;
            }
            try
            {
                MidiDevices.Manager = await WebMidiManager.InitializeAsync(JSRuntime);
                status = "Initialized";
                return true;
            }
            catch (Exception e)
            {
                status = $"Initialization failed: {e.Message}";
                return false;
            }
        }

        private async Task ListKits()
        {
            if (!await MaybeInitializeMidi())
            {
                return;
            }

            inputDevices = string.Join(", ", MidiDevices.ListInputDevices().Select(device => $"{device.SystemDeviceId} ({device.Name})"));
            outputDevices = string.Join(", ", MidiDevices.ListOutputDevices().Select(device => $"{device.SystemDeviceId} ({device.Name})"));

            Log("Detecting Roland devices (this can take a few seconds)");
            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(logger, ModuleSchema.KnownSchemas.Keys);
            if (client is object)
            {
                await Task.Yield();
                var schema = ModuleSchema.KnownSchemas[client.Identifier].Value;
                Log($"Listing {schema.Identifier.Name} kits:");
                var deviceController = new DeviceController(client);
                for (int i = 1; i <= schema.Kits; i++)
                {
                    var name = await deviceController.LoadKitNameAsync(i, CancellationToken.None);
                    Log($"Kit {i}: {name}");
                }
            }
        }

        private void Log(string message)
        {
            messages.Add($"{DateTime.UtcNow:HH:mm:ss.fff} {message}");
            StateHasChanged();
        }
    }
}

// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Gui
{
    public class ModuleLoader : Form
    {
        private readonly TextBox logPanel;
        private readonly Button loadFileButton;
        private readonly Button loadDeviceButton;

        private (SysExClient client, ModuleSchema schema)? detectedMidi;

        public ModuleLoader()
        {
            logPanel = new TextBox
            {
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Multiline = true,
                ReadOnly = true,
            };
            var logGroupBox = new GroupBox
            {
                Text = "Log",
                Controls = { logPanel },
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            loadFileButton = new Button { Text = "Load file", AutoSize = true };
            loadFileButton.Click += LoadFile;
            loadDeviceButton = new Button { Text = "Load from MIDI device", AutoSize = true, Enabled = false };
            loadDeviceButton.Click += LoadFromDevice;
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Controls = { loadFileButton, loadDeviceButton },
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            var panel = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Controls = { buttonPanel, logGroupBox }
            };
            
            Text = "V-Drum Explorer";
            Controls.Add(panel);
            Size = new Size(800, 1000);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            detectedMidi?.client.Dispose();
        }

        protected override async void OnLoad(EventArgs e)
        {
            var midiDevice = await DetectMidiDeviceAsync();
            this.detectedMidi = midiDevice;
            loadDeviceButton.Enabled = detectedMidi.HasValue;
            Log("-----------------");
        }

        private void LoadFile(object sender, EventArgs e)
        {
            string fileName;
            using (OpenFileDialog dialog = new OpenFileDialog { Multiselect = false })
            {
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                fileName = dialog.FileName;
            }

            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var data = ModuleData.FromStream(stream);
                    data.Validate();
                    var client = detectedMidi?.schema == data.Schema ? detectedMidi?.client : null;
                    new ModuleExplorer(data, client).Show();
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        private void LoadFromDevice(object sender, EventArgs e)
        {
            var detected = detectedMidi;
            if (detected == null)
            {
                return;
            }
            var client = detected.Value.client;
            var schema = detected.Value.schema;
            var progress = new DeviceLoaderDialog(client, schema);
            progress.ShowDialog();
            if (progress.LoadedData != null)
            {
                new ModuleExplorer(progress.LoadedData, detectedMidi?.client).Show();
            }
        }

        private async Task<(SysExClient client, ModuleSchema schema)?> DetectMidiDeviceAsync()
        {
            var inputDevices = DeviceInfo.GetInputDevices();
            var outputDevices = DeviceInfo.GetOutputDevices();

            Log($"Detecting MIDI ports");
            Log($"Input ports:");
            foreach (var input in inputDevices)
            {
                Log($"{input.LocalDeviceId}: {input.Name}");
            }
            Log($"Output ports:");
            foreach (var output in outputDevices)
            {
                Log($"{output.LocalDeviceId}: {output.Name}");
            }
            var commonNames = inputDevices.Select(input => input.Name).Intersect(outputDevices.Select(output => output.Name)).OrderBy(x => x).ToList();
            if (commonNames.Count == 0)
            {
                Log($"Not detected any input/output MIDI ports. Abandoning MIDI detection.");
                return null;
            }
            if (commonNames.Count > 1)
            {
                Log($"Detected multiple input/output MIDI ports: {string.Join(",", commonNames)}. Abandoning MIDI detection.");
                return null;
            }
            string name = commonNames.Single();
            var matchedInputs = inputDevices.Where(input => input.Name == name).ToList();
            var matchedOutputs = outputDevices.Where(output => output.Name == name).ToList();
            if (matchedInputs.Count != 1 || matchedOutputs.Count != 1)
            {
                Log($"Matched name {name} is ambiguous. Abandoning MIDI detection.");
                return null;
            }
            Log($"Using MIDI ports with name {name}. Detecting devices using Roland identity requests.");

            var inputId = matchedInputs[0].LocalDeviceId;
            var outputId = matchedOutputs[0].LocalDeviceId;

            ConcurrentBag<IdentityResponse> responses = new ConcurrentBag<IdentityResponse>();
            using (var identityClient = new IdentityClient(inputId, outputId))
            {
                identityClient.IdentityReceived += response => responses.Add(response);
                identityClient.SendRequests();
                // Half a second should be plenty of time.
                await Task.Delay(500);                
            }
            var schemas = SchemaRegistry.GetSchemas();
            var responseList = responses.OrderBy(r => r.DeviceId).ToList();
            ModuleSchema matchedSchema = null;
            IdentityResponse matchedResponse = null;
            int matchCount = 0;
            foreach (var response in responseList)
            {
                var match = schemas.FirstOrDefault(s => response.FamilyCode == s.FamilyCode && response.FamilyNumberCode == s.FamilyNumberCode);
                string matchLog = match == null ? "No matching schema" : $"Matches schema {match.Name}";
                Log($"Detected device ID {response.DeviceId} with family code {response.FamilyCode} ({response.FamilyNumberCode}) : {matchLog}");
                if (match != null)
                {
                    matchedSchema = match;
                    matchedResponse = response;
                    matchCount++;
                }
            }
            switch (matchCount)
            {
                case 0:
                    Log($"No devices with a known schema. Abandoning MIDI detection.");
                    return null;
                case 1:
                    Log($"Using device {matchedResponse.DeviceId} with schema {matchedSchema.Name}.");
                    return (new SysExClient(inputId, outputId, matchedSchema.ModelId, matchedResponse.DeviceId), matchedSchema);
                default:
                    Log($"Multiple devices with a known schema. Abandoning MIDI detection.");
                    return null;
            }
        }

        private void Log(string text) => logPanel.AppendText($"{text}\r\n");

        private void Log(Exception e)
        {
            // TODO: Aggregate exception etc.
            Log(e.ToString());
        }

        private class DeviceLoaderDialog : Form
        {
            private readonly ModuleData data;            
            private readonly SysExClient client;
            private readonly Label label;
            private readonly ProgressBar progress;
            private readonly List<Container> containers;
            public ModuleData LoadedData { get; private set; }

            public DeviceLoaderDialog(SysExClient client, ModuleSchema schema)
            {
                this.client = client;
                containers = schema.Root.DescendantsAndSelf().OfType<Container>().Where(c => c.Loadable).ToList();
                data = new ModuleData(schema);
                label = new Label { TextAlign = ContentAlignment.MiddleCenter, Text = "", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(5) };
                progress = new ProgressBar { Maximum = containers.Count, Dock = DockStyle.Bottom, Padding = new Padding(5) };
                Controls.Add(label);
                Controls.Add(progress);
                Padding = new Padding(5);
                Text = $"Loading {schema.Name}";
                AutoSize = true;
                Size = new Size(500, 0);
                //AutoSizeMode = AutoSizeMode.GrowAndShrink;
            }

            protected override async void OnShown(EventArgs e)
            {
                // TODO: A cancel button.
                // TODO: Error logging
                var tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                int loaded = 0;
                foreach (var container in containers)
                {
                    // TODO: Make RequestDataAsync return a segment.
                    label.Text = $"Loading {container.Path}";
                    var segment = await client.RequestDataAsync(container.Address.Value, container.Size, tokenSource.Token);
                    loaded++;
                    progress.Value = loaded;
                    data.Populate(container.Address, segment);
                }
                LoadedData = data;
                Close();
            }
        }
    }
}

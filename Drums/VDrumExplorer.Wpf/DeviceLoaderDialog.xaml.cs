// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for LoadFromDeviceDialog.xaml
    /// </summary>
    public partial class DeviceLoaderDialog : Window
    {
        private readonly ILogger logger;
        private readonly RolandMidiClient client;
        private readonly ModuleSchema schema;
        private readonly CancellationTokenSource cancellationTokenSource;
        
        public ModuleData Data { get; private set; }

        public DeviceLoaderDialog()
        {
            InitializeComponent();
        }

        internal DeviceLoaderDialog(ILogger logger, RolandMidiClient client, ModuleSchema schema) : this()
        {
            this.logger = logger;
            this.client = client;
            this.schema = schema;
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        }
        
        internal async void LoadDeviceData(FixedContainer root)
        {
            var data = new ModuleData();
            // TODO: Possibly reinstate this in RolandMidiClient.
            // client.UnconsumedMessageHandler += LogUnconsumedMessage;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var containers = root.AnnotateDescendantsAndSelf().Where(c => c.Container.Loadable).ToList();
                logger.LogInformation($"Loading {containers.Count} containers from device {schema.Identifier.Name}");
                progress.Maximum = containers.Count;
                int loaded = 0;
                foreach (var container in containers)
                {
                    // TODO: Make RequestDataAsync return a segment.
                    label.Content = $"Loading {container.Path}";
                    await PopulateSegment(data, container, cancellationTokenSource.Token);
                    loaded++;
                    progress.Value = loaded;
                }
                Data = data;
                DialogResult = true;
                logger.LogInformation($"Finished loading in {(int) sw.Elapsed.TotalSeconds} seconds");
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Data loading from device was cancelled");
                DialogResult = false;
            }
            catch (Exception ex)
            {
                logger.LogError("Error loading data from device", ex);
            }
            finally
            {
                //client.UnconsumedMessageHandler -= LogUnconsumedMessage;
            }

            /*
            void LogUnconsumedMessage(object sender, DataResponseMessage message) =>
                logger.Log($"Unexpected data response. Address: {message.Address:x8}; Length: {message.Length:x8}");
            */
        }

        internal async void CopySegmentsToDeviceAsync(List<DataSegment> segments)
        {
            // Slightly ugly to have this here rather than in XAML, but it's at least simple.
            Title = "Copying data to module";
            Stopwatch sw = Stopwatch.StartNew();
            int written = 0;
            try
            {
                logger.LogInformation($"Writing {segments.Count} segments to the device.");
                progress.Maximum = segments.Count;
                foreach (var segment in segments)
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    client.SendData(segment.Start.Value, segment.CopyData());
                    await Task.Delay(40);
                    written++;
                    progress.Value = written;
                }
                logger.LogInformation($"Finished writing segments to the device {(int) sw.Elapsed.TotalSeconds} seconds.");
                DialogResult = true;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning($"Data copying to device was cancelled. Warning: module may now have inconsistent data.");
                DialogResult = false;
            }
            catch (Exception e)
            {
                logger.LogError("Failed while writing data to the device.");
                logger.LogInformation($"Segments successfully written: {written}");
                logger.LogError($"Error: {e}");
                DialogResult = false;
            }
        }

        private async Task PopulateSegment(ModuleData data, AnnotatedContainer annotatedContainer, CancellationToken token)
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
                logger.LogWarning($"Device didn't respond for container {annotatedContainer.Path}; skipping.");
            }
            catch
            {
                logger.LogError($"Failure while loading {annotatedContainer.Path}");
                throw;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e) =>
            cancellationTokenSource.Cancel();
    }
}

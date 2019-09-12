// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
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
        private readonly SysExClient client;
        private readonly ModuleSchema schema;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ModuleData data;
        
        public Module Module { get; private set; }

        public DeviceLoaderDialog()
        {
            InitializeComponent();
        }

        internal DeviceLoaderDialog(ILogger logger, SysExClient client, ModuleSchema schema) : this()
        {
            this.logger = logger;
            this.client = client;
            this.schema = schema;
            data = new ModuleData();
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            Loaded += LoadDeviceData;
        }
        
        private async void LoadDeviceData(object sender, RoutedEventArgs e)
        {
            client.UnconsumedMessageHandler += LogUnconsumedMessage;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var containers = schema.Root.AnnotateDescendantsAndSelf().Where(c => c.Container.Loadable).ToList();
                logger.Log($"Loading {containers.Count} containers from device {schema.Identifier.Name}");
                progress.Maximum = containers.Count;
                int loaded = 0;
                foreach (var container in containers)
                {
                    // TODO: Make RequestDataAsync return a segment.
                    label.Content = $"Loading {container.Path}";
                    await PopulateSegment(container, cancellationTokenSource.Token);
                    loaded++;
                    progress.Value = loaded;
                }
                Module = new Module(schema, data);
                DialogResult = true;
                logger.Log($"Finished loading in {(int) sw.Elapsed.TotalSeconds} seconds");
            }
            catch (OperationCanceledException)
            {
                logger.Log("Data loading from device was cancelled");
                DialogResult = false;
            }
            catch (Exception ex)
            {
                logger.Log("Error loading data from device", ex);
            }
            finally
            {
                client.UnconsumedMessageHandler -= LogUnconsumedMessage;
            }

            void LogUnconsumedMessage(object sender, DataResponseMessage message) =>
                logger.Log($"Unexpected data response. Address: {message.Address:x8}; Length: {message.Length:x8}");
        }

        private async Task PopulateSegment(AnnotatedContainer annotatedContainer, CancellationToken token)
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
                logger.Log($"Device didn't respond for container {annotatedContainer.Path}; skipping.");
            }
            catch
            {
                logger.Log($"Failure while loading {annotatedContainer.Path}");
                throw;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e) =>
            cancellationTokenSource.Cancel();
    }
}

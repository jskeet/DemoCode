using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var containers = schema.Root.DescendantsAndSelf().OfType<Container>().Where(c => c.Loadable).ToList();
                logger.Log($"Loading {containers.Count} containers from device {schema.Name}");
                progress.Maximum = containers.Count;
                int loaded = 0;
                foreach (var container in containers)
                {
                    // TODO: Make RequestDataAsync return a segment.
                    label.Content = $"Loading {container.Path}";
                    var segment = await client.RequestDataAsync(container.Address.Value, container.Size, cancellationTokenSource.Token);
                    loaded++;
                    progress.Value = loaded;
                    data.Populate(container.Address, segment);
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
        }

        private void Cancel(object sender, RoutedEventArgs e) =>
            cancellationTokenSource.Cancel();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Audio;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for InstrumentAudioExplorer.xaml
    /// </summary>
    public partial class InstrumentAudioExplorer : Window
    {
        private readonly ModuleAudio audio;
        private readonly ILogger logger;
        private readonly ILookup<InstrumentGroup, InstrumentAudio> capturesByGroup;

        public InstrumentAudioExplorer()
        {
            InitializeComponent();
        }

        internal InstrumentAudioExplorer(ILogger logger, ModuleAudio audio) : this()
        {
            this.audio = audio;
            this.logger = logger;
            capturesByGroup = audio.Captures.ToLookup(c => c.Instrument.Group);
            var allOutputDeviceNames = AudioDevices.GetOutputDeviceNames();
            outputDevice.ItemsSource = allOutputDeviceNames;

            // Assume that device 0 is the default. That will usually be the case.
            if (allOutputDeviceNames.Count > 0)
            {
                outputDevice.SelectedIndex = 0;
            }

            moduleId.Content = audio.Schema.Identifier.Name;
            userSamples.Content = TextConversions.Format(capturesByGroup[null].Count());
            var format = audio.Format;
            audioFormat.Content = $"Channels: {format.Channels}; Bits: {format.Bits}; Frequency: {format.Frequency}";
            timePerInstrument.Content = TextConversions.Format(audio.DurationPerInstrument.TotalSeconds);

            var groups = capturesByGroup.Select(c => new InstrumentGroupOrUserSample(c.Key)).Distinct();
            treeView.ItemsSource = groups;
            instrumentsGroupBox.Visibility = Visibility.Collapsed;
        }

        private void HandleTreeViewSelection(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(treeView.SelectedItem is InstrumentGroupOrUserSample model))
            {
                instrumentsGroupBox.Visibility = Visibility.Collapsed;
                return;
            }
            instrumentsGroupBox.Visibility = Visibility.Visible;
            instrumentsGroupBox.Header = model.ToString();
            instrumentsGrid.Children.Clear();
            instrumentsGrid.RowDefinitions.Clear();
            foreach (var capture in capturesByGroup[model.Group])
            {
                var label = new Label
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(2, 1, 5, 0),
                    Content = capture.Instrument.Name,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var button = new Button
                {
                    Content = "Play",
                    Tag = capture,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(2),
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                button.Click += PlaySample;
                Grid.SetRow(label, instrumentsGrid.RowDefinitions.Count);
                Grid.SetRow(button, instrumentsGrid.RowDefinitions.Count);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(button, 1);
                instrumentsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                instrumentsGrid.Children.Add(label);
                instrumentsGrid.Children.Add(button);
            }
        }

        private async void PlaySample(object sender, RoutedEventArgs args)
        {
            try
            {
                InstrumentAudio capture = (InstrumentAudio) ((Button) sender).Tag;
                int? deviceId = AudioDevices.GetAudioOutputDeviceId(outputDevice.Text);
                if (deviceId == null)
                {
                    return;
                }
                // TODO: Allow audio playback cancellation. Seems unlikely we'll actually need it.
                await AudioDevices.PlayAudio(deviceId.Value, audio.Format, capture.Audio, CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.Log("Error playing sample", e);
            }
        }

        private class InstrumentGroupOrUserSample
        {
            internal InstrumentGroup Group { get; }

            internal InstrumentGroupOrUserSample(InstrumentGroup group) =>
                Group = group;

            public override string ToString() => Group?.Description ?? "User samples";
        }
    }
}

// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NewTek.NDI;
using OpenMacroBoard.SDK;
using StreamDeckSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace NdiStreamDeck
{
    /// <summary>
    /// Interaction logic for the main window - there's not very much to do here.
    /// This could all be data driven with view models, but it didn't seem worth it...
    /// </summary>
    public partial class MainWindow : Window
    {
        private Finder finder;
        private IReadOnlyList<NdiStreamDeckView> views = new List<NdiStreamDeckView>();
        private IReadOnlyList<CameraStatusProvider> statusProviders = new List<CameraStatusProvider>();
        private IReadOnlyList<IMacroBoard> streamDecks;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void InitializeStreamDecksAndFinder(object sender, RoutedEventArgs e)
        {
            // Initialize the Stream Decks and display information about them.
            var devices = StreamDeck.EnumerateDevices().ToList();
            if (!devices.Any())
            {
                MessageBox.Show("Please connect a Stream Deck before starting this application.", "Connect Stream Deck");
                startButton.IsEnabled = false;
                return;
            }
            streamDecks = devices.Select(device => StreamDeck.OpenDevice(device.DevicePath)).ToList();            
            streamDeckInfo.Text = string.Join("\r\n", devices.Zip(streamDecks, (device, deck) =>
                $"{device.DeviceName}: Firmware {deck.GetFirmwareVersion()}\r\n" +
                $"  Keys: {deck.Keys.CountX}x{deck.Keys.CountY}\r\n" +
                $"  Key size: {deck.Keys.KeySize}px\r\n" +
                $"  Key gap: {deck.Keys.GapSize}px"));

            // Start the NDI "finder" which discovers available sources
            finder = new Finder();
            finder.Sources.CollectionChanged += UpdateFinderInfo;
            // Update the list of sources immediately, in case we already had
            // sources before adding the event handler.
            UpdateFinderInfo(this, null);

            // Update the NDI status (resolution and frame rate) twice per second.
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5), IsEnabled = true };
            timer.Tick += UpdateStatus;
        }

        /// <summary>
        /// Update the status display based on the status providers. (This is called from
        /// a dispatcher timer, roughly twice per second.)
        /// </summary>
        private void UpdateStatus(object sender, EventArgs e) =>
            ndiStatus.Text = string.Join("\r\n", statusProviders.Select(v => v.GetStatus()));

        /// <summary>
        /// Starts the Stream Deck views based on the most recently found NDI sources.
        /// </summary>
        private void Start(object sender, RoutedEventArgs e)
        {
            DisposeViewsAndStatusProviders();
            var sources = ndiFinderInfo.Text.Replace("\r\n", "\n").Split("\n")
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => VideoFrameReceiver.Start(Dispatcher, new Source(name)))
                .ToList();
            statusProviders = sources.Select(source => new CameraStatusProvider(source)).ToList();

            views = streamDecks.Select(deck => new NdiStreamDeckView(deck, sources)).ToList();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DisposeViewsAndStatusProviders();
        }

        private void DisposeViewsAndStatusProviders()
        {
            foreach (var disposable in views.Concat<IDisposable>(statusProviders))
            {
                disposable.Dispose();
            }
            views = new List<NdiStreamDeckView>();
            statusProviders = new List<CameraStatusProvider>();
        }

        private void UpdateFinderInfo(object sender, NotifyCollectionChangedEventArgs e) =>
            Dispatcher.Invoke(() =>
            {
                var sources = (finder.Sources ?? Enumerable.Empty<Source>()).OrderBy(source => source.Name);
                ndiFinderInfo.Text = string.Join("\r\n", sources);
            });
    }
}

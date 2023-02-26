// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Schema;
using System.Windows;

namespace DmxLighting.WpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadSchema(object sender, RoutedEventArgs e)
        {
            //var schema = FixtureSchema.FromJsonResource("DmxLighting.SchemaResources.LattaAlvor.86SmallPar.json");
            //var schema = FixtureSchema.FromJsonResource("DmxLighting.SchemaResources.Equinox.FusionOrbit37.json");
            //var schema = FixtureSchema.FromJsonResource("DmxLighting.SchemaResources.UKing.StageDiscoLaser-Simple.json");
            //var schema = FixtureSchema.FromJsonResource("DmxLighting.SchemaResources.UKing.StageDiscoLaser-Full.json");
            var universe = new DmxUniverse(universeNumber: 1, size: 16);
            var acnSender = new StreamingAcnSender("192.168.1.46");
            acnSender.SendUniverse(universe);
            acnSender.WatchUniverse(universe);

            var schema1 = FixtureSchema.FromJsonResource("DmxLighting.SchemaResources.Beamz.BBP94-Full.json");
            var data1 = schema1.ToFixtureData(universe, fixtureFirstChannel: 1);
            Fixture1.DataContext = new FixtureViewModel { Data = data1, Universe = universe, Sender = acnSender };

            var schema2 = FixtureSchema.FromJsonResource("DmxLighting.SchemaResources.Beamz.BBP94-Simple.json");
            var data2 = schema2.ToFixtureData(universe, fixtureFirstChannel: 11);
            Fixture2.DataContext = new FixtureViewModel { Data = data2, Universe = universe, Sender = acnSender };
        }
    }
}

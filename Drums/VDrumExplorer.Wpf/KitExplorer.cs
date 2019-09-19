// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Globalization;
using System.IO;
using System.Windows;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    public class KitExplorer : DataExplorer
    {
        private readonly Kit kit;
        private const string SaveFileFilter = "VDrum Explorer kit files|*.vkit";

        internal KitExplorer(ILogger logger, Kit kit, SysExClient midiClient, string fileName)
            : base(logger, kit.Schema, kit.Data, kit.KitRoot, midiClient, fileName, SaveFileFilter, "Kit explorer")
        {
            this.kit = kit;
            openKitButton.Visibility = Visibility.Collapsed;
            copyToDeviceButton.Content = "Copy kit to device";
            copyToDeviceKitNumber.Text = kit.DefaultKitNumber.ToString(CultureInfo.InvariantCulture);
        }

        protected override void SaveToStream(Stream stream) => kit.Save(stream);

        protected override string FormatNodeDescription(VisualTreeNode node) =>
            (node.KitOnlyDescription ?? node.Description).Format(node.Context, Data);

        protected override async void CopyToDevice(object sender, RoutedEventArgs e)
        {
            if (!KitInputValidation.TryGetKitRoot(copyToDeviceKitNumber.Text, Schema, Logger, out var targetKitRoot))
            {
                return;
            }

            // It's simplest to clone our root node into a new ModuleData at the right place,
            // then send all those segments. It does involve copying the data in memory
            // twice, but that's much quicker than sending it to the kit anyway.
            var clonedData = RootNode.Context.CloneData(Data, targetKitRoot.Context.Address);
            var segments = clonedData.GetSegments();
            await CopySegmentsToDeviceAsync(segments);
        }
    }
}

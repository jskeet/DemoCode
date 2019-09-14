﻿// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    public class ModuleExplorer : DataExplorer
    {
        private readonly Module module;
        private const string SaveFileFilter = "VDrum Explorer module files|*.vdrum";

        internal ModuleExplorer(ILogger logger, Module module, SysExClient midiClient)
            : base(logger, module.Schema, module.Data, module.Schema.LogicalRoot, midiClient, SaveFileFilter)
        {
            this.module = module;
            Title = $"Module explorer: {Schema.Identifier.Name}";
            kitNumberLabel.Visibility = Visibility.Collapsed;
            copyToDeviceKitNumber.Visibility = Visibility.Collapsed;
        }

        protected override void SaveToStream(Stream stream) => module.Save(stream);

        protected override void OpenKitInKitExplorer(object sender, RoutedEventArgs e)
        {
            var node = (VisualTreeNode) detailsPanel.Tag;
            var kitNode = FindKitNode(node);

            // We try to protect against this in terms of enabling/disabling the button, but
            // let's be cautious anyway.
            if (kitNode == null)
            {
                return;
            }

            // We clone the data from kitNode downwards, but relocating it as if it were the first kit.
            var firstKitNode = Schema.KitRoots[1];
            var clonedData = kitNode.Context.CloneData(Data, firstKitNode.Context.Address);
            var kit = new Kit(Schema, clonedData);
            new KitExplorer(Logger, kit, MidiClient).Show();
        }

        private VisualTreeNode FindKitNode(VisualTreeNode currentNode)
        {
            while (currentNode != null)
            {
                if (currentNode.KitNumber != null)
                {
                    return currentNode;
                }
                currentNode = currentNode.Parent;
            }
            return null;
        }

        protected override void LoadDetailsPage()
        {
            base.LoadDetailsPage();
            var node = (VisualTreeNode) detailsPanel.Tag;
            openKitButton.IsEnabled = FindKitNode(node) is object;
        }

        protected override async void CopyToDevice(object sender, RoutedEventArgs e)
        {
            var node = (VisualTreeNode) detailsPanel.Tag;
            if (node == null)
            {
                return;
            }

            // Find all the segments we need.
            // We assume that each item of data is reflected in the details as a container
            // rather than a formatted description either in the details page or in the tree.
            // (Only containers have *editable* data anyway.)
            // We can't just save "all containers under the fixed container" as instruments
            // use several peer containers rather than "everything under one node".
            var segments = node.DescendantNodesAndSelf()
                .SelectMany(GetSegments)
                .Distinct()
                .OrderBy(segment => segment.Start)
                .ToList();
            await CopySegmentsToDeviceAsync(segments);

            IEnumerable<DataSegment> GetSegments(VisualTreeNode treeNode) =>
                treeNode.Details
                    .Where(d => d.Container is object)
                    .Select(d => d.FixContainer(treeNode.Context))
                    .Where(fc => fc.Container.Loadable)
                    .Select(fc => Data.GetSegment(fc.Address));
        }
    }
}

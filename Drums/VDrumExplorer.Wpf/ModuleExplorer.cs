// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    public class ModuleExplorer : DataExplorer
    {
        private readonly Module module;
        private const string SaveFileFilter = "V-Drum Explorer module files|*.vdrum";

        internal ModuleExplorer(ILogger logger, Module module, SysExClient midiClient, string fileName)
            : base(logger, module.Schema, module.Data, module.Schema.LogicalRoot, midiClient, fileName, SaveFileFilter, "Module explorer")
        {
            this.module = module;
            kitNumberLabel.Visibility = Visibility.Collapsed;
            copyToDeviceButton.Content = "Copy data to device";
            copyToDeviceKitNumber.Visibility = Visibility.Collapsed;
            defaultKitPanel.Visibility = Visibility.Collapsed;
            AddKitContextMenus();
            CommandBindings.Add(new CommandBinding(DataExplorerCommands.OpenCopyInKitExplorer, OpenCopyInKitExplorer));
        }

        private void AddKitContextMenus()
        {
            foreach (var item in treeView.Items.Cast<TreeViewItem>())
            {
                AddContextMenusRecursively(item);
            }

            void AddContextMenusRecursively(TreeViewItem node)
            {
                if (!(node.Tag is VisualTreeNode vtn))
                {
                    return;
                }
                if (vtn.KitNumber is null)
                {
                    foreach (var item in node.Items.Cast<TreeViewItem>())
                    {
                        AddContextMenusRecursively(item);
                    }
                }
                else
                {
                    AddKitMenu(node, vtn);
                }
            }

            void AddKitMenu(TreeViewItem node, VisualTreeNode vtn)
            {
                node.ContextMenu = new ContextMenu
                {
                    Items =
                    {
                        new MenuItem { Header = "Open copy in Kit Explorer", Command = DataExplorerCommands.OpenCopyInKitExplorer, CommandParameter = vtn  },
                    }
                };
            }
        }

        protected override void SaveToStream(Stream stream) => module.Save(stream);

        private void OpenCopyInKitExplorer(object sender, ExecutedRoutedEventArgs e)
        {
            var kitNode = (VisualTreeNode) e.Parameter;

            // We clone the data from kitNode downwards, but relocating it as if it were the first kit.
            var firstKitNode = Schema.KitRoots[1];
            var clonedData = kitNode.Context.CloneData(Data, firstKitNode.Context.Address);
            var kit = new Kit(Schema, clonedData, kitNode.KitNumber.Value);
            new KitExplorer(Logger, kit, MidiClient, fileName: null).Show();
        }

        protected override async void CopyToDevice(object sender, RoutedEventArgs e)
        {
            if (CurrentNode == null)
            {
                return;
            }

            // Find all the segments we need.
            // We assume that each item of data is reflected in the details as a container
            // rather than a formatted description either in the details page or in the tree.
            // (Only containers have *editable* data anyway.)
            // We can't just save "all containers under the fixed container" as instruments
            // use several peer containers rather than "everything under one node".
            var segments = CurrentNode.DescendantNodesAndSelf()
                .SelectMany(GetSegments)
                .Distinct()
                .OrderBy(segment => segment.Start)
                .ToList();
            await CopySegmentsToDeviceAsync(segments);

            IEnumerable<DataSegment> GetSegments(VisualTreeNode treeNode) =>
                treeNode.Details
                    .Where(d => d.Container is object)
                    .Select(d => d.Container)
                    .Where(fc => fc.Container.Loadable)
                    .Select(fc => Data.GetSegment(fc.Address));
        }
    }
}

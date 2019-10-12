// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
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
            CommandBindings.Add(new CommandBinding(DataExplorerCommands.ImportKitFromFile, ImportKitFromFile));
            CommandBindings.Add(new CommandBinding(DataExplorerCommands.CopyKit, CopyKit));
            CommandBindings.Add(new CommandBinding(DataExplorerCommands.ExportKit, ExportKit));
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
                        new MenuItem { Header = "Copy to another kit", Command = DataExplorerCommands.CopyKit, CommandParameter = vtn },
                        new MenuItem { Header = "Import from file", Command = DataExplorerCommands.ImportKitFromFile, CommandParameter = node },
                        new MenuItem { Header = "Export to file", Command = DataExplorerCommands.ExportKit, CommandParameter = vtn },
                    }
                };
            }
        }

        protected override void SaveToStream(Stream stream) => module.Save(stream);

        private void OpenCopyInKitExplorer(object sender, ExecutedRoutedEventArgs e)
        {
            var kit = CreateKit((VisualTreeNode) e.Parameter);
            new KitExplorer(Logger, kit, MidiClient, fileName: null).Show();
        }

        private void ExportKit(object sender, ExecutedRoutedEventArgs e)
        {
            var kit = CreateKit((VisualTreeNode) e.Parameter);
            var dialog = new SaveFileDialog { Filter = KitExplorer.SaveFileFilter };
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            using (var stream = File.Create(dialog.FileName))
            {
                kit.Save(stream);
            }
        }

        private Kit CreateKit(VisualTreeNode kitRoot)
        {
            // We clone the data from kitNode downwards, but relocating it as if it were the first kit.
            var firstKitNode = Schema.KitRoots[1];
            var clonedData = kitRoot.Context.CloneData(Data, firstKitNode.Context.Address);
            return new Kit(Schema, clonedData, kitRoot.KitNumber.Value);
        }

        private void ImportKitFromFile(object sender, ExecutedRoutedEventArgs e)
        {
            var treeNode = (TreeViewItem) e.Parameter;
            var targetKitNode = (VisualTreeNode) treeNode.Tag;

            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Filter = "Kit files|*.vkit" };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            object loaded;
            try
            {
                using (var stream = File.OpenRead(dialog.FileName))
                {
                    loaded = SchemaRegistry.ReadStream(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading {dialog.FileName}", ex);
                return;
            }
            if (!(loaded is Kit kit))
            {
                Logger.Log("Loaded file was not a kit");
                return;
            }
            if (!kit.Schema.Identifier.Equals(Schema.Identifier))
            {
                Logger.Log($"Kit was from {kit.Schema.Identifier.Name}; this module is {Schema.Identifier.Name}");
                return;
            }
            var clonedData = kit.KitRoot.Context.CloneData(kit.Data, targetKitNode.Context.Address);
            Data.OverwriteWithDataFrom(clonedData);
            // OverwriteWithDataFrom doesn't doesn't (currently) raise any events.
            // We assume that no other ancestor tree nodes will have text based on the kit details,
            // so just refresh from here downwards.
            RefreshTreeNodeAndDescendants(treeNode);
            LoadDetailsPage();
        }

        private void CopyKit(object sender, ExecutedRoutedEventArgs e)
        {
            var sourceKitNode = (VisualTreeNode) e.Parameter;
            var dialog = new CopyKitTargetDialog(Schema, Data, sourceKitNode);
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            var targetKit = dialog.SelectedKit;
            var clonedData = sourceKitNode.Context.CloneData(Data, targetKit.Context.Address);
            Data.OverwriteWithDataFrom(clonedData);
            // We assume there's a single tree view root node.
            RefreshTreeNodeAndDescendants((TreeViewItem) treeView.Items[0]);
            LoadDetailsPage();
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

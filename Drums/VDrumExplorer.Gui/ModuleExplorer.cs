// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Gui
{
    public partial class ModuleExplorer : Form
    {
        // Non-UI elements
        private readonly ModuleData data;
        private readonly SysExClient midiClient;

        // UI elements we need to be able to refer to
        private readonly TreeView treeView;
        private readonly TableLayoutPanel detailsPanel;
        private readonly ToolStripMenuItem physicalViewMenuItem;
        private readonly ToolStripMenuItem logicalViewMenuItem;

        public ModuleExplorer(ModuleData data, SysExClient midiClient)
        {
            this.data = data;
            this.midiClient = midiClient;
            Width = 600;
            Height = 800;
            Text = $"Module explorer: {data.Schema.Name}";

            var topPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                Controls = { new Button { AutoSize = true, Text = "Edit mode" } },
                Dock = DockStyle.Top,
                Padding = new Padding (5)
            };
            treeView = new TreeView { Dock = DockStyle.Fill };
            treeView.AfterSelect += HandleTreeViewSelection;
            detailsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoScroll = true
            };
            var splitContainer = new SplitContainer
            {
                Panel1 = { Controls = { treeView } },
                Panel2 = { Controls = { detailsPanel } },
                Dock = DockStyle.Fill
            };

            Controls.Add(splitContainer);
            Controls.Add(topPanel);

            logicalViewMenuItem = new ToolStripMenuItem("Logical", null, SetView) { Checked = true };
            physicalViewMenuItem = new ToolStripMenuItem("Physical", null, SetView);

            var menu = new MenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem("File")
                    {
                        DropDownItems =
                        {
                            new ToolStripMenuItem("Save", null, SaveFile)
                        }
                    },
                    new ToolStripMenuItem("View")
                    {                        
                        DropDownItems = { logicalViewMenuItem, physicalViewMenuItem }
                    }
                }
            };
            MainMenuStrip = menu;
            Controls.Add(menu);
            LoadView(data.Schema.LogicalRoot);
        }

        private void SetView(object sender, EventArgs e)
        {
            var selected = (ToolStripMenuItem) sender;
            if (selected.Checked)
            {
                return;
            }
            selected.Checked = true;
            if (selected == physicalViewMenuItem)
            {
                logicalViewMenuItem.Checked = false;
                LoadView(data.Schema.PhysicalRoot);
            }
            else
            {
                physicalViewMenuItem.Checked = false;
                LoadView(data.Schema.LogicalRoot);
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {
            string fileName;
            using (var dialog = new SaveFileDialog())
            {
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                fileName = dialog.FileName;
            }
            using (var stream = File.OpenWrite(fileName))
            {
                data.Save(stream);
            }
        }

        private void LoadView(VisualTreeNode rootVisualNode)
        {
            var root = new TreeNode();
            PopulateNode(root, data, rootVisualNode);
            treeView.Nodes.Clear();
            treeView.Nodes.Add(root);
            LoadReadOnlyDetailsPage(rootVisualNode);
        }

        private static void PopulateNode(TreeNode node, ModuleData data, VisualTreeNode vnode)
        {
            node.Tag = vnode;
            node.Text = vnode.Description.Format(data);
            foreach (var vchild in vnode.Children)
            {
                var childNode = new TreeNode();
                PopulateNode(childNode, data, vchild);
                node.Nodes.Add(childNode);
            }
        }

        private void HandleTreeViewSelection(object sender, TreeViewEventArgs e)
        {
            LoadReadOnlyDetailsPage((VisualTreeNode)e.Node.Tag);
        }

        private void LoadReadOnlyDetailsPage(VisualTreeNode node)
        {
            detailsPanel.Controls.Clear();
            foreach (var detail in node.Details)
            {
                var groupBox = new GroupBox { Text = detail.Description, Dock = DockStyle.Fill, AutoSize = true };
                groupBox.AutoSize = true;
                var nestedFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
                groupBox.Controls.Add(nestedFlow);
                if (detail.Container != null)
                {
                    // TODO: Show disabled fields in physical view?
                    foreach (var primitive in detail.Container.Fields.SelectMany(GetFields).Where(p => p.IsEnabled(data)))
                    {
                        nestedFlow.Controls.Add(new Label { Text = primitive.Description, AutoSize = true });
                        var value = new Label { Text = primitive.GetText(data), AutoSize = true };
                        nestedFlow.Controls.Add(value);
                        nestedFlow.SetFlowBreak(value, true);
                    }
                }
                else
                {
                    foreach (var formatElement in detail.DetailDescriptions)
                    {
                        var value = new Label { Text = formatElement.Format(data), AutoSize = true };
                        nestedFlow.Controls.Add(value);
                        nestedFlow.SetFlowBreak(value, true);
                    }
                }
                detailsPanel.Controls.Add(groupBox);
            }
        }

        private IEnumerable<IPrimitiveField> GetFields(IField field)
        {
            if (field is IPrimitiveField primitive)
            {
                yield return primitive;
            }
            if (field is DynamicOverlay overlay)
            {
                var fields = overlay.Children(data);
                foreach (var primitive2 in fields.OfType<IPrimitiveField>())
                {
                    yield return primitive2;
                }
            }
        }
    }
}

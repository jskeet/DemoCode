// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly Module module;
        private readonly SysExClient midiClient;
        private ViewMode viewMode;

        // UI elements we need to be able to refer to
        private readonly TreeView treeView;
        private readonly TableLayoutPanel detailsPanel;
        private readonly ToolStripMenuItem physicalViewMenuItem;
        private readonly ToolStripMenuItem logicalViewMenuItem;

        public ModuleExplorer(Module module, SysExClient midiClient)
        {
            this.module = module;
            this.midiClient = midiClient;
            Width = 600;
            Height = 800;
            Text = $"Module explorer: {module.Schema.Name}";

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

            logicalViewMenuItem = new ToolStripMenuItem("Logical", null, SetViewFromMenu) { Checked = true, Tag = ViewMode.Logical };
            physicalViewMenuItem = new ToolStripMenuItem("Physical", null, SetViewFromMenu) { Tag = ViewMode.Physical };

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
            LoadView(ViewMode.Logical);
        }

        private void SetViewFromMenu(object sender, EventArgs e)
        {
            var senderMenuItem = (ToolStripMenuItem) sender;
            var targetViewMode = (ViewMode) senderMenuItem.Tag;
            if (targetViewMode == viewMode)
            {
                return;
            }
            LoadView(targetViewMode);
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
                module.Save(stream);
            }
        }

        private void LoadView(ViewMode viewMode)
        {
            this.viewMode = viewMode;
            logicalViewMenuItem.Checked = viewMode == ViewMode.Logical;
            physicalViewMenuItem.Checked = viewMode == ViewMode.Physical;

            var rootModelNode = viewMode == ViewMode.Logical ? module.Schema.LogicalRoot : module.Schema.PhysicalRoot;
            var rootGuiNode = new TreeNode();
            PopulateNode(rootGuiNode, rootModelNode);
            treeView.Nodes.Clear();
            treeView.Nodes.Add(rootGuiNode);
            LoadReadOnlyDetailsPage(rootModelNode);
        }

        private void PopulateNode(TreeNode node, VisualTreeNode vnode)
        {
            node.Tag = vnode;
            node.Text = vnode.Description.Format(module.Data);
            foreach (var vchild in vnode.Children)
            {
                var childNode = new TreeNode();
                PopulateNode(childNode, vchild);
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
                var table = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
                if (detail.Container != null)
                {
                    var fields = detail.Container.Fields
                        .SelectMany(GetPrimtiveFields)
                        .Where(ShouldDisplayField);
                    foreach (var primitive in fields)
                    {
                        table.Controls.Add(new Label { Font = DefaultFont, Text = primitive.Description, AutoSize = true });
                        var value = new Label { Font = DefaultFont, Text = primitive.GetText(module.Data), AutoSize = true };
                        table.Controls.Add(value);
                    }
                }
                else
                {
                    foreach (var formatElement in detail.DetailDescriptions)
                    {
                        var value = new Label { Font = DefaultFont, Text = formatElement.Format(module.Data), AutoSize = true };
                        table.Controls.Add(value);
                        table.SetColumnSpan(value, 2);
                    }
                }
                var groupBox = new GroupBox
                {
                    Font = new Font(DefaultFont, FontStyle.Bold),
                    Text = detail.Description,
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Controls = { table }
                };
                detailsPanel.Controls.Add(groupBox);
            }
        }

        private IEnumerable<IPrimitiveField> GetPrimtiveFields(IField field)
        {
            if (field is IPrimitiveField primitive)
            {
                yield return primitive;
            }
            else if (field is DynamicOverlay overlay)
            {
                var fields = overlay.Children(module.Data);
                foreach (var primitive2 in fields.OfType<IPrimitiveField>())
                {
                    yield return primitive2;
                }
            }
        }

        private bool ShouldDisplayField(IField field)
        {
            // In physical view, we display all fields, for schema debugging.
            if (viewMode == ViewMode.Physical)
            {
                return true;
            }
            // In logical view, conditional fields may or may not be shown.
            return field.IsEnabled(module.Data);
        }
    }
}

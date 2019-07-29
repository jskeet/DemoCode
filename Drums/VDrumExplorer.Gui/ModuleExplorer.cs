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
        private readonly TreeView treeView;
        private readonly TableLayoutPanel detailsPanel;
        private readonly ModuleData data;
        private readonly SysExClient midiClient;

        public ModuleExplorer(ModuleData data, SysExClient midiClient)
        {
            this.data = data;
            this.midiClient = midiClient;
            Width = 600;
            Height = 800;
            Text = $"Module explorer: {data.Schema.Name}";

            var splitContainer = new SplitContainer() { Dock = DockStyle.Fill };
            treeView = new TreeView { Dock = DockStyle.Fill };
            treeView.AfterSelect += HandleTreeViewSelection;
            detailsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoScroll = true
            };
            splitContainer.Panel1.Controls.Add(treeView);
            splitContainer.Panel2.Controls.Add(detailsPanel);

            Controls.Add(splitContainer);

            var root = new TreeNode();
            PopulateNode(root, data, data.Schema.VisualRoot);
            treeView.Nodes.Add(root);

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
                    }
                }
            };
            MainMenuStrip = menu;
            Controls.Add(menu);
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
            var vtn = (VisualTreeNode) e.Node.Tag;
            detailsPanel.Controls.Clear();
            foreach (var detail in vtn.Details)
            {
                var groupBox = new GroupBox { Text = detail.Description ?? "FIXME", Dock = DockStyle.Fill, AutoSize = true };
                groupBox.AutoSize = true;
                var nestedFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
                groupBox.Controls.Add(nestedFlow);
                if (detail.Container != null)
                {
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

        /* Physical mode
        private void HandleTreeViewSelection(object sender, TreeViewEventArgs e)
        {
            var container = (VContainer) e.Node.Tag;
            detailsPanel.Controls.Clear();
            foreach (var primitive in container.Fields.SelectMany(GetFields))
            {
                detailsPanel.Controls.Add(new Label { Text = primitive.Description, AutoSize = true });
                var value = new Label { Text = primitive.GetText(data), AutoSize = true };
                detailsPanel.Controls.Add(value);
                detailsPanel.SetFlowBreak(value, true);
            }
        }


        private static void PopulateNodes(TreeNodeCollection nodes, ModuleData data, VContainer container)
        {
            foreach (var subcontainer in container.Fields.OfType<VContainer>())
            {
                var node = new TreeNode
                {
                    Text = subcontainer.Description,
                    Tag = subcontainer
                };
                nodes.Add(node);
                PopulateNodes(node.Nodes, data, subcontainer);
            }
        }*/

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

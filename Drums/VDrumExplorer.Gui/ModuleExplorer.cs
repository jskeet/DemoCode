using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDrumExplorer.Models;
using VDrumExplorer.Models.Fields;
using VContainer = VDrumExplorer.Models.Fields.Container;

namespace VDrumExplorer.Gui
{
    public partial class ModuleExplorer : Form
    {
        private readonly TreeView treeView;
        private readonly FlowLayoutPanel detailsPanel;
        private readonly ModuleData data;

        public ModuleExplorer(ModuleData data)
        {
            this.data = data;
            Width = 600;
            Height = 800;
            Text = $"Module explorer: {data.ModuleFields.Name}";

            var splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            treeView = new TreeView();
            treeView.AfterSelect += HandleTreeViewSelection;
            treeView.Dock = DockStyle.Fill;
            detailsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill
            };
            splitContainer.Panel1.Controls.Add(treeView);
            splitContainer.Panel2.Controls.Add(detailsPanel);

            Controls.Add(splitContainer);

            PopulateNodes(treeView.Nodes, data, data.ModuleFields.Root);
        }

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
        }
    }
}

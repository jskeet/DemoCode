using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for ModuleExplorer.xaml
    /// </summary>
    public partial class ModuleExplorer : Window
    {
        private readonly Module module;
        private readonly SysExClient midiClient;
        private ViewMode viewMode;
        
        public ModuleExplorer()
        {
            InitializeComponent();
        }

        public ModuleExplorer(Module module, SysExClient midiClient) : this()
        {
            this.module = module;
            this.midiClient = midiClient;
            Title = $"Module explorer: {module.Schema.Name}";
            LoadView(ViewMode.Logical);
        }


        private void SetViewFromMenu(object sender, EventArgs e)
        {
            var senderMenuItem = (MenuItem) sender;
            var targetViewMode = (ViewMode) senderMenuItem.Tag;
            if (targetViewMode == viewMode)
            {
                return;
            }
            LoadView(targetViewMode);
        }

        private void SaveFile(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            using (var stream = File.OpenWrite(dialog.FileName))
            {
                module.Save(stream);
            }
        }

        private void LoadView(ViewMode viewMode)
        {
            this.viewMode = viewMode;
            logicalViewMenuItem.IsChecked = viewMode == ViewMode.Logical;
            physicalViewMenuItem.IsChecked = viewMode == ViewMode.Physical;

            var rootModelNode = viewMode == ViewMode.Logical ? module.Schema.LogicalRoot : module.Schema.PhysicalRoot;
            var rootGuiNode = new TreeViewItem();
            PopulateNode(rootGuiNode, rootModelNode);
            treeView.Items.Clear();
            treeView.Items.Add(rootGuiNode);
            LoadReadOnlyDetailsPage(rootModelNode);
        }

        private void PopulateNode(TreeViewItem node, VisualTreeNode vnode)
        {
            node.Tag = vnode;
            node.Header = vnode.Description.Format(module.Data);
            foreach (var vchild in vnode.Children)
            {
                var childNode = new TreeViewItem();
                PopulateNode(childNode, vchild);
                node.Items.Add(childNode);
            }
        }

        private void HandleTreeViewSelection(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem) e.NewValue;
            if (item is null)
            {
                detailsPanel.Children.Clear();
                return;
            }
            LoadReadOnlyDetailsPage((VisualTreeNode) item.Tag);
        }

        private void LoadReadOnlyDetailsPage(VisualTreeNode node)
        {
            detailsPanel.Children.Clear();
            foreach (var detail in node.Details)
            {
                var grid = new Grid();
                if (detail.Container != null)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    var fields = detail.Container.Fields
                        .SelectMany(GetPrimtiveFields)
                        .Where(ShouldDisplayField);
                    foreach (var primitive in fields)
                    {
                        var label = new Label
                        {
                            Padding = new Thickness(2, 1, 0, 0),
                            Content = primitive.Description
                        };
                        var value = new Label
                        {
                            Padding = new Thickness(5, 1, 0, 0),
                            Content = primitive.GetText(module.Data)
                        };
                        Grid.SetRow(label, grid.RowDefinitions.Count);
                        Grid.SetRow(value, grid.RowDefinitions.Count);
                        Grid.SetColumn(label, 0);
                        Grid.SetColumn(value, 1);
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.Children.Add(label);
                        grid.Children.Add(value);
                    }
                }
                else
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    foreach (var formatElement in detail.DetailDescriptions)
                    {
                        var value = new Label
                        {
                            Padding = new Thickness(2, 1, 0, 0),
                            Content = formatElement.Format(module.Data)
                        };
                        Grid.SetRow(value, grid.RowDefinitions.Count);
                        Grid.SetColumn(value, 0);
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.Children.Add(value);
                    }
                }
                var groupBox = new GroupBox
                {
                    Header = new TextBlock { FontWeight = FontWeights.SemiBold, Text = detail.Description },
                    Content = grid
                };
                detailsPanel.Children.Add(groupBox);
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

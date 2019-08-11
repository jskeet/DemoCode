// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private readonly ILogger logger;
        private readonly Module module;
        private readonly SysExClient midiClient;
        private ViewMode viewMode;
        private bool editMode;
        private ModuleData snapshot;
        private ILookup<ModuleAddress, TreeViewItem> treeViewItemsToUpdateBySegmentStart;
        
        public ModuleExplorer()
        {
            InitializeComponent();
        }

        internal ModuleExplorer(ILogger logger, Module module, SysExClient midiClient) : this()
        {
            this.logger = logger;
            this.module = module;
            this.midiClient = midiClient;
            module.Data.DataChanged += HandleModuleDataChanged;
            copyToDeviceButton.IsEnabled = midiClient != null;
            Title = $"Module explorer: {module.Schema.Name}";
            LoadView(ViewMode.Logical);
        }

        protected override void OnClosed(EventArgs e)
        {
            module.Data.DataChanged -= HandleModuleDataChanged;
            base.OnClosed(e);
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
            var boundItems = new List<(TreeViewItem, ModuleAddress)>();            
            this.viewMode = viewMode;
            logicalViewMenuItem.IsChecked = viewMode == ViewMode.Logical;
            physicalViewMenuItem.IsChecked = viewMode == ViewMode.Physical;

            var rootModelNode = viewMode == ViewMode.Logical ? module.Schema.LogicalRoot : module.Schema.PhysicalRoot;
            var rootGuiNode = CreateNode(rootModelNode);
            treeView.Items.Clear();
            treeView.Items.Add(rootGuiNode);
            detailsPanel.Tag = rootModelNode;
            LoadDetailsPage();
            
            TreeViewItem CreateNode(VisualTreeNode vnode)
            {
                var node = new TreeViewItem
                {                    
                    Header = vnode.Description.Format(module.Data),
                    Tag = vnode
                };
                foreach (var address in vnode.Description.FormatFieldsOrEmpty.Select(field => module.Data.GetSegment(field.Address).Start).Distinct())
                {
                    boundItems.Add((node, address));
                }                
                foreach (var vchild in vnode.Children)
                {
                    var childNode = CreateNode(vchild);
                    node.Items.Add(childNode);
                }
                return node;
            }

            treeViewItemsToUpdateBySegmentStart = boundItems
                .ToLookup(pair => pair.Item2, pair => pair.Item1);
        }

        private void HandleTreeViewSelection(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem) e.NewValue;
            detailsPanel.Tag = (VisualTreeNode) item?.Tag;
            LoadDetailsPage();
        }

        private void LoadDetailsPage()
        {
            var node = (VisualTreeNode) detailsPanel.Tag;
            detailsPanel.Children.Clear();
            if (node == null)
            {
                return;
            }
            foreach (var detail in node.Details)
            {
                var grid = detail.Container == null ? FormatDescriptions(detail) : FormatContainer(detail);
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

        private Grid FormatContainer(VisualTreeDetail detail)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var fields = detail.Container.Fields
                .SelectMany(GetPrimtiveFields)
                .Where(ShouldDisplayField);
            foreach (var primitive in fields)
            {
                var label = new Label
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(2, 1, 0, 0),
                    Content = primitive.Description,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                FrameworkElement value;
                if (editMode)
                {
                    value = CreateReadWriteFieldElement(primitive);
                    value.Margin = new Thickness(5, 1, 0, 0);

                }
                else
                {
                    value = new Label
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(5, 1, 0, 0),
                        Content = primitive.GetText(module.Data)
                    };
                }

                Grid.SetRow(label, grid.RowDefinitions.Count);
                Grid.SetRow(value, grid.RowDefinitions.Count);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(value, 1);
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(label);
                grid.Children.Add(value);
            }
            return grid;
        }

        private FrameworkElement CreateReadWriteFieldElement(IPrimitiveField field) =>
            field switch
            {
                BooleanField bf => CreateBooleanFieldElement(bf),
                EnumField ef => CreateEnumFieldElement(ef),
                StringField sf => CreateStringFieldElement(sf),
                InstrumentField inst => CreateInstrumentFieldElement(inst),
                NumericField num => CreateNumericFieldElement(num),
                _ => new Label { Content = field.GetText(module.Data), Padding = new Thickness(0) }
            };

        private FrameworkElement CreateBooleanFieldElement(BooleanField field)
        {
            var box = new CheckBox { IsChecked = field.GetValue(module.Data), Padding = new Thickness(0) };
            box.Checked += (sender, args) => field.SetValue(module.Data, true);
            box.Unchecked += (sender, args) => field.SetValue(module.Data, false);
            return box;
        }

        private FrameworkElement CreateEnumFieldElement(EnumField field)
        {
            var combo = new ComboBox
            {
                ItemsSource = field.Values,
                SelectedItem = field.GetText(module.Data),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            combo.SelectionChanged += (sender, args) => field.SetValue(module.Data, combo.SelectedIndex);
            return combo;
        }

        private FrameworkElement CreateStringFieldElement(StringField field)
        {
            var textBox = new TextBox { MaxLength = field.Length, Text = field.GetText(module.Data), Padding = new Thickness(0) };
            textBox.TextChanged += (sender, args) =>
                textBox.Foreground = field.TrySetText(module.Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
            return textBox;
        }
        
        private FrameworkElement CreateInstrumentFieldElement(InstrumentField field)
        {
            var allGroups = module.Schema.InstrumentGroups;
            var selected = field.GetInstrument(module.Data);
            var groupChoice = new ComboBox { ItemsSource = module.Schema.InstrumentGroups, SelectedItem = selected.Group };
            var instrumentChoice = new ComboBox { ItemsSource = selected.Group.Instruments, SelectedItem = selected, DisplayMemberPath = "Name", Margin = new Thickness(4, 0, 0, 0) };
            groupChoice.SelectionChanged += (sender, args) =>
            {
                var currentInstrument = (Instrument) instrumentChoice.SelectedItem;
                var newGroup = (InstrumentGroup) groupChoice.SelectedItem;
                if (currentInstrument?.Group != newGroup)
                {
                    instrumentChoice.ItemsSource = ((InstrumentGroup) groupChoice.SelectedItem).Instruments;
                    instrumentChoice.SelectedIndex = 0;
                }
            };
            instrumentChoice.SelectionChanged += (sender, args) =>
            {
                var instrument = (Instrument) instrumentChoice.SelectedItem;
                if (instrument == null)
                {
                    return;
                }
                field.SetInstrument(module.Data, instrument);
            };

            Grid.SetColumn(groupChoice, 0);
            Grid.SetColumn(instrumentChoice, 1);
            Grid grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
                RowDefinitions = { new RowDefinition() },
                Children = { groupChoice, instrumentChoice }
            };
            return grid;
        }

        private static readonly Brush errorBrush = new SolidColorBrush(Colors.Red);
        private FrameworkElement CreateNumericFieldElement(NumericField field)
        {
            var textBox = new TextBox { Text = field.GetText(module.Data), Padding = new Thickness(0) };
            textBox.TextChanged += (sender, args) =>
                textBox.Foreground = field.TrySetText(module.Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
            return textBox;
        }

        private Grid FormatDescriptions(VisualTreeDetail detail)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            foreach (var formatElement in detail.DetailDescriptions)
            {
                var value = new Label
                {
                    Margin = new Thickness(2, 1, 0, 0),
                    Padding = new Thickness(0),
                    Content = formatElement.Format(module.Data)
                };
                Grid.SetRow(value, grid.RowDefinitions.Count);
                Grid.SetColumn(value, 0);
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(value);
            }
            return grid;
        }

        private void EnterEditMode(object sender, RoutedEventArgs e)
        {
            editMode = true;
            Stopwatch sw = Stopwatch.StartNew();
            snapshot = module.Data.Clone();
            sw.Stop();
            logger.Log($"Snapshotting took {(int) sw.ElapsedMilliseconds}ms");
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CommitChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            snapshot = null;
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            module.Data.Reset(snapshot);
            snapshot = null;
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void EnableDisableButtons()
        {
            editModeButton.IsEnabled = !editMode;
            commitChangesButton.IsEnabled = editMode;
            cancelChangesButton.IsEnabled = editMode;
        }

        private async void CopyToDevice(object sender, RoutedEventArgs e)
        {
            var node = (VisualTreeNode) detailsPanel.Tag;
            if (node == null)
            {
                return;
            }

            // Find all the segments we need.
            var segments = new HashSet<DataSegment>();
            foreach (var detail in node.Details)
            {
                if (detail.Container is Container container)
                {
                    if (container.Loadable)
                    {
                        segments.Add(module.Data.GetSegment(container.Address));
                    }
                }
                else
                {
                    var addresses = detail.DetailDescriptions
                        .SelectMany(dd => dd.FormatFieldsOrEmpty)
                        .Select(field => field.Address);
                    foreach (var address in addresses)
                    {
                        segments.Add(module.Data.GetSegment(address));
                    }
                }
            }
            logger.Log($"Writing {segments.Count} segments to the device.");
            foreach (var segment in segments.OrderBy(s => s.Start))
            {
                midiClient.SendData(segment.Start.Value, segment.GetData().ToArray());
                await Task.Delay(100);
            }
            logger.Log($"Finished writing segments to the device.");
        }

        private void HandleModuleDataChanged(object sender, ModuleDataChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action) HandleModuleDataChangedImpl);
            
            void HandleModuleDataChangedImpl()
            {
                var segment = e.ChangedSegment;
                ReflectChangesInTree(segment);
                ReflectChangesInDetails(segment);
            }
            
            void ReflectChangesInTree(DataSegment segment)
            {
                foreach (var treeViewItem in treeViewItemsToUpdateBySegmentStart[segment.Start])
                {
                    var vnode = (VisualTreeNode) treeViewItem.Tag;
                    treeViewItem.Header = vnode.Description.Format(module.Data);
                }
            }

            void ReflectChangesInDetails(DataSegment segment)
            {
            }
        }
    }
}

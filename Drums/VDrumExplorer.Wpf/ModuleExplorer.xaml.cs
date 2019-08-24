// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private ILookup<ModuleAddress, TreeViewItem> treeViewItemsToUpdateBySegmentStart;
        private ILookup<ModuleAddress, GroupBox> detailGroupsToUpdateBySegmentStart;

        public ModuleExplorer()
        {
            InitializeComponent();
        }

        internal ModuleExplorer(ILogger logger, Module module, SysExClient midiClient) : this()
        {
            this.logger = logger;
            this.module = module;
            this.midiClient = midiClient;
            if (midiClient == null)
            {
                mainPanel.Children.Remove(midiPanel);
            }
            module.Data.DataChanged += HandleModuleDataChanged;            
            Title = $"Module explorer: {module.Schema.Identifier.Name}";
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
                var segmentStarts = vnode.Description.FormatFieldsOrEmpty
                    .Select(field => field.Address)
                    .Where(address => address != null)
                    .Select(address => module.Data.GetSegment(address.Value).Start)
                    .Distinct();
                foreach (var address in segmentStarts)
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
            var boundItems = new List<(GroupBox, ModuleAddress)>();
            var node = (VisualTreeNode) detailsPanel.Tag;
            detailsPanel.Children.Clear();
            playNoteButton.IsEnabled = midiClient is object && node?.MidiNoteField?.GetMidiNote(module.Data) is int note;
            if (node == null)
            {
                detailGroupsToUpdateBySegmentStart = boundItems
                    .ToLookup(pair => pair.Item2, pair => pair.Item1);
                return;
            }
            foreach (var detail in node.Details)
            {
                var grid = detail.Container == null ? FormatDescriptions(detail) : FormatContainer(detail);
                var groupBox = new GroupBox
                {
                    Header = new TextBlock { FontWeight = FontWeights.SemiBold, Text = detail.Description },
                    Content = grid,
                    Tag = detail
                };
                detailsPanel.Children.Add(groupBox);
                if (grid.Tag is (DynamicOverlay overlay, Container currentContainer))
                {
                    var segmentStart = module.Data.GetSegment(overlay.SwitchAddress).Start;
                    boundItems.Add((groupBox, segmentStart));
                }
            }
            detailGroupsToUpdateBySegmentStart = boundItems
                .ToLookup(pair => pair.Item2, pair => pair.Item1);
        }

        private IEnumerable<IPrimitiveField> GetPrimitiveFields(IField field)
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
                .SelectMany(GetPrimitiveFields)
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

            // Assumption: at most one dynamic overlay per container
            var overlay = detail.Container.Fields.OfType<DynamicOverlay>().FirstOrDefault();
            if (overlay != null)
            {
                var currentContainer = overlay.GetOverlaidContainer(module.Data);
                grid.Tag = (overlay, currentContainer);
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
            // Instrument fields are really complicated:
            // - They can be preset or user samples ("bank")
            // - For preset instruments, we want to pick group and then instrument
            // - For user samples, we want a simple textbox for the sample number

            const string presetBank = "Preset";
            const string samplesBank = "User sample";
            
            var selected = field.GetInstrument(module.Data);
            var bankChoice = new ComboBox { Items = { presetBank, samplesBank }, SelectedItem = selected.Group != null ? presetBank : samplesBank };
            var groupChoice = new ComboBox { ItemsSource = module.Schema.InstrumentGroups, SelectedItem = selected.Group, Margin = new Thickness(4, 0, 0, 0) };
            var instrumentChoice = new ComboBox { ItemsSource = selected.Group?.Instruments, SelectedItem = selected, DisplayMemberPath = "Name", Margin = new Thickness(4, 0, 0, 0) };
            var userSampleTextBox = new TextBox { Width = 50, Text = selected.Id.ToString(CultureInfo.InvariantCulture), Padding = new Thickness(0), Margin = new Thickness(4, 0, 0, 0), VerticalContentAlignment = VerticalAlignment.Center };
            
            SetVisibility(selected.Group != null);

            userSampleTextBox.SelectionChanged += (sender, args) =>
            {
                bool valid = int.TryParse(userSampleTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int sample)
                    && sample >= 1 && sample <= module.Schema.UserSampleInstruments.Count;
                if (valid)
                {
                    field.SetInstrument(module.Data, module.Schema.UserSampleInstruments[sample - 1]);
                }
                userSampleTextBox.Foreground = valid ? SystemColors.WindowTextBrush : errorBrush;
            };
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
            bankChoice.SelectionChanged += (sender, args) =>
            {
                switch (bankChoice.SelectedIndex)
                {
                    case 0:
                        // Force a change so that we set the instrument
                        groupChoice.SelectedIndex = 1;
                        groupChoice.SelectedIndex = 0;
                        SetVisibility(true);
                        break;
                    case 1:
                        // Make it temporarily invalid so that it's forced to set the data
                        userSampleTextBox.Text = "";
                        userSampleTextBox.Text = "1";
                        SetVisibility(false);
                        break;
                }
            };
            
            Grid.SetColumn(bankChoice, 0);
            Grid.SetColumn(groupChoice, 1);
            Grid.SetColumn(instrumentChoice, 2);
            Grid.SetColumn(userSampleTextBox, 3);
            Grid grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() },
                RowDefinitions = { new RowDefinition() },
                Children = { bankChoice, groupChoice, instrumentChoice, userSampleTextBox }
            };
            return grid;

            void SetVisibility(bool preset)
            {
                userSampleTextBox.Visibility = preset ? Visibility.Collapsed : Visibility.Visible;
                groupChoice.Visibility = preset ? Visibility.Visible : Visibility.Collapsed;
                instrumentChoice.Visibility = preset ? Visibility.Visible : Visibility.Collapsed;
            }
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
            module.Data.Snapshot();
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CommitChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            module.Data.CommitSnapshot();
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            module.Data.RevertSnapshot();
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

            // TODO: We should copy all the segments from the container down.
            
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
                        .Select(field => field.Address)
                        .Where(address => address != null)
                        .Select(address => address.Value);
                    foreach (var address in addresses)
                    {
                        segments.Add(module.Data.GetSegment(address));
                    }
                }
            }
            logger.Log($"Writing {segments.Count} segments to the device.");
            foreach (var segment in segments.OrderBy(s => s.Start))
            {
                midiClient.SendData(segment.Start.Value, segment.CopyData());
                await Task.Delay(100);
            }
            logger.Log($"Finished writing segments to the device.");
        }

        private void PlayNote(object sendar, RoutedEventArgs e)
        {
            var node = detailsPanel.Tag as VisualTreeNode;            
            if (node?.MidiNoteField?.GetMidiNote(module.Data) is int note)
            {
                int attack = (int) attackSlider.Value;
                int channel = int.Parse(midiChannelSelector.Text);
                midiClient.PlayNote(channel, note, attack);
            }            
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
                foreach (var groupBox in detailGroupsToUpdateBySegmentStart[segment.Start])
                {
                    var detail = (VisualTreeDetail) groupBox.Tag;
                    Grid grid = (Grid) groupBox.Content;
                    var (overlay, previousContainer) = ((DynamicOverlay, Container)) grid.Tag;
                    var currentContainer = overlay.GetOverlaidContainer(module.Data);
                    if (currentContainer != previousContainer)
                    {
                        // As the container has changed, let's reset the values to sensible defaults.
                        // This will itself trigger a change notification event, but that's okay.
                        currentContainer.Reset(module.Data);
                        groupBox.Content = FormatContainer(detail);
                    }
                }
            }
        }
    }
}

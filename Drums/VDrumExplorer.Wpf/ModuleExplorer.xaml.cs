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
            var dialog = new SaveFileDialog { Filter = "VDrum Explorer files|*.vdrum" };
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
                    Header = vnode.Description.Format(vnode.Context, module.Data),
                    Tag = vnode
                };
                foreach (var address in vnode.Description.GetSegmentAddresses(vnode.Context))
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
            var context = node.Context;
            detailsPanel.Children.Clear();
            playNoteButton.IsEnabled = GetMidiNote(node) is int note;
            if (node == null)
            {
                detailGroupsToUpdateBySegmentStart = boundItems
                    .ToLookup(pair => pair.Item2, pair => pair.Item1);
                return;
            }
            foreach (var detail in node.Details)
            {
                var grid = detail.Container == null ? FormatDescriptions(context, detail) : FormatContainer(context, detail);
                var groupBox = new GroupBox
                {
                    Header = new TextBlock { FontWeight = FontWeights.SemiBold, Text = detail.Description },
                    Content = grid,
                    Tag = detail
                };
                detailsPanel.Children.Add(groupBox);

                if (grid.Tag is (DynamicOverlay overlay, Container currentContainer))
                {
                    var container = detail.Container.FinalField;
                    var detailContext = detail.Container.GetFinalContext(context).ToChildContext(container);
                    var segmentStart = module.Data.GetSegment(detailContext.Address + overlay.SwitchOffset).Start;
                    boundItems.Add((groupBox, segmentStart));
                }
            }
            detailGroupsToUpdateBySegmentStart = boundItems
                .ToLookup(pair => pair.Item2, pair => pair.Item1);
        }

        private bool ShouldDisplayField(FixedContainer context, IField field)
        {
            // In physical view, we display all fields, for schema debugging.
            if (viewMode == ViewMode.Physical)
            {
                return true;
            }
            // In logical view, conditional fields may or may not be shown.
            return field.IsEnabled(context, module.Data);
        }

        private Grid FormatContainer(FixedContainer context, VisualTreeDetail detail)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Find the real context based on the container.
            var container = detail.Container.FinalField;
            context = detail.Container.GetFinalContext(context).ToChildContext(container);

            var fields = context.GetPrimitiveFields(module.Data)
                .Where(f => ShouldDisplayField(context, f));

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
                    value = CreateReadWriteFieldElement(context, primitive);
                    value.Margin = new Thickness(5, 1, 0, 0);

                }
                else
                {
                    value = new Label
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(5, 1, 0, 0),
                        Content = primitive.GetText(context, module.Data)
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
            var overlay = context.Container.Fields.OfType<DynamicOverlay>().FirstOrDefault();
            if (overlay != null)
            {
                var currentContainer = overlay.GetOverlaidContainer(context, module.Data);
                grid.Tag = (overlay, currentContainer);
            }

            return grid;
        }

        private FrameworkElement CreateReadWriteFieldElement(FixedContainer context, IPrimitiveField field) =>
            field switch
            {
                BooleanField bf => CreateBooleanFieldElement(context, bf),
                EnumField ef => CreateEnumFieldElement(context, ef),
                StringField sf => CreateStringFieldElement(context, sf),
                InstrumentField inst => CreateInstrumentFieldElement(context, inst),
                NumericField num => CreateNumericFieldElement(context, num),
                _ => new Label { Content = field.GetText(context, module.Data), Padding = new Thickness(0) }
            };

        private FrameworkElement CreateBooleanFieldElement(FixedContainer context, BooleanField field)
        {
            var box = new CheckBox { IsChecked = field.GetValue(context, module.Data), Padding = new Thickness(0) };
            box.Checked += (sender, args) => field.SetValue(context, module.Data, true);
            box.Unchecked += (sender, args) => field.SetValue(context, module.Data, false);
            return box;
        }

        private FrameworkElement CreateEnumFieldElement(FixedContainer context, EnumField field)
        {
            var combo = new ComboBox
            {
                ItemsSource = field.Values,
                SelectedItem = field.GetText(context, module.Data),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            combo.SelectionChanged += (sender, args) => field.SetValue(context, module.Data, combo.SelectedIndex);
            return combo;
        }

        private FrameworkElement CreateStringFieldElement(FixedContainer context, StringField field)
        {
            var textBox = new TextBox
            {
                MaxLength = field.Length,
                Text = field.GetText(context, module.Data).TrimEnd(),
                Padding = new Thickness(0)
            };
            textBox.TextChanged += (sender, args) =>
                textBox.Foreground = field.TrySetText(context, module.Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
            return textBox;
        }
        
        private FrameworkElement CreateInstrumentFieldElement(FixedContainer context, InstrumentField field)
        {
            // Instrument fields are really complicated:
            // - They can be preset or user samples ("bank")
            // - For preset instruments, we want to pick group and then instrument
            // - For user samples, we want a simple textbox for the sample number

            const string presetBank = "Preset";
            const string samplesBank = "User sample";
            
            var selected = field.GetInstrument(context, module.Data);
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
                    field.SetInstrument(context, module.Data, module.Schema.UserSampleInstruments[sample - 1]);
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
                field.SetInstrument(context, module.Data, instrument);
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
        private FrameworkElement CreateNumericFieldElement(FixedContainer context, NumericField field)
        {
            var textBox = new TextBox { Text = field.GetText(context, module.Data), Padding = new Thickness(0) };
            textBox.TextChanged += (sender, args) =>
                textBox.Foreground = field.TrySetText(context, module.Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
            return textBox;
        }

        private Grid FormatDescriptions(FixedContainer context, VisualTreeDetail detail)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            foreach (var formatElement in detail.DetailDescriptions)
            {
                var value = new Label
                {
                    Margin = new Thickness(2, 1, 0, 0),
                    Padding = new Thickness(0),
                    Content = formatElement.Format(context, module.Data)
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

            // Find all the segments we need.
            // We assume that each item of data is reflected in the details as a container
            // rather than a formatted description either in the details page or in the tree.
            // (Only containers have *editable* data anyway.)
            var segments = node.DescendantNodesAndSelf()
                .SelectMany(GetSegments)
                .Distinct()
                .OrderBy(segment => segment.Start)
                .ToList();
            logger.Log($"Writing {segments.Count} segments to the device.");
            foreach (var segment in segments)
            {
                midiClient.SendData(segment.Start.Value, segment.CopyData());
                await Task.Delay(100);
            }
            logger.Log($"Finished writing segments to the device.");

            IEnumerable<DataSegment> GetSegments(VisualTreeNode treeNode) =>
                treeNode.Details
                    .Select(d => d.Container)
                    .Where(chain => chain is FieldChain<Container>)
                    .Select(chain => chain.GetFinalContext(treeNode.Context).ToChildContext(chain.FinalField))
                    .Where(fc => fc.Container.Loadable)
                    .Select(fc => module.Data.GetSegment(fc.Address));
        }

        private void PlayNote(object sender, RoutedEventArgs e)
        {
            var node = detailsPanel.Tag as VisualTreeNode;
            if (GetMidiNote(node) is int note)
            {
                int attack = (int) attackSlider.Value;
                int channel = int.Parse(midiChannelSelector.Text);
                midiClient.PlayNote(channel, note, attack);
            }            
        }

        private int? GetMidiNote(VisualTreeNode node)
        {
            if (midiClient is null || node?.MidiNoteField is null)
            {
                return null;
            }
            var finalContext = node.MidiNoteField.GetFinalContext(node.Context);
            var field = node.MidiNoteField.FinalField;
            return field.GetMidiNote(finalContext, module.Data);
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
                    treeViewItem.Header = vnode.Description.Format(vnode.Context, module.Data);
                }
            }

            void ReflectChangesInDetails(DataSegment segment)
            {
                var node = (VisualTreeNode) detailsPanel.Tag;
                var context = node.Context;
                foreach (var groupBox in detailGroupsToUpdateBySegmentStart[segment.Start])
                {
                    var detail = (VisualTreeDetail) groupBox.Tag;

                    var container = detail.Container.FinalField;
                    var detailContext = detail.Container.GetFinalContext(context).ToChildContext(container);
                    Grid grid = (Grid) groupBox.Content;
                    var (overlay, previousContainer) = ((DynamicOverlay, Container)) grid.Tag;
                    var currentContainer = overlay.GetOverlaidContainer(detailContext, module.Data);
                    if (currentContainer != previousContainer)
                    {
                        // As the container has changed, let's reset the values to sensible defaults.
                        // This will itself trigger a change notification event, but that's okay.
                        currentContainer.Reset(detailContext, module.Data);
                        groupBox.Content = FormatContainer(context, detail);
                    }
                }
            }
        }
    }
}

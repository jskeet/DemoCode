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
    /// Interaction logic for DataExplorer.xaml
    /// </summary>
    public partial class DataExplorer : Window
    {
        protected ModuleData Data { get; }
        protected ModuleSchema Schema { get; }
        internal ILogger Logger { get; }
        protected SysExClient MidiClient { get; }
        protected VisualTreeNode RootNode { get; }
        private readonly string saveFileFilter;

        private bool editMode;
        private ILookup<ModuleAddress, TreeViewItem> treeViewItemsToUpdateBySegmentStart;
        private ILookup<ModuleAddress, GroupBox> detailGroupsToUpdateBySegmentStart;

        public DataExplorer()
        {
            InitializeComponent();
        }

        internal DataExplorer(ILogger logger, ModuleSchema schema, ModuleData data, VisualTreeNode rootNode, SysExClient midiClient,
            string saveFileFilter) : this()
        {
            Logger = logger;
            Schema = schema;
            Data = data;
            MidiClient = midiClient;
            RootNode = rootNode;
            this.saveFileFilter = saveFileFilter;
            if (midiClient == null)
            {
                mainPanel.Children.Remove(midiPanel);
            }
            Data.DataChanged += HandleModuleDataChanged;
            LoadView();
        }

        protected override void OnClosed(EventArgs e)
        {
            Data.DataChanged -= HandleModuleDataChanged;
            base.OnClosed(e);
        }

        protected void SaveFile(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = saveFileFilter };
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            using (var stream = File.OpenWrite(dialog.FileName))
            {
                SaveToStream(stream);
            }
        }

        protected virtual void SaveToStream(Stream stream)
        {
        }

        protected virtual void CopyToDevice(object sender, RoutedEventArgs e)
        {
        }

        private void LoadView()
        {
            var boundItems = new List<(TreeViewItem, ModuleAddress)>();

            var rootGuiNode = CreateNode(RootNode);
            treeView.Items.Clear();
            treeView.Items.Add(rootGuiNode);
            detailsPanel.Tag = RootNode;
            LoadDetailsPage();

            TreeViewItem CreateNode(VisualTreeNode vnode)
            {
                var node = new TreeViewItem
                {
                    Header = vnode.Description.Format(vnode.Context, Data),
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

        protected virtual void OpenKitInKitExplorer(object sender, RoutedEventArgs e)
        {
        }

        private VisualTreeNode FindKitNode(VisualTreeNode currentNode)
        {
            while (currentNode != null)
            {
                if (currentNode.KitNumber != null)
                {
                    return currentNode;
                }
                currentNode = currentNode.Parent;
            }
            return null;
        }

        protected virtual void LoadDetailsPage()
        {
            var boundItems = new List<(GroupBox, ModuleAddress)>();
            var node = (VisualTreeNode) detailsPanel.Tag;
            detailsPanel.Children.Clear();
            playNoteButton.IsEnabled = GetMidiNote(node) is int note;
            if (node == null)
            {
                detailGroupsToUpdateBySegmentStart = boundItems
                    .ToLookup(pair => pair.Item2, pair => pair.Item1);
                return;
            }
            var context = node.Context;
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
                    var detailContext = detail.FixContainer(context);
                    var segmentStart = Data.GetSegment(detailContext.Address + overlay.SwitchOffset).Start;
                    boundItems.Add((groupBox, segmentStart));
                }
            }
            detailGroupsToUpdateBySegmentStart = boundItems
                .ToLookup(pair => pair.Item2, pair => pair.Item1);
        }

        private Grid FormatContainer(FixedContainer context, VisualTreeDetail detail)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Find the real context based on the container.
            var container = detail.Container.FinalField;
            context = detail.FixContainer(context);

            var fields = context.GetPrimitiveFields(Data)
                .Where(f => f.IsEnabled(context, Data));

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
                        Content = primitive.GetText(context, Data)
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
                var currentContainer = overlay.GetOverlaidContainer(context, Data);
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
                _ => new Label { Content = field.GetText(context, Data), Padding = new Thickness(0) }
            };

        private FrameworkElement CreateBooleanFieldElement(FixedContainer context, BooleanField field)
        {
            var box = new CheckBox { IsChecked = field.GetValue(context, Data), Padding = new Thickness(0) };
            box.Checked += (sender, args) => field.SetValue(context, Data, true);
            box.Unchecked += (sender, args) => field.SetValue(context, Data, false);
            return box;
        }

        private FrameworkElement CreateEnumFieldElement(FixedContainer context, EnumField field)
        {
            var combo = new ComboBox
            {
                ItemsSource = field.Values,
                SelectedItem = field.GetText(context, Data),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            combo.SelectionChanged += (sender, args) => field.SetValue(context, Data, combo.SelectedIndex);
            return combo;
        }

        private FrameworkElement CreateStringFieldElement(FixedContainer context, StringField field)
        {
            var textBox = new TextBox
            {
                MaxLength = field.Length,
                Text = field.GetText(context, Data).TrimEnd(),
                Padding = new Thickness(0)
            };
            textBox.TextChanged += (sender, args) =>
                textBox.Foreground = field.TrySetText(context, Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
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

            var selected = field.GetInstrument(context, Data);
            var bankChoice = new ComboBox { Items = { presetBank, samplesBank }, SelectedItem = selected.Group != null ? presetBank : samplesBank };
            var groupChoice = new ComboBox { ItemsSource = Schema.InstrumentGroups, SelectedItem = selected.Group, Margin = new Thickness(4, 0, 0, 0) };
            var instrumentChoice = new ComboBox { ItemsSource = selected.Group?.Instruments, SelectedItem = selected, DisplayMemberPath = "Name", Margin = new Thickness(4, 0, 0, 0) };
            var userSampleTextBox = new TextBox { Width = 50, Text = selected.Id.ToString(CultureInfo.InvariantCulture), Padding = new Thickness(0), Margin = new Thickness(4, 0, 0, 0), VerticalContentAlignment = VerticalAlignment.Center };

            SetVisibility(selected.Group != null);

            userSampleTextBox.SelectionChanged += (sender, args) =>
            {
                bool valid = int.TryParse(userSampleTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int sample)
                    && sample >= 1 && sample <= Schema.UserSampleInstruments.Count;
                if (valid)
                {
                    field.SetInstrument(context, Data, Schema.UserSampleInstruments[sample - 1]);
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
                field.SetInstrument(context, Data, instrument);
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
            var textBox = new TextBox { Text = field.GetText(context, Data), Padding = new Thickness(0) };
            textBox.TextChanged += (sender, args) =>
                textBox.Foreground = field.TrySetText(context, Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
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
                    Content = formatElement.Format(context, Data)
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
            Data.Snapshot();
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CommitChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            Data.CommitSnapshot();
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            Data.RevertSnapshot();
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void EnableDisableButtons()
        {
            editModeButton.IsEnabled = !editMode;
            commitChangesButton.IsEnabled = editMode;
            cancelChangesButton.IsEnabled = editMode;
        }

        private void PlayNote(object sender, RoutedEventArgs e)
        {
            var node = detailsPanel.Tag as VisualTreeNode;
            if (GetMidiNote(node) is int note)
            {
                int attack = (int) attackSlider.Value;
                int channel = int.Parse(midiChannelSelector.Text);
                MidiClient.PlayNote(channel, note, attack);
            }
        }

        private int? GetMidiNote(VisualTreeNode node)
        {
            if (MidiClient is null || node?.MidiNoteField is null)
            {
                return null;
            }
            var finalContext = node.MidiNoteField.GetFinalContext(node.Context);
            var field = node.MidiNoteField.FinalField;
            return field.GetMidiNote(finalContext, Data);
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
                    treeViewItem.Header = vnode.Description.Format(vnode.Context, Data);
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
                    var detailContext = detail.FixContainer(context);
                    Grid grid = (Grid) groupBox.Content;
                    var (overlay, previousContainer) = ((DynamicOverlay, Container)) grid.Tag;
                    var currentContainer = overlay.GetOverlaidContainer(detailContext, Data);
                    if (currentContainer != previousContainer)
                    {
                        // As the container has changed, let's reset the values to sensible defaults.
                        // This will itself trigger a change notification event, but that's okay.
                        currentContainer.Reset(detailContext, Data);
                        groupBox.Content = FormatContainer(context, detail);
                    }
                }
            }
        }

        protected async Task CopySegmentsToDeviceAsync(List<DataSegment> segments)
        {
            midiPanel.IsEnabled = false;
            try
            {
                Logger.Log($"Writing {segments.Count} segments to the device.");
                foreach (var segment in segments)
                {
                    MidiClient.SendData(segment.Start.Value, segment.CopyData());
                    await Task.Delay(40);
                }
                Logger.Log($"Finished writing segments to the device.");
            }
            finally
            {
                midiPanel.IsEnabled = true;
            }
        }
    }
}

// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        // Not const just to avoid unreadable code warnings.
        private static readonly bool UseSliders = true;

        protected ModuleData Data { get; }
        protected ModuleSchema Schema { get; }
        internal ILogger Logger { get; }
        protected RolandMidiClient MidiClient { get; }
        protected VisualTreeNode RootNode { get; }
        private readonly string explorerName;
        private readonly string saveFileFilter;

        private bool editMode;
        private string fileName;
        private ILookup<ModuleAddress, TreeViewItem> treeViewItemsToUpdateBySegmentStart;
        private ILookup<ModuleAddress, GroupBox> detailGroupsToUpdateBySegmentStart;

        // When we enter edit mode, we create a dictionary to remember transient
        // overlay data. When we switch between overload containers, we reset to
        // the previous values if we have any, instead of resetting to defaults.
        private Dictionary<(ModuleAddress, Container), byte[]> savedOverlayEditData;

        protected VisualTreeNode CurrentNode { get; set; }

        public DataExplorer()
        {
            InitializeComponent();
            // We can't attach these event handlers in XAML, as only instance members of the current class are allowed.
            copyToDeviceKitNumber.PreviewTextInput += TextConversions.CheckDigits;
            defaultKitNumber.PreviewTextInput += TextConversions.CheckDigits;
        }

        internal DataExplorer(ILogger logger, ModuleSchema schema, ModuleData data, VisualTreeNode rootNode, RolandMidiClient midiClient, string fileName,
            string saveFileFilter, string explorerName) : this()
        {
            Logger = logger;
            Schema = schema;
            Data = data;
            MidiClient = midiClient;
            RootNode = rootNode;
            this.saveFileFilter = saveFileFilter;
            this.explorerName = explorerName;
            this.fileName = fileName;
            if (midiClient == null)
            {
                mainPanel.Children.Remove(midiPanel);
            }
            Data.DataChanged += HandleModuleDataChanged;
            LoadView();
            UpdateTitle();
        }

        private void UpdateTitle() =>
            Title = fileName == null
            ? $"{explorerName} ({Schema.Identifier.Name})"
            : $"{explorerName} ({Schema.Identifier.Name}) - {fileName}";

        protected override void OnClosed(EventArgs e)
        {
            Data.DataChanged -= HandleModuleDataChanged;
            base.OnClosed(e);
        }

        protected void SaveFile(object sender, ExecutedRoutedEventArgs e) =>
            SaveFile(fileName);

        protected void SaveFileAs(object sender, ExecutedRoutedEventArgs e) =>
            SaveFile(defaultFileName: null);

        private void SaveFile(string defaultFileName)
        {
            string newFileName = defaultFileName;
            if (newFileName == null)
            {
                var dialog = new SaveFileDialog { Filter = saveFileFilter };
                var result = dialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                newFileName = dialog.FileName;
            }
            using (var stream = File.OpenWrite(newFileName))
            {
                SaveToStream(stream);
            }
            fileName = newFileName;
            UpdateTitle();
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
            CurrentNode = RootNode;
            LoadDetailsPage();

            TreeViewItem CreateNode(VisualTreeNode vnode)
            {
                var node = new TreeViewItem
                {
                    Header = FormatNodeDescription(vnode),
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

        protected void RefreshTreeNodeAndDescendants(TreeViewItem node)
        {
            RefreshTreeNodeText(node);
            foreach (TreeViewItem child in node.Items)
            {
                RefreshTreeNodeAndDescendants(child);
            }
        }

        protected void RefreshTreeNodeText(TreeViewItem node)
        {
            var vnode = (VisualTreeNode) node.Tag;
            node.Header = FormatNodeDescription(vnode);
        }

        protected virtual string FormatNodeDescription(VisualTreeNode node) =>
            node.Description.Format(node.Context, Data);

        private void HandleTreeViewSelection(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem) e.NewValue;
            CurrentNode = (VisualTreeNode) item?.Tag;
            LoadDetailsPage();
        }

        protected virtual void LoadDetailsPage()
        {
            var boundItems = new List<(GroupBox, ModuleAddress)>();
            detailsPanel.Children.Clear();
            playNoteButton.IsEnabled = GetMidiNote(CurrentNode) is int note;
            if (CurrentNode == null)
            {
                detailGroupsToUpdateBySegmentStart = boundItems
                    .ToLookup(pair => pair.Item2, pair => pair.Item1);
                return;
            }
            var context = CurrentNode.Context;
            foreach (var detail in CurrentNode.Details)
            {
                var grid = detail.Container == null ? FormatDescriptions(context, detail) : FormatContainer(detail.Container);
                var groupBox = new GroupBox
                {
                    Header = new TextBlock { FontWeight = FontWeights.SemiBold, Text = detail.Description },
                    Content = grid,
                    Tag = detail
                };
                detailsPanel.Children.Add(groupBox);

                if (grid.Tag is (DynamicOverlay overlay, Container currentContainer))
                {
                    var segmentStart = Data.GetSegment(detail.Container.Address + overlay.SwitchContainerOffset).Start;
                    boundItems.Add((groupBox, segmentStart));
                }
            }
            detailGroupsToUpdateBySegmentStart = boundItems
                .ToLookup(pair => pair.Item2, pair => pair.Item1);
        }

        private Grid FormatContainer(FixedContainer context)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

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

            // Instrument overlays are particularly complicated:
            // - For most fields, the field doesn't reset unless the instrument group resets (like a normal overlay)
            // - For cymbal/hi-hat fields, the size resets when the instrument is changed, because the size is part of the instrument.

            const string presetBank = "Preset";
            const string samplesBank = "User sample";

            var selected = field.GetInstrument(context, Data);
            var bankChoice = new ComboBox { Items = { presetBank, samplesBank }, SelectedItem = selected.Group != null ? presetBank : samplesBank };
            var groupChoice = new ComboBox { ItemsSource = Schema.InstrumentGroups, SelectedItem = selected.Group, Margin = new Thickness(4, 0, 0, 0) };
            var instrumentChoice = new ComboBox { ItemsSource = selected.Group?.Instruments, SelectedItem = selected, DisplayMemberPath = "Name", Margin = new Thickness(4, 0, 0, 0) };
            var userSampleTextBox = new TextBox { Width = 50, Text = TextConversions.Format(selected.Id), Padding = new Thickness(0), Margin = new Thickness(4, 0, 0, 0), VerticalContentAlignment = VerticalAlignment.Center };

            SetVisibility(selected.Group != null);

            userSampleTextBox.SelectionChanged += (sender, args) =>
            {
                bool valid = TextConversions.TryParseInt32(userSampleTextBox.Text, out int sample)
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
                // Note: this updates the vedit fields if necessary too. (Size for cymbals etc.)
                field.SetInstrument(context, Data, instrument);
                // Update the vedit if necessary.
                // FIXME: This is really hacky, but working out exactly what should trigger when is harder.
                // We don't really need to recreate the fields - we'd just like to modify the content. But
                // that's tricky too.
                UpdateVeditGroupBox();
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

            void UpdateVeditGroupBox()
            {
                var veditAddress = context.Address + field.VeditOffset;
                foreach (var groupBox in detailsPanel.Children.OfType<GroupBox>())
                {
                    if (groupBox.Tag is VisualTreeDetail detail && detail.Container?.Address.Value == veditAddress.Value)
                    {
                        groupBox.Content = FormatContainer(detail.Container);
                    }
                }
            }
        }

        private static readonly Brush errorBrush = new SolidColorBrush(Colors.Red);
        private FrameworkElement CreateNumericFieldElement(FixedContainer context, NumericField field)
        {
            if (UseSliders)
            {
                var slider = new Slider
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(2, 1, 0, 0),
                    Minimum = field.Min,
                    Maximum = field.Max,
                    Value = field.GetRawValue(context, Data),
                    SmallChange = 1,
                    LargeChange = Math.Max((field.Max - field.Min) / 10, 1),
                    Width = 150                    
                };
                
                var label = new Label
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(2, 1, 0, 0),
                    Content = field.GetText(context, Data),
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                slider.ValueChanged += (sender, args) =>
                {
                    field.SetRawValue(context, Data, (int) args.NewValue);
                    label.Content = field.GetText(context, Data);
                };

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { slider, label }
                };
            }
            else
            {
                var textBox = new TextBox { Text = field.GetText(context, Data), Padding = new Thickness(0) };
                textBox.TextChanged += (sender, args) =>
                    textBox.Foreground = field.TrySetText(context, Data, textBox.Text) ? SystemColors.WindowTextBrush : errorBrush;
                return textBox;
            }
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
            savedOverlayEditData = new Dictionary<(ModuleAddress, Container), byte[]>();
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CommitChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            Data.CommitSnapshot();
            savedOverlayEditData = null;
            EnableDisableButtons();
            LoadDetailsPage();
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            editMode = false;
            Data.RevertSnapshot();
            savedOverlayEditData = null;
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
            if (GetMidiNote(CurrentNode) is int note)
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
            // Note: this is *not* thread-safe. We assume that only the UI is making changes
            // to the data it's responsible for. This allows us to handle data changes synchronously,
            // which greatly simplifies ordering aspects when changing an instrument group -
            // we first need to reset the vedit overlay, then set the size etc.
            var segment = e.ChangedSegment;
            ReflectChangesInTree(segment);
            ReflectChangesInDetails(segment);

            void ReflectChangesInTree(DataSegment segment)
            {
                foreach (var treeViewItem in treeViewItemsToUpdateBySegmentStart[segment.Start])
                {
                    RefreshTreeNodeText(treeViewItem);
                }
            }

            void ReflectChangesInDetails(DataSegment segment)
            {
                foreach (var groupBox in detailGroupsToUpdateBySegmentStart[segment.Start])
                {
                    var detail = (VisualTreeDetail) groupBox.Tag;

                    var detailContext = detail.Container;
                    Grid grid = (Grid) groupBox.Content;
                    var (overlay, previousContainer) = ((DynamicOverlay, Container)) grid.Tag;
                    var currentContainer = overlay.GetOverlaidContainer(detailContext, Data);
                    if (currentContainer != previousContainer)
                    {
                        var overlayAddress = detailContext.Address + overlay.Offset;
                        // Save the current container data in case we need to come back to it.
                        savedOverlayEditData[(overlayAddress, previousContainer)] = Data.GetData(overlayAddress, overlay.Size);
                        // As the container has changed, let's either reset the values to sensible defaults,
                        // or to previous values if we have any.
                        // This will itself trigger a change notification event, but that's okay.
                        if (savedOverlayEditData.TryGetValue((overlayAddress, currentContainer), out var data))
                        {
                            Data.GetSegment(overlayAddress).SetData(overlayAddress, data);
                        }
                        else
                        {
                            currentContainer.Reset(detailContext, Data);
                        }
                        groupBox.Content = FormatContainer(detailContext);
                    }
                }
            }
        }

        protected async Task CopySegmentsToDeviceAsync(List<DataSegment> segments)
        {
            midiPanel.IsEnabled = false;
            int written = 0;
            try
            {
                Logger.Log($"Writing {segments.Count} segments to the device.");
                foreach (var segment in segments)
                {
                    MidiClient.SendData(segment.Start.Value, segment.CopyData());
                    await Task.Delay(40);
                    written++;
                }
                Logger.Log($"Finished writing segments to the device.");
            }
            catch (Exception e)
            {
                Logger.Log("Failed while writing data to the device.");
                Logger.Log($"Segments successfully written: {written}");
                Logger.Log($"Error: {e}");
            }
            finally
            {
                midiPanel.IsEnabled = true;
            }
        }
    }
}

// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Data
{
    public class DataTreeNodeViewModel : ViewModelBase<DataTreeNode>
    {
        private readonly DataFieldFormattableString formattableString;

        // Note: exposing the root like this is somewhat ugly, but it means we can have "4 command instances which take a parameter"
        // rather than "4 command instances per tree view node". It's relatively tricky to get back to the root datacontext
        // within the XAML for a menu item.
        public DataExplorerViewModel Root { get; }

        public new DataTreeNode Model => base.Model;

        private readonly Lazy<IReadOnlyList<DataTreeNodeViewModel>> children;

        public string Title
        {
            get
            {
                if (KitNumber is int kit)
                {
                    string name = Kit.GetKitName(Model);
                    return Root.IsKitExplorer ? name : $"Kit {kit}: {name}";
                }
                else
                {
                    return formattableString.Text;
                }
            }
        }

        public IReadOnlyList<DataTreeNodeViewModel> Children => children.Value;

        public int? KitNumber => Model.SchemaNode.KitNumber;
        public bool KitContextCommandsEnabled => Root.IsModuleExplorer && KitNumber.HasValue;

        public string? MidiNotePath => Model.SchemaNode.MidiNotePath;

        public int? GetMidiNote() => Model.GetMidiNote();

        public DataTreeNodeViewModel(DataTreeNode model, DataExplorerViewModel root) : base(model)
        {
            Root = root;
            formattableString = model.Format;
            children = Lazy.Create(() => model.Children.ToReadOnlyList(child => new DataTreeNodeViewModel(child, root)));
        }

        protected override void OnPropertyChangedHasSubscribers()
        {
            base.OnPropertyChangedHasSubscribers();
            formattableString.PropertyChanged += RaiseTitleChanged;
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            base.OnPropertyChangedHasNoSubscribers();
            formattableString.PropertyChanged -= RaiseTitleChanged;
        }

        public void RaiseTitleChanged(object sender, PropertyChangedEventArgs e) =>
            RaisePropertyChanged(nameof(Title));

        internal IReadOnlyList<IDataNodeDetailViewModel> CreateDetails() => Model.Details.ToReadOnlyList(CreateDetail);

        private IDataNodeDetailViewModel CreateDetail(IDataNodeDetail detail) =>
            detail switch
            {
                ListDataNodeDetail model => new ListDataNodeDetailViewModel(model),
                FieldContainerDataNodeDetail model => new FieldContainerDataNodeDetailViewModel(model, Root),
                _ => throw new ArgumentException($"Unknown detail type: {detail?.GetType()}")
            };
    }
}

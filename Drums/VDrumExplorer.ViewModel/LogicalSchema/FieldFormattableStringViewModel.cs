// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.LogicalSchema
{
    public class FieldFormattableStringViewModel : ViewModelBase<FieldFormattableString>
    {
        private readonly Lazy<IReadOnlyList<KeyValueViewModel>> table;

        public FieldFormattableStringViewModel(FieldFormattableString model) : base(model)
        {
            Lazy.Initialize(out table, () => GenerateTable().ToList().AsReadOnly());
        }

        private IEnumerable<KeyValueViewModel> GenerateTable()
        {
            yield return new KeyValueViewModel("Format string", Model.FormatString);
            for (int i = 0; i < Model.FieldPaths.Count; i++)
            {
                yield return new KeyValueViewModel($"Field path {i}", Model.FieldPaths[i]);
            }
        }

        public string FormatString => Model.FormatString;
        public IReadOnlyList<KeyValueViewModel> Table => table.Value;
    }
}

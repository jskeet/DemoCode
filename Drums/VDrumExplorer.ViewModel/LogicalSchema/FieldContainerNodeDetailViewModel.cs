// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.LogicalSchema
{
    public sealed class FieldContainerNodeDetailViewModel : NodeDetailViewModel<FieldContainerNodeDetail>
    {
        private readonly Lazy<IReadOnlyList<KeyValueViewModel>> fields;

        public FieldContainerNodeDetailViewModel(FieldContainerNodeDetail model) : base(model)
        {
            Lazy.Initialize(out fields, () => GenerateFields().ToList().AsReadOnly());
        }

        private IEnumerable<KeyValueViewModel> GenerateFields()
        {
            foreach (var field in Model.Container.Fields)
            {
                // TODO: A FieldViewModel?
                string value = field switch
                {
                    InstrumentField _ => "Instrument",
                    OverlayField f => $"Overlay; {f.FieldLists.Count} lists; {f.NestedFieldCount} nested fields",
                    BooleanField _ => "Boolean",
                    EnumField f => $"Enum ({f.Values.Count} values)",
                    NumericField f => $"Numeric: {f.Min}-{f.Max}",
                    StringField f => $"String ({f.Length})",
                    TempoField f => "Tempo",
                    _ => throw new InvalidOperationException($"Unexpected field type: {field?.GetType()}")
                };
                string? valueToolTip = field switch
                {
                    EnumField f => string.Join("\r\n", f.Values.Select(value => $"{f.RawNumberByName[value]}: {value}")),
                    _ => null
                };
                yield return new KeyValueViewModel($"{field.Offset}: {field.Description}", value, keyToolTip: null, valueToolTip);
            }
        }

        public IReadOnlyList<KeyValueViewModel> Fields => fields.Value;
    }
}

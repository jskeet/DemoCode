﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public class EnumDataField : NumericDataFieldBase<EnumField>
    {
        private IReadOnlyList<string> Values => SchemaField.Values;

        internal EnumDataField(EnumField field) : base(field)
        {
        }

        protected override void RaisePropertyChanges()
        {
            base.RaisePropertyChanges();
            RaisePropertyChanged(nameof(Value));
        }

        public string Value
        {
            get => Values[RawValue - Min];
            set
            {
                if (!TrySetValue(value))
                {
                    throw new ArgumentException(nameof(value));
                }
            }
        }

        private bool TrySetValue(string value) =>
            SchemaField.RawNumberByName.TryGetValue(value, out int number) && TrySetRawValue(number);

        public override string FormattedText => Value;
    }
}

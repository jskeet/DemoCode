// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Diagnostics.CodeAnalysis;

namespace VDrumExplorer.Data.Fields
{
    public abstract class PrimitiveFieldBase : FieldBase, IPrimitiveField
    {
        public abstract string GetText(FixedContainer context, ModuleData data);
        public abstract void Reset(FixedContainer context, ModuleData data);
        public abstract bool TrySetText(FixedContainer context, ModuleData data, string text);

        private protected PrimitiveFieldBase(Parameters p) : base(p)
        {
        }
        
        public bool Validate(FixedContainer context, ModuleData data, out string? error)
        {
            var segment = data.GetSegment(GetAddress(context));
            // TODO: Check the length against size?
            if (segment == null)
            {
                error = "No segment containing field present in the module data.";
                return false;
            }
            return ValidateData(context, data, out error);
        }

        /// <summary>
        /// Validates the data against the field. This is called by <see cref="Validate"/>
        /// after performing the common check that the field has a segment in the module data.
        /// Implementations may therefore assume that the data exists.
        /// </summary>
        protected abstract bool ValidateData(FixedContainer context, ModuleData data, [NotNullWhen(false)] out string? error);
    }
}

// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    internal sealed class FieldFormattableString : IModuleDataFormattableString
    {
        private FieldChain<IPrimitiveField> chain;

        public FieldFormattableString(FieldChain<IPrimitiveField> chain) => this.chain = chain;

        public string Format(FixedContainer context, ModuleData data)
        {
            context = chain.GetFinalContext(context);
            var fields = chain.FinalField;
            return fields.GetText(context, data);
        }
    }
}

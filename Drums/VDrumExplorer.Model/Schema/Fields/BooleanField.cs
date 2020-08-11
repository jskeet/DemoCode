// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Concurrent;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field representing a Boolean value.
    /// </summary>
    public sealed class BooleanField : NumericFieldBase
    {
        private static ConcurrentDictionary<NumericCodec, NumericFieldBaseParameters> parameterCache =
            new ConcurrentDictionary<NumericCodec, NumericFieldBaseParameters>();

        private BooleanField(FieldContainer? parent, FieldParameters common, NumericFieldBaseParameters numericBaseParameters)
            : base(parent, common, numericBaseParameters)
        {
        }

        internal BooleanField(FieldContainer? parent, FieldParameters common, NumericCodec codec)
            : this(parent, common,
                  parameterCache.GetOrAdd(codec, c => new NumericFieldBaseParameters(min: 0, max: 1, @default: 0, c)))
        {
        }

        internal override FieldBase WithParent(FieldContainer parent) =>
            new BooleanField(parent, Parameters, NumericBaseParameters);
    }
}

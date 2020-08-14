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
        private static ConcurrentDictionary<(NumericCodec, int), NumericFieldBaseParameters> parameterCache =
            new ConcurrentDictionary<(NumericCodec, int), NumericFieldBaseParameters>();

        private BooleanField(FieldContainer? parent, FieldParameters common, NumericFieldBaseParameters numericBaseParameters)
            : base(parent, common, numericBaseParameters)
        {
        }

        internal BooleanField(FieldContainer? parent, FieldParameters common, NumericCodec codec, int @default = 0)
            : this(parent, common,
                  parameterCache.GetOrAdd((codec, @default), pair => new NumericFieldBaseParameters(min: 0, max: 1, @default: pair.Item2, pair.Item1)))
        {
        }

        internal override FieldBase WithParent(FieldContainer parent) =>
            new BooleanField(parent, Parameters, NumericBaseParameters);
    }
}

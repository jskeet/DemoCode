// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field representing a Boolean value.
    /// </summary>
    public sealed class BooleanField : NumericFieldBase
    {
        private static readonly NumericFieldBaseParameters Boolean8Parameters =
            new NumericFieldBaseParameters(min: 0, max: 1, @default: 0, NumericCodec.Range8);

        private static readonly NumericFieldBaseParameters Boolean32Parameters =
            new NumericFieldBaseParameters(min: 0, max: 1, @default: 0, NumericCodec.Range32);

        internal BooleanField(FieldContainer? parent, FieldParameters common)
            : base(parent, common, GetBaseParametersForSize(common.Size))
        {
        }

        private static NumericFieldBaseParameters GetBaseParametersForSize(int size) => size switch
        {
            1 => Boolean8Parameters,
            4 => Boolean32Parameters,
            _ => throw new ArgumentOutOfRangeException($"Invalid size: {size}")
        };

        internal override FieldBase WithParent(FieldContainer parent) =>
            new BooleanField(parent, Parameters);
    }
}

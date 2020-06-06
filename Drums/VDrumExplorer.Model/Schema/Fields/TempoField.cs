// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Text;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A compound field consisting of a Boolean "sync" switch, a rate/duration, and a musical note.
    /// The switch determines whether the numeric field or the musical note is in use.
    /// </summary>
    public sealed class TempoField : FieldBase
    {
        private static readonly IReadOnlyList<string> MusicalNoteValues = new List<string>
        {
            "Hemidemisemiquaver triplet \U0001d163\u00b3",
            "Hemidemisemiquaver \U0001d163",
            "Demisemiquaver triplet \U0001d162\u00b3",
            "Demisemiquaver \U0001d162",
            "Semiquaver triplet \U0001d161\u00b3",
            "Dotted demisemiquaver \U0001d162.",
            "Semiquaver \U0001d161",
            "Quaver triplet \U0001d160\u00b3",
            "Dotted semiquaver \U0001d161.",
            "Quaver \U0001d160",
            "Crotchet triplet \U0001d15f\u00b3",
            "Dotted quaver \U0001d160.",
            "Crotchet \U0001d15f",
            "Minim triplet \U0001d15e\u00b3",
            "Dotted crotchet \U0001d15f.",
            "Minim \U0001d15e",
            "Semibreve triplet \U0001d15d\u00b3",
            "Dotted minim \U0001d15e.",
            "Semibreve \U0001d15d",
            "Breve triplet \U0001d15c\u00b3",
            "Dotted semibreve \U0001d15d.",
            "Breve \U0001d15c",
        }.AsReadOnly();

        public BooleanField SwitchField { get; }
        public NumericField NumericField { get; }
        public EnumField MusicalNoteField { get; }

        internal TempoField(FieldContainer? parent, FieldParameters common, int min, int max, int @default,
            int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting)
            : this(
                parent, common,
                new BooleanField(parent, new FieldParameters(common.Name + "Sync", common.Description + " Sync", common.Offset, 4)),
                new NumericField(parent, new FieldParameters(common.Name + "Numeric", common.Description + " FIXME", common.Offset + 4, 4), min, max, @default, divisor, multiplier, valueOffset, suffix, customValueFormatting),
                new EnumField(parent, new FieldParameters(common.Name + "Note", common.Description + " FIXME", common.Offset + 8, 4), MusicalNoteValues, 0, 0))
        {
        }

        private TempoField(FieldContainer? parent, FieldParameters common, BooleanField switchField, NumericField numericField, EnumField musicalNoteField)
            : base(parent, common) =>
            (SwitchField, NumericField, MusicalNoteField) = (switchField, numericField, musicalNoteField);

        internal override FieldBase WithParent(FieldContainer parent) =>
            new TempoField(parent, Parameters, (BooleanField) SwitchField.WithParent(parent), (NumericField) NumericField.WithParent(parent), (EnumField) MusicalNoteField.WithParent(parent));
    }
}

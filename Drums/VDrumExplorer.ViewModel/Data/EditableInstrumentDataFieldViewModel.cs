// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.ViewModel.Data
{
    public class EditableInstrumentDataFieldViewModel : DataFieldViewModel<InstrumentDataField>
    {
        public EditableInstrumentDataFieldViewModel(InstrumentDataField model) : base(model)
        {
        }

        protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Instrument));
            RaisePropertyChanged(nameof(InstrumentGroup));
            RaisePropertyChanged(nameof(IsPreset));
            RaisePropertyChanged(nameof(IsUserSample));
            RaisePropertyChanged(nameof(UserSample));
        }

        private ModuleSchema Schema => Model.Context.FieldContainer.Schema;
        public bool IsPreset => InstrumentGroup is object;
        public bool IsUserSample => InstrumentGroup is null;

        public IReadOnlyList<InstrumentGroup> InstrumentGroups => Schema.InstrumentGroups;
        public InstrumentGroup? InstrumentGroup
        {
            get => Model.Instrument.Group;
            set => Instrument = value?.Instruments[0] ?? Schema.UserSampleInstruments[0];
        }

        public Instrument Instrument
        {
            get => Model.Instrument;
            set => Model.Instrument = value;
        }

        public int? UserSample
        {
            get => InstrumentGroup is null ? Instrument.Id + 1 : default(int?);
            set
            {
                if (InstrumentGroup is null == value is null)
                {
                    throw new ArgumentException("Can't switch between user samples and preset instruments with this property");
                }
                if (value is int sampleId)
                {
                    if (sampleId < 1 || sampleId > Schema.UserSampleInstruments.Count)
                    {
                        throw new ArgumentOutOfRangeException($"Invalid user sample number: {sampleId}");
                    }
                    Instrument = Schema.UserSampleInstruments[sampleId - 1];
                }
            }
        }
    }
}

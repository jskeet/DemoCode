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
            // TODO: Maybe map from original property to our property?
            RaisePropertyChanged(nameof(IsPreset));
            RaisePropertyChanged(nameof(IsUserSample));
            RaisePropertyChanged(nameof(Instrument));
            RaisePropertyChanged(nameof(Group));
            RaisePropertyChanged(nameof(UserSample));
        }

        private ModuleSchema Schema => Model.Schema;
        public bool IsPreset => Model.Instrument.Group.Preset;
        public bool IsUserSample => !IsPreset;

        public IReadOnlyList<InstrumentGroup> InstrumentGroups => Schema.InstrumentGroups;

        public InstrumentGroup Group
        {
            get => Model.Instrument.Group;
            set => Model.Group = value;
        }

        public Instrument Instrument
        {
            get => Model.Instrument;
            set
            {
                // This is triggered when we change to user samples.
                // TODO: Work out why, and stop it.
                // (That might have changed now...)
                if (value is null)
                {
                    return;
                }
                Model.Instrument = value;
            }
        }

        public int? UserSample
        {
            get => Group.Preset ? default(int?) : Instrument.Id + 1;
            set
            {
                if (Group.Preset != value is null)
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

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
        // TODO: Move this somewhere else, or have some kind of enum data source.
        private static readonly IReadOnlyList<InstrumentBank> s_instrumentBanks =
            new List<InstrumentBank> { InstrumentBank.Preset, InstrumentBank.UserSamples }.AsReadOnly();

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
            RaisePropertyChanged(nameof(Bank));
            RaisePropertyChanged(nameof(UserSample));
        }

        private ModuleSchema Schema => Model.Schema;
        public bool IsPreset => Bank == InstrumentBank.Preset;
        public bool IsUserSample => Bank == InstrumentBank.UserSamples;

        public IReadOnlyList<InstrumentBank> InstrumentBanks => s_instrumentBanks;
        public IReadOnlyList<InstrumentGroup> InstrumentGroups => Schema.InstrumentGroups;

        public InstrumentGroup? Group
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
                if (value is null)
                {
                    return;
                }
                Model.Instrument = value;
            }
        }

        public InstrumentBank Bank
        {
            get => Model.Bank;
            set => Model.Bank = value;
        }

        public int? UserSample
        {
            get => Group is null ? Instrument.Id + 1 : default(int?);
            set
            {
                if (Group is null == value is null)
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

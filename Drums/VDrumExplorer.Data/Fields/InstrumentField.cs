// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    public class InstrumentField : PrimitiveFieldBase, IPrimitiveField
    {
        /// <summary>
        /// The address within the data that determines whether the instrument is a preset
        /// instrument or a user sample.
        /// </summary>
        public ModuleAddress BankAddress { get; }

        internal InstrumentField(Parameters common, ModuleAddress bankAddress) : base(common) =>
            BankAddress = bankAddress;

        public override string GetText(ModuleData data)
        {
            var instrument = GetInstrument(data);
            return instrument.ToString();
        }

        public Instrument GetInstrument(ModuleData data)
        {
            // TODO: Maybe put this code into ModuleData to avoid the duplication.
            int rawValue = (short) (
                    (data.GetAddressValue(Address) << 12) |
                    (data.GetAddressValue(Address + 1) << 8) |
                    (data.GetAddressValue(Address + 2) << 4) |
                    data.GetAddressValue(Address + 3));
            byte instrumentBank = data.GetAddressValue(BankAddress);            
            return instrumentBank switch
            {
                0 => Schema.PresetInstruments[rawValue],
                1 => Schema.UserSampleInstruments[rawValue],
                _ => throw new InvalidOperationException($"Invalid instrument bank {instrumentBank}")
            };
        }

        public override bool TrySetText(ModuleData data, string text)
        {
            var instrument = Schema.PresetInstruments.Concat(Schema.UserSampleInstruments).FirstOrDefault(inst => inst.Name == text);
            if (instrument != null)
            {
                SetInstrument(data, instrument);
                return false;
            }
            return true;
        }

        public void SetInstrument(ModuleData data, Instrument instrument)
        {
            byte[] idBytes =
            {
                (byte) ((instrument.Id >> 12) & 0xf),
                (byte) ((instrument.Id >> 8) & 0xf),
                (byte) ((instrument.Id >> 4) & 0xf),
                (byte) ((instrument.Id >> 0) & 0xf)
            };
            byte[] bankBytes = new[] { instrument.Group != null ? (byte) 0 : (byte) 1 };
            data.SetMultipleData((Address, idBytes), (BankAddress, bankBytes));
        }

        public override void Reset(ModuleData data) => SetInstrument(data, Schema.PresetInstruments.First());

        protected override bool ValidateData(ModuleData data, out string? error)
        {
            int rawValue = (short) (
               (data.GetAddressValue(Address) << 12) |
               (data.GetAddressValue(Address + 1) << 8) |
               (data.GetAddressValue(Address + 2) << 4) |
               data.GetAddressValue(Address + 3));
            byte instrumentBank = data.GetAddressValue(BankAddress);
            if (instrumentBank > 1)
            {
                error = $"Invalid instrument bank {instrumentBank}";
                return false;
            }
            int maxExclusive = instrumentBank == 0 ? Schema.PresetInstruments.Count : Schema.UserSampleInstruments.Count;
            if (rawValue < 0 || rawValue >= maxExclusive)
            {
                error = $"Invalid instrument ID {rawValue}";
                return false;
            }

            error = null;
            return true;
        }
    }
}

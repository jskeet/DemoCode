using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models
{
    /// <summary>
    /// Top-level information about a module, breaking down into kits.
    /// </summary>
    public sealed class ModuleFields
    {
        /// <summary>
        /// The name of the module, e.g. "TD-17".
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The ID of the module.
        /// </summary>
        public int MidiId { get; }
        
        public IReadOnlyList<FieldSet> FieldSets { get; }
        public IReadOnlyList<KitFields> Kits { get; }
        
        public IReadOnlyList<InstrumentGroup> InstrumentGroups { get; }
        public IReadOnlyDictionary<int, Instrument> InstrumentsById { get; }

        private ModuleFields(string resourceBase, string resourceName)
        {
            JObject json = FieldUtilities.LoadJson($"{resourceBase}.{resourceName}");
            var module = json.ToObject<ModuleJson>();
            Name = module.Name;
            MidiId = FieldUtilities.ParseHex(module.MidiId);
            InstrumentGroups = module.InstrumentGroups
                .Select(ig => new InstrumentGroup(this, ig.Name, ig.Instruments))
                // We assume every group has at least one instrument.
                .OrderBy(ig => ig.Instruments.First().Id)
                .ToList()
                .AsReadOnly();
            InstrumentsById = new ReadOnlyDictionary<int, Instrument>(InstrumentGroups.SelectMany(ig => ig.Instruments).ToDictionary(i => i.Id));

            FieldSets = module.FieldSets.Select(fs => fs.Load(resourceBase, 0)).ToList().AsReadOnly();
            Kits = Enumerable.Range(1, module.Kits)
                .Select(kit => module.KitFieldSets.Load(this, kit, module.InstrumentsPerKit, resourceBase))
                .ToList().AsReadOnly();
        }

        internal static ModuleFields Load(string resourceBase, string resourceName)
            => new ModuleFields(resourceBase, resourceName);

        private class ModuleJson
        {
            public string Name { get; set; }
            public string MidiId { get; set; }
            public int Kits { get; set; }
            public int InstrumentsPerKit { get; set; }
            public List<InstrumentGroupJson> InstrumentGroups { get; set; }
            public List<FieldSetJson> FieldSets { get; set; }
            public KitFieldSetsJson KitFieldSets { get; set; }
        }

        private class InstrumentGroupJson
        {
            public string Name { get; set; }
            public Dictionary<int, string> Instruments { get; set; }
        }

        private class FieldSetJson
        {
            public string File { get; set; }
            public string Offset { get; set; }
            public string Description { get; set; }

            public FieldSet Load(string resourceBase, int parentOffset)
            {
                JObject json = FieldUtilities.LoadJson($"{resourceBase}.{File}");
                return new FieldSet(json, Description, parentOffset + FieldUtilities.ParseHex(Offset));
            }
        }

        private class KitFieldSetsJson
        {
            public string FirstOffset { get; set; }
            public string Gap { get; set; }
            public List<FieldSetJson> FieldSets { get; set; }
            public InstrumentFieldSetsJson InstrumentFieldSets { get; set; }

            public KitFields Load(ModuleFields module, int kit, int instrumentsPerKit, string resourceBase)
            {
                int firstOffset = FieldUtilities.ParseHex(FirstOffset);
                int gap = FieldUtilities.ParseHex(Gap);
                int parentOffset = firstOffset + (kit - 1) * gap;
                return new KitFields(module, kit, FieldSets.Select(fs => fs.Load(resourceBase, parentOffset)).ToList().AsReadOnly(),
                    kf => Enumerable.Range(1, instrumentsPerKit).Select(instrument => InstrumentFieldSets.Load(kf, instrument, parentOffset, resourceBase)).ToList().AsReadOnly());
            }
        }

        private class InstrumentFieldSetsJson
        {
            public string Gap { get; set; }
            public List<FieldSetJson> FieldSets { get; set; }

            public InstrumentFields Load(KitFields kit, int instrument, int parentOffset, string resourceBase)
            {
                int gap = FieldUtilities.ParseHex(Gap);
                int offset = parentOffset + (instrument - 1) * gap;
                return new InstrumentFields(kit, instrument, FieldSets.Select(fs => fs.Load(resourceBase, offset)).ToList().AsReadOnly());
            }
        }
    }
}

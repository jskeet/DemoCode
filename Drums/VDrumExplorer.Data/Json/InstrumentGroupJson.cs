using System.Collections.Generic;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    internal sealed class InstrumentGroupJson
    {
        private static readonly FieldPath instrumentGroupsPath = FieldPath.Root() + "instrumentGroups";
        
        public string? Description { get; set; }
        public Dictionary<int, string>? Instruments { get; set; }

        internal InstrumentGroup ToInstrumentGroup(int index) =>
            new InstrumentGroup(
                ValidateNotNull(instrumentGroupsPath, Description, nameof(Description)),
                index,
                ValidateNotNull(instrumentGroupsPath + Description!, Instruments, nameof(Instruments)));

        public override string ToString() => Description ?? "(Unspecified)";
    }
}

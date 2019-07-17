using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models.TD17
{
    public class TD17ModuleFields
    {
        public FieldSet Current { get; }
        public FieldSet Setup { get; }
        public FieldSet Trigger { get; }
        public IReadOnlyList<TD17KitFields> Kits { get; }

        public TD17ModuleFields(ModuleFields moduleFields)
        {
            Current = moduleFields.FieldSets[0];
            Setup = moduleFields.FieldSets[1];
            Trigger = moduleFields.FieldSets[2];

            Kits = moduleFields.Kits.Select(k => new TD17KitFields(this, k)).ToList().AsReadOnly();
        }

        public static TD17ModuleFields Load()
        {
            var moduleFields = ModuleFields.Load(typeof(TD17ModuleFields).Namespace, "TD17.json");
            return new TD17ModuleFields(moduleFields);
        }
    }
}

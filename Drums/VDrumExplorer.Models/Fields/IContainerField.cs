using System.Collections.Generic;

namespace VDrumExplorer.Models.Fields
{
    public interface IContainerField : IField
    {
        IEnumerable<IField> GetFields(ModuleData data);
    }
}

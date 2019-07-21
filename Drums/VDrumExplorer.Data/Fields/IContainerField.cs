using System.Collections.Generic;

namespace VDrumExplorer.Models.Fields
{
    public interface IContainerField : IField
    {
        IEnumerable<IField> Children(ModuleData data);
    }
}

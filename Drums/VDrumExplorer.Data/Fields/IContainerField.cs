using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public interface IContainerField : IField
    {
        IEnumerable<IField> Children(ModuleData data);
    }
}

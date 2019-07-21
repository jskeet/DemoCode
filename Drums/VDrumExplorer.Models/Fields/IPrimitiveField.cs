namespace VDrumExplorer.Models.Fields
{
    public interface IPrimitiveField : IField
    {
        string GetText(ModuleData data);
    }
}

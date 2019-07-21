namespace VDrumExplorer.Data.Fields
{
    public interface IPrimitiveField : IField
    {
        string GetText(ModuleData data);
    }
}

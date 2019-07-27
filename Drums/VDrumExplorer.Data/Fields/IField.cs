namespace VDrumExplorer.Data.Fields
{
    public interface IField
    {
        string Description { get; }
        FieldPath Path { get; }
        ModuleAddress Address { get; }
        int Size { get; }
    }
}

namespace VDrumExplorer.Data.Fields
{
    public interface IField
    {
        string Description { get; }
        string Path { get; }
        ModuleAddress Address { get; }
        int Size { get; }
    }
}

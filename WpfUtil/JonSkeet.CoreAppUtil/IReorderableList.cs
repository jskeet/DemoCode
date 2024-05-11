namespace JonSkeet.CoreAppUtil;

/// <summary>
/// Interface to support extension methods in JonSkeet.WpfUtil.ReorderableLists.
/// (It's in CoreAppUtil so that SelectableCollection can implement it.)
/// </summary>
public interface IReorderableList
{
    void MoveSelectedItemUp();
    void MoveSelectedItemDown();
    void DeleteSelectedItem();
}

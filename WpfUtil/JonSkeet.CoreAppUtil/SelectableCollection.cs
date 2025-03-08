using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JonSkeet.CoreAppUtil;

public static class SelectableCollections
{
    public static SelectableCollection<T> ToSelectableCollection<T>(this IEnumerable<T> items) where T : class => [.. items];
}

/// <summary>
/// An observable collection which also exposes <see cref="SelectedItem"/>
/// and <see cref="SelectedIndex"/> properties, for use when displaying the
/// collection in a UI.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SelectableCollection<T> : ObservableCollection<T>, IReorderableList where T : class
{
    private static readonly PropertyChangedEventArgs selectedItemChangedArgs = new PropertyChangedEventArgs(nameof(SelectedItem));
    private static readonly PropertyChangedEventArgs selectedIndexChangedArgs = new PropertyChangedEventArgs(nameof(SelectedIndex));

    public SelectableCollection()
    {
    }

    public SelectableCollection(IEnumerable<T> items)
    {
        this.AddRange(items);
    }

    public void AddAndSelect(T item)
    {
        Add(item);
        SelectedIndex = Count - 1;
    }

    private T selectedItem;
    /// <summary>
    /// The selected item in the collection, or null if no item is selected.
    /// </summary>
    public T SelectedItem
    {
        get => selectedItem;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }
            var newIndex = IndexOf(value);
            if (newIndex == -1)
            {
                throw new ArgumentException("Item is not in collection");
            }
            // This will raise the appropriate change notifications.
            SelectedIndex = newIndex;
        }
    }

    private int selectedIndex = -1;

    /// <summary>
    /// The index of the currently-selected item, or -1
    /// if nothing is selected.
    /// </summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value == selectedIndex)
            {
                return;
            }
            if (value == -1)
            {
                selectedIndex = -1;
                selectedItem = null;
                RaiseSelectionChanges();
                return;
            }
            if (value < 0 || value >= Count)
            {
                throw new ArgumentOutOfRangeException("Specified index is out of range");
            }
            selectedIndex = value;
            selectedItem = this[value];
            RaiseSelectionChanges();
        }
    }

    private void RaiseSelectionChanges()
    {
        OnPropertyChanged(selectedItemChangedArgs);
        OnPropertyChanged(selectedIndexChangedArgs);
    }

    protected override void ClearItems()
    {
        SelectedIndex = -1;
        base.ClearItems();
    }

    public void DeleteSelectedItem() => SelectedIndex = this.RemoveSelectedIndex(SelectedIndex);
    public void MoveSelectedItemUp() => this.MoveSelectedIndexUp(SelectedIndex, value => SelectedIndex = value);
    public void MoveSelectedItemDown() => this.MoveSelectedIndexDown(SelectedIndex, value => SelectedIndex = value);

    /// <summary>
    /// Selects the first item, if the collection is non-empty.
    /// </summary>
    public void MaybeSelectFirst()
    {
        if (Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Clears any existing selection. (This is equivalent to setting
    /// <see cref="SelectedIndex"/> to -1, or <see cref="SelectedItem"/> to null.)
    /// </summary>
    public void ClearSelection() => SelectedIndex = -1;

    // TODO: Maybe handle moves, removals etc to keep the selection the same?
}

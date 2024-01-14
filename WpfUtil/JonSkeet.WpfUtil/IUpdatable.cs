namespace JonSkeet.WpfUtil;

/// <summary>
/// Indicates that an item can have its status updated from another one.
/// Used by <see cref="ObservableCollections"/> to merge incoming items.
/// </summary>
/// <typeparam name="T">The element to update from</typeparam>
public interface IUpdatable<T>
{
    void UpdateFrom(T other);
}

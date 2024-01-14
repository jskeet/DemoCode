namespace JonSkeet.WpfUtil;

/// <summary>
/// A wrapper for an item within an observable collection, allowing the value
/// to be modified directly e.g. in a textbox. (I'm surprised this is actually
/// needed, but it *seems* to be.)
/// </summary>
public class ObservableItemWrapper<T> : ViewModelBase
{
    private T value;
    public T Value
    {
        get => this.value;
        set => SetProperty(ref this.value, value);
    }

    public ObservableItemWrapper(T value) => Value = value;
}

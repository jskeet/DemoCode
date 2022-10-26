using DigiMixer.Wpf.Utilities;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DigiMixer.Wpf;

/// <summary>
/// Base for view model classes, supporting simple property changes.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    private static ConcurrentDictionary<Type, Dictionary<string, string[]>> relatedPropertiesByTypeThenName = new ConcurrentDictionary<Type, Dictionary<string, string[]>>();
    private static ConcurrentDictionary<Type, Dictionary<string, MethodInfo>> reactionMethodByTypeThenName = new ConcurrentDictionary<Type, Dictionary<string, MethodInfo>>();

    private PropertyChangedEventHandler propertyChanged;

    // Implement the event subscription manually so ViewModels can subscribe and unsubscribe
    // from events raised by their models.
    public event PropertyChangedEventHandler PropertyChanged
    {
        add
        {
            if (NotifyPropertyChangedHelper.AddHandler(ref propertyChanged, value))
            {
                OnPropertyChangedHasSubscribers();
            }
        }
        remove
        {
            if (NotifyPropertyChangedHelper.RemoveHandler(ref propertyChanged, value))
            {
                OnPropertyChangedHasNoSubscribers();
            }
        }
    }

    /// <summary>
    /// Called when the PropertyChangedEventHandler subscription that goes from "no subscribers"
    /// to "at least one subscriber". Derived classes should perform any event subscriptions here.
    /// </summary>
    protected virtual void OnPropertyChangedHasSubscribers()
    {
    }

    /// <summary>
    /// Called when the PropertyChangedEventHandler subscription that goes from "at least one subscriber"
    /// to "no subscribers". Derived classes should perform any event unsubscriptions here.
    /// </summary>
    protected virtual void OnPropertyChangedHasNoSubscribers()
    {
    }

    protected void RaisePropertyChanged(string name) => propertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        RaisePropertyChanged(name!);
        return true;
    }

    protected bool SetProperty<T>(T currentValue, T newValue, Action<T> setter, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
        {
            return false;
        }
        setter(newValue);
        RaisePropertyChanged(name!);
        return true;
    }
}

/// <summary>
/// Base class for view models which wrap a single model.
/// </summary>
/// <typeparam name="TModel">The model type this is based on.</typeparam>
public abstract class ViewModelBase<TModel> : ViewModelBase
{
    public TModel Model { get; }

    protected ViewModelBase(TModel model) =>
        Model = model;

    protected override void OnPropertyChangedHasSubscribers()
    {
        if (Model is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged += OnPropertyModelChanged;
        }
    }

    protected override void OnPropertyChangedHasNoSubscribers()
    {
        if (Model is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged -= OnPropertyModelChanged;
        }
    }

    protected virtual void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
    {
    }
}

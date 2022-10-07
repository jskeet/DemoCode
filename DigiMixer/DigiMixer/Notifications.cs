using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigiMixer;

internal static class Notifications
{
    internal static bool SetProperty<T>(this INotifyPropertyChanged parent, PropertyChangedEventHandler? propertyChanged, ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        propertyChanged?.Invoke(parent, new PropertyChangedEventArgs(name!));
        return true;
    }
}

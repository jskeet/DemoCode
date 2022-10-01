using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using System.Reflection;

namespace JonSkeet.WpfUtil;

public static class Notifications
{
    public static IDisposable Subscribe(INotifyPropertyChanged source, string property, Action<object, object> handler)
    {
        PropertyInfo propertyInfo = source.GetType().GetProperty(property);
        return Subscribe(source, (sender, args) =>
        {
            if (args.PropertyName == property)
            {
                handler(sender, propertyInfo.GetValue(sender));
            }
        });
    }

    public static IDisposable Subscribe(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
    {
        source.PropertyChanged += handler;
        return Disposables.ForAction(() => source.PropertyChanged -= handler);
    }

    public static void MaybeSubscribe(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
    {
        if (source is not null)
        {
            source.PropertyChanged += handler;
        }
    }

    public static void MaybeUnsubscribe(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
    {
        if (source is not null)
        {
            source.PropertyChanged -= handler;
        }
    }
}

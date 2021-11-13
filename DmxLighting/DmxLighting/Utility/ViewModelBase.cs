// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DmxLighting.Utility
{
    /// <summary>
    /// Base for view model classes, supporting simple property changes.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private static ConcurrentDictionary<Type, Dictionary<string, string[]>> relatedPropertiesByTypeThenName = new ConcurrentDictionary<Type, Dictionary<string, string[]>>();

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

        protected void RaisePropertyChanged(string name)
        {
            RaisePropertyChanged(name, propertiesSoFar: null);
        }

        private void RaisePropertyChanged(string name, LinkedList<string> propertiesSoFar)
        {
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            propertiesSoFar?.AddLast(name);
            var relatedProperties = GetRelatedProperties(name);
            if (relatedProperties is object)
            {
                if (propertiesSoFar is null)
                {
                    propertiesSoFar = new LinkedList<string>();
                    propertiesSoFar.AddLast(name);
                }
                foreach (var relatedProperty in relatedProperties)
                {
                    if (!propertiesSoFar.Contains(relatedProperty))
                    {
                        RaisePropertyChanged(relatedProperty, propertiesSoFar);
                    }
                }
            }
        }

        /// <summary>
        /// Returns an array of related property names, to raise a property changed event on each of them.
        /// These can contain cycles, and the change event will only be raised once. Return null for no properties.
        /// </summary>
        private string[] GetRelatedProperties(string name)
        {
            var relatedPropertiesByName = relatedPropertiesByTypeThenName.GetOrAdd(GetType(), CreateRelatedPropertiesMap);
            return relatedPropertiesByName.TryGetValue(name, out var value) ? value : null;
        }

        private Dictionary<string, string[]> CreateRelatedPropertiesMap(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(prop => (prop, prop.GetCustomAttribute<RelatedPropertiesAttribute>()))
                .Where(pair => pair.Item2 is object)
                .ToDictionary(pair => pair.Item1.Name, pair => pair.Item2.PropertyNames);

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

}

﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Base for view model classes, supporting simple property changes.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler? propertyChanged;

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

        protected void RaisePropertyChanged(string name) =>
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
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
        protected TModel Model { get; }

        private protected ViewModelBase(TModel model) =>
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

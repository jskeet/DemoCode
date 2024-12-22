// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Windows.Input;

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Base class for command implementations.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Simple command that is never enabled, and throws on execution
        /// </summary>
        public static DelegateCommand NotImplemented { get; } = new DelegateCommand(() => throw new NotImplementedException(), false);

        public event EventHandler? CanExecuteChanged;

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool CanExecute(object parameter) => Enabled;

        public abstract void Execute(object parameter);
    }

    /// <summary>
    /// Command which ignores parameters.
    /// </summary>
    public sealed class DelegateCommand : CommandBase
    {
        private readonly Action action;

        public DelegateCommand(Action action, bool enabled) =>
            (this.action, Enabled) = (action, enabled);

        public override void Execute(object parameter) => action();
    }

    /// <summary>
    /// Command which expects a parameter of a specified type.
    /// </summary>
    public sealed class DelegateCommand<T> : CommandBase
    {
        private readonly Action<T> action;

        public DelegateCommand(Action<T> action, bool enabled) =>
            (this.action, Enabled) = (action, enabled);

        public override void Execute(object parameter) => action((T) parameter);
    }

    /// <summary>
    /// Command which is enabled or not based on the parameter it is provided.
    /// </summary>
    public sealed class ConditionallyEnabledDelegateCommand<T> : ICommand
    {
        private readonly IViewServices viewServices;
        private readonly Action<T> action;
        private readonly Func<T, bool> enabled;

        public ConditionallyEnabledDelegateCommand(IViewServices viewServices, Action<T> action, Func<T, bool> enabled)
        {
            this.viewServices = viewServices;
            this.action = action;
            this.enabled = enabled;
        }

        public event EventHandler CanExecuteChanged
        {
            add => viewServices.AddRequerySuggestion(value);
            remove => viewServices.RemoveRequerySuggestion(value);
        }

        public bool CanExecute(object parameter) => parameter is T param && enabled(param);

        public void Execute(object parameter) => action((T) parameter);
    }
}

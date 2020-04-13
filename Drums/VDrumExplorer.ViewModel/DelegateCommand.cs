// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Windows.Input;

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Simple ICommand implementation with explicit enablement.
    /// </summary>
    public sealed class DelegateCommand : ICommand
    {
        private readonly Action action;
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

        public DelegateCommand(Action action, bool enabled) =>
            (this.action, this.enabled) = (action, enabled);

        public bool CanExecute(object parameter) => Enabled;

        public void Execute(object parameter) => action();
    }
}

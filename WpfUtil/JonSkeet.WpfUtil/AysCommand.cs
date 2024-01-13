// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using System.Windows.Input;

namespace JonSkeet.WpfUtil;

public interface ILabeledCommand : ICommand
{
    public string Label { get; }
}

// FIXME: Rename this!
public class AysCommand : ILabeledCommand
{
    private Func<object, Task> action;
    private Func<object, bool> canExecute;

    public string Label { get; }

    public event EventHandler CanExecuteChanged;

    private AysCommand(Func<object, Task> action, Func<object, bool> canExecute, string label)
    {
        this.action = action;
        this.canExecute = canExecute;
        Label = label;
    }

    public static AysCommand FromAction<TParam>(Func<TParam, Task> action) =>
        new AysCommand(p => action((TParam) p), _ => true, "Unlabeled");

    public static AysCommand FromAction<TParam>(Action<TParam> action) =>
        FromAction<TParam>(p => { action(p); return Task.CompletedTask; });

    public static AysCommand FromAction(Func<Task> action) =>
        new AysCommand(_ => action(), _ => true, "Unlabeled");

    public static AysCommand FromAction(Action action) =>
        FromAction<object>(_ => action());

    public bool CanExecute(object parameter) => canExecute(parameter);

    public async void Execute(object parameter) => await action(parameter);

    public AysCommand WithCanExecute(bool canExecute) => new AysCommand(action, _ => canExecute, Label);

    // TODO: If we call this and then WithLabel on the result, we lose the change propagation.
    public AysCommand WithCanExecuteProperty(INotifyPropertyChanged source, string name)
    {
        var property = source.GetType().GetProperty(name);
        if (property is null)
        {
            throw new ArgumentException($"Property {source.GetType().Name}.{name} not found.");
        }
        if (property.PropertyType != typeof(bool))
        {
            throw new ArgumentException($"Property {source.GetType().Name}.{name} is not Boolean.");
        }
        var command = new AysCommand(action, _ => (bool) property.GetValue(source), Label);
        Notifications.Subscribe(source, name, (_, _) => command.CanExecuteChanged?.Invoke(command, EventArgs.Empty));
        return command;
    }

    public AysCommand WithLabel(string label) => new AysCommand(action, canExecute, label);
}

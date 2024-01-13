// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using System.Windows.Input;

namespace JonSkeet.WpfUtil;

/// <summary>
/// A label for a command. This is slightly hacky, but has proved useful in At Your Service.
/// </summary>
public interface ILabeledCommand : ICommand
{
    public string Label { get; }
}

/// <summary>
/// A command based on an action (which may be asynchronous).
/// </summary>
public class ActionCommand : ILabeledCommand
{
    private Func<object, Task> action;
    private Func<object, bool> canExecute;

    public string Label { get; }

    public event EventHandler CanExecuteChanged;

    private ActionCommand(Func<object, Task> action, Func<object, bool> canExecute, string label)
    {
        this.action = action;
        this.canExecute = canExecute;
        Label = label;
    }

    public static ActionCommand FromAction<TParam>(Func<TParam, Task> action) =>
        new ActionCommand(p => action((TParam) p), _ => true, "Unlabeled");

    public static ActionCommand FromAction<TParam>(Action<TParam> action) =>
        FromAction<TParam>(p => { action(p); return Task.CompletedTask; });

    public static ActionCommand FromAction(Func<Task> action) =>
        new ActionCommand(_ => action(), _ => true, "Unlabeled");

    public static ActionCommand FromAction(Action action) =>
        FromAction<object>(_ => action());

    public bool CanExecute(object parameter) => canExecute(parameter);

    public async void Execute(object parameter) => await action(parameter);

    public ActionCommand WithCanExecute(bool canExecute) => new ActionCommand(action, _ => canExecute, Label);

    // TODO: If we call this and then WithLabel on the result, we lose the change propagation.
    public ActionCommand WithCanExecuteProperty(INotifyPropertyChanged source, string name)
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
        var command = new ActionCommand(action, _ => (bool) property.GetValue(source), Label);
        Notifications.Subscribe(source, name, (_, _) => command.CanExecuteChanged?.Invoke(command, EventArgs.Empty));
        return command;
    }

    public ActionCommand WithLabel(string label) => new ActionCommand(action, canExecute, label);
}

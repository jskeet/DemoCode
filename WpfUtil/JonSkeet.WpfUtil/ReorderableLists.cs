using JonSkeet.CoreAppUtil;
using System.Windows.Controls;
using System.Windows.Input;

namespace JonSkeet.WpfUtil;

public static class ReorderableLists
{
    public static bool MaybeHandleKeyboard(this IReorderableList viewModel, ListBox listBox, KeyEventArgs e)
    {
        var modifiers = e.KeyboardDevice.Modifiers;
        if (e.Key == Key.Up && modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            viewModel.MoveSelectedItemUp();
            listBox.FocusOnSelectedItem();
            return true;
        }
        else if (e.Key == Key.Down && modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            viewModel.MoveSelectedItemDown();
            listBox.FocusOnSelectedItem();
            return true;
        }
        else if (e.Key == Key.Delete && modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            viewModel.DeleteSelectedItem();
            listBox.FocusOnSelectedItem();
            return true;
        }
        return false;
    }
}


using JonSkeet.CoreAppUtil;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace JonSkeet.WpfUtil;

/// <summary>
/// Extensions to WPF classes that don't easily fit into other places.
/// </summary>
public static class WpfExtensions
{
    public static T GetVisualTreeDescendant<T>(this DependencyObject source) where T : class
    {
        if (source is null)
        {
            return null;
        }
        int count = VisualTreeHelper.GetChildrenCount(source);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(source, i);
            if (child is T result)
            {
                return result;
            }
            if (child.GetVisualTreeDescendant<T>() is T recursiveResult)
            {
                return recursiveResult;
            }
        }
        return null;
    }

    public static Task ScrollCurrentAndNextIntoView<T>(this ListBox listBox, SelectableCollection<T> items) where T : class =>
        ScrollCurrentAndNextIntoView(listBox, items, items.SelectedIndex);

    public static async Task ScrollCurrentAndNextIntoView<T>(this ListBox listBox, IReadOnlyList<T> items, int currentIndex)
    {
        if (currentIndex < 0)
        {
            return;
        }
        // Scroll the next item into view if there is one.
        if (currentIndex + 1 < items.Count)
        {
            listBox.ScrollIntoView(items[currentIndex + 1]);
        }
        // Scroll the current item into view, when the dispatcher is idle.
        // This waiting is required to ensure that the previous call fully completes
        // first, as ScrollIntoView is *effectively* asynchronous.
        await listBox.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            () => listBox.ScrollIntoView(items[currentIndex]));
    }

    /// <summary>
    /// Attempts to focus on the selected item in the given listbox.
    /// </summary>
    /// <returns>Whether the selected item was found (and therefore whether focus was requested).</returns>
    public static bool FocusOnSelectedItem(this ListBox listBox)
    {
        var selectedItem = listBox.SelectedItem;
        if (selectedItem is null)
        {
            return false;
        }
        var container = listBox.ItemContainerGenerator?.ContainerFromItem(selectedItem) as UIElement;
        // Sometimes we need to update the layout in order to get the container.
        if (container is null)
        {
            listBox.UpdateLayout();
            container = listBox.ItemContainerGenerator?.ContainerFromItem(selectedItem) as UIElement;
        }
        container?.Focus();
        return container is not null;
    }
}

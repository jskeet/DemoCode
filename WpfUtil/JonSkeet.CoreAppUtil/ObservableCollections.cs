using System.Collections.ObjectModel;

namespace JonSkeet.CoreAppUtil;

public static class ObservableCollections
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
    {
        var ret = new ObservableCollection<T>();
        AddRange(ret, items);
        return ret;
    }

    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Clears <paramref name="collection"/> and then adds each item from <paramref name="newContent"/>.
    /// </summary>
    public static void ReplaceContentWith<T>(this ObservableCollection<T> collection, IEnumerable<T> newContent)
    {
        collection.Clear();
        collection.AddRange(newContent);
    }

    /// <summary>
    /// Like <see cref="ReplaceContentWith"/> but changes are performed in-place. Existing items are updated with the new content
    /// (if they are not equal to the existing lines),
    /// new items are added where necessary, and then the remainder is trimmed.
    /// </summary>
    /// <param name="trimStart">If this is true, excess lines are removed from the start of <paramref name="collection"/>;
    /// otherwise, excess lines are removed from the end of the collection.</param>
    public static void ReplaceContentWithoutClearing<T>(this ObservableCollection<T> collection, IReadOnlyList<T> newContent, bool trimStart)
    {
        while (collection.Count > newContent.Count)
        {
            collection.RemoveAt(trimStart ? 0 : collection.Count - 1);
        }
        for (int i = 0; i < newContent.Count; i++)
        {
            if (i < collection.Count)
            {
                if (!EqualityComparer<T>.Default.Equals(collection[i], newContent[i]))
                {
                    collection[i] = newContent[i];
                }
            }
            else
            {
                collection.Add(newContent[i]);
            }
        }
    }

    public static ObservableCollection<ObservableItemWrapper<T>> ToObservableCollectionOfWrappers<T>(this IEnumerable<T> items) =>
        items.Select(item => new ObservableItemWrapper<T>(item)).ToObservableCollection();

    public static List<T> Unwrap<T>(this ObservableCollection<ObservableItemWrapper<T>> source) =>
        source.Select(item => item.Value).ToList();

    /// <summary>
    /// Removes the given item (expected to be the selected item in a list) and returns
    /// the new item to select, which is the item currently after it (or before it if it's at the end of the list),
    /// or null if the collection is empty after the removal.
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="source">The collection to remove from</param>
    /// <param name="item">The item to remove, if found.</param>
    /// <returns>The new item to select, or <paramref name="item"/> if it wasn't found, or null if the collection is now empty.</returns>
    public static T RemoveSelected<T>(this ObservableCollection<T> source, T item) where T : class
    {
        var index = source.IndexOf(item);
        if (index == -1)
        {
            return item;
        }
        source.RemoveAt(index);
        return
            index < source.Count ? source[index]
            : source.Count > 0 ? source[source.Count - 1]
            : null;
    }

    public static void MoveSelectedItemUp<TElement>(this ObservableCollection<TElement> collection, TElement selectedItem, Action<TElement> selector) where TElement : class
    {
        if (selectedItem is null)
        {
            return;
        }

        int index = collection.IndexOf(selectedItem);
        if (index == -1 || index == 0)
        {
            return;
        }

        selector(null);
        collection.Move(index, index - 1);
        selector(selectedItem);
    }

    public static void MoveSelectedItemDown<TElement>(this ObservableCollection<TElement> collection, TElement selectedItem, Action<TElement> selector) where TElement : class
    {
        if (selectedItem is null)
        {
            return;
        }

        int index = collection.IndexOf(selectedItem);
        if (index == -1 || index == collection.Count - 1)
        {
            return;
        }

        // This is an ugly way of doing it, but it means we never remove
        // the selected item from the collection... which means we can
        // refocus on it afterwards. Abstraction violation!
        selector(null);
        collection.Move(index, index + 1);
        selector(selectedItem);
    }

    public static int RemoveSelectedIndex<T>(this ObservableCollection<T> source, int index)
    {
        if (index < 0 || index >= source.Count)
        {
            return index;
        }
        source.RemoveAt(index);
        return source.Count == 0 ? -1 : Math.Min(index, source.Count - 1);
    }


    public static void MoveSelectedIndexUp<TElement>(this ObservableCollection<TElement> source, int index, Action<int> selector)
    {
        if (index == -1 || index == 0)
        {
            return;
        }

        selector(-1);
        source.Move(index, index - 1);
        selector(index - 1);
    }

    public static void MoveSelectedIndexDown<TElement>(this ObservableCollection<TElement> source, int index, Action<int> selector)
    {
        if (index == -1 || index == source.Count - 1)
        {
            return;
        }

        selector(-1);
        source.Move(index, index + 1);
        selector(index + 1);
    }

    /// <summary>
    /// Merges the given new set of items with the existing collection, expecting both the existing
    /// collection and the new one to be ordered by a given key selector.
    /// </summary>
    public static void MergeOrdered<TElement, TKey>(this ObservableCollection<TElement> existing, IEnumerable<TElement> incoming, Func<TElement, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        int nextExistingIndex = 0;

        foreach (var incomingElement in incoming)
        {
            var incomingKey = keySelector(incomingElement);

            bool incomingItemHandled = false;
            while (nextExistingIndex < existing.Count)
            {
                var existingElement = existing[nextExistingIndex];
                var existingKey = keySelector(existingElement);
                var comparison = existingKey.CompareTo(incomingKey);

                if (comparison < 0)
                {
                    // Existing item is earlier than new one. Delete the existing item, and
                    // continue (index doesn't change).
                    existing.RemoveAt(nextExistingIndex);
                    continue;
                }
                else if (comparison == 0)
                {
                    // Existing item is equal to new one.
                    // Potentially update the item, move our "next index" to
                    // skip over this one, and we're done.
                    if (existingElement is IUpdatable<TElement> updatable)
                    {
                        updatable.UpdateFrom(incomingElement);
                    }
                    nextExistingIndex++;
                    incomingItemHandled = true;
                    break;
                }
                else
                {
                    // New item comes before existing one. Insert the new item,
                    // move the "next index" to skip over it, and we're done.
                    existing.Insert(nextExistingIndex, incomingElement);
                    nextExistingIndex++;
                    incomingItemHandled = true;
                    break;
                }
            }

            // TODO: try to refactor this to be nicer
            if (!incomingItemHandled)
            {
                // We've exhausted our old list. Just add this to the end.
                if (nextExistingIndex == existing.Count)
                {
                    existing.Add(incomingElement);
                    nextExistingIndex++;
                    continue;
                }
            }
        }

        // Anything still left needs to be removed. Remove from the end as a
        // generally more collection-friendly approach.
        while (nextExistingIndex < existing.Count)
        {
            existing.RemoveAt(existing.Count - 1);
        }
    }
}

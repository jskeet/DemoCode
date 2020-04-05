using System;
using System.Collections;
using System.Collections.Generic;

namespace VDrumExplorer.Utility
{
    /// <summary>
    /// Factory methods for <see cref="SingleItemCollection{T}"/>.
    /// </summary>
    public static class SingleItemCollection
    {
        /// <summary>
        /// Creates an instance of <see cref="SingleItemCollection{T}"/> for the given item.
        /// This method exists to avoid the type argument having to be specified directly
        /// when calling the constructor.
        /// </summary>
        /// <typeparam name="T">The element type of the new collection.</typeparam>
        /// <param name="item">The item within the collection.</param>
        /// <returns>A new collection with just the single element.</returns>
        public static SingleItemCollection<T> Of<T>(T item) =>
            new SingleItemCollection<T>(item);
    }

    /// <summary>
    /// A read-only collection which only ever has exactly one item, provided on construction.
    /// This is used for the root of tree hierarchies which are required to bind to collections.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SingleItemCollection<T> : IReadOnlyList<T>
    {
        private readonly T item;

        public SingleItemCollection(T item) =>
            this.item = item;

        public T this[int index] => index == 0 ? item : throw new IndexOutOfRangeException();

        public int Count => 1;

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<T>
        {
            private readonly SingleItemCollection<T> parent;
            private int index = -1; 

            internal Enumerator(SingleItemCollection<T> parent) =>
                this.parent = parent;

            public T Current => index == 0 ? parent.item : throw new InvalidOperationException();

            object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (index < 1)
                {
                    index++;
                    return index == 0;
                }
                return false;
            }

            public void Dispose() { }
            public void Reset() { }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Cosmos.Build.Builder.Collections
{
    internal class ObservableFixedSizeStack<T> : IEnumerable<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private T[] _items;

        public ObservableFixedSizeStack(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            _items = new T[size];
        }

        public void Push(T item)
        {
            Array.Copy(_items, 1, _items, 0, _items.Length - 1);
            _items[_items.Length - 1] = item;

            OnCollectionChanged();
        }

        public void Clear() => Array.Clear(_items, 0, _items.Length);

        private void OnCollectionChanged() => CollectionChanged?.Invoke(
            this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}

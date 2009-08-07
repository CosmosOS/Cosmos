using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDAm.Framework.Collections
{
    public class MruDictionary<TKey, TValue>
    {
        int maxCapacity; 
        // The linked list of items in MRU order
        private LinkedList<MruItem> items;

        // The dictionary of keys and nodes
        private Dictionary<TKey, LinkedListNode<MruItem>> itemIndex;

        class MruItem
        {
            private TKey _key;
            private TValue _value;
            public MruItem(TKey k, TValue v)
            {
                _key = k;
                _value = v;
            }

            public TKey Key
            {
                get { return _key; }
            }

            public TValue Value
            {
                get { return _value; }
            }
        }

        public MruDictionary(int cap)
        {
            maxCapacity = cap;
            items = new LinkedList<MruItem>();
            itemIndex = new Dictionary<TKey, LinkedListNode<MruItem>>(maxCapacity);
        }

        public void Add(TKey key, TValue value)
        {
            // Check to see if the key is already in the dictionary
            if (itemIndex.ContainsKey(key))
            {
                throw new ArgumentException("An item with the same key already exists.");
            }

            // If the list is at capacity, remove the LRU item.
            if (itemIndex.Count == maxCapacity)
            {
                LinkedListNode<MruItem> node = items.Last;
                items.RemoveLast();
                itemIndex.Remove(node.Value.Key);
            }

            // Create a node for this key/value pair
            LinkedListNode<MruItem> newNode = new LinkedListNode<MruItem>(new MruItem(key, value));
            // Add to the items list
            items.AddFirst(newNode);
            // and to the dictionary
            itemIndex.Add(key, newNode);
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            LinkedListNode<MruItem> node;
            if (itemIndex.TryGetValue(key, out node))
            {
                value = node.Value.Value;
                // move this node to the front of the list
                items.Remove(node);
                items.AddFirst(node);
                return true;
            }
            value = default(TValue);
            return false;
        }

    } //MruDictionary
}

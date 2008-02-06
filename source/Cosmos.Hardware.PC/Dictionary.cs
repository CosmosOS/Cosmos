using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC
{
    public class Dictionary<Key, Value>
    {
        private List<DictionaryItem> items = new List<DictionaryItem>();

        public class DictionaryItem
        {
            Key key;

            public Key Key
            {
                get { return key; }
                set { key = value; }
            }

            Value value;

            public Value Value
            {
                get { return value; }
                set { value = this.value; }
            }

            public DictionaryItem(Key key, Value value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public Value this[Key key]
        {
            get
            {
                Value res;
                TryGetValue(key, out res);
                return res;
            }
            set
            {
                TrySetValue(key, value);
            }
        }

        public void Add(Key key, Value value)
        {
            items.Add(new Dictionary<Key, Value>.DictionaryItem(key, value));
        }

        public bool TryGetValue(Key key, out Value value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (((IComparable)items[i].Key).CompareTo(key) == 0)
                {
                    value = items[i].Value;
                    return true;
                }
            }
            value = default(Value);
            return false;
        }

        public bool TrySetValue(Key key, Value value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (((IComparable)items[i].Key).CompareTo(key) == 0)
                {
                    items[i].Value = value;
                    return true;
                }
            }
            return false;
        }
    }
}

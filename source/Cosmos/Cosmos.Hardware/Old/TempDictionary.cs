using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware
{
    public class TempDictionary<Value>
    {
        private List<DictionaryItem> items = new List<DictionaryItem>();

        public class DictionaryItem
        {
            UInt32 key;

            public UInt32 Key
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

            public DictionaryItem(UInt32 key, Value value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public Value this[UInt32 key]
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

        public void Add(UInt32 key, Value value)
        {
            items.Add(new TempDictionary<Value>.DictionaryItem(key, value));
        }

        public bool TryGetValue(UInt32 key, out Value value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Key == key)
                {
                    value = items[i].Value;
                    return true;
                }
            }
            value = default(Value);
            return false;
        }

        public bool ContainsKey(UInt32 key)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Key == key)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TrySetValue(UInt32 key, Value value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Key == key)
                {
                    items[i].Value = value;
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC
{
    public class Dictionary
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

            String value;

            public String Value
            {
                get { return value; }
                set { value = this.value; }
            }

            public DictionaryItem(UInt32 key, String value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public String this[UInt32 key]
        {
            get
            {
                String res;
                TryGetValue(key, out res);
                return res;
            }
            set
            {
                TrySetValue(key, value);
            }
        }

        public void Add(UInt32 key, String value)
        {
            items.Add(new Dictionary.DictionaryItem(key, value));
        }

        public bool TryGetValue(UInt32 key, out String value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Key == key)
                {
                    value = items[i].Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public bool TrySetValue(UInt32 key, String value)
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

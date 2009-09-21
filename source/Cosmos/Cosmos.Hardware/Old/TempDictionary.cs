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

            Value _value;

            public Value Value
            {
                get { return _value; }
                set { value = this._value; }
            }

            public DictionaryItem(UInt32 key, Value avalue)
            {
                this.key = key;
                Console.Write("..ctor. this.key = ");
                Interrupts.WriteNumber(this.key, 32);
                Console.WriteLine("");
                this._value = avalue;
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
          Console.Write("In .Add. key = ");
          Interrupts.WriteNumber(key, 32);
            items.Add(new TempDictionary<Value>.DictionaryItem(key, value));
            Console.WriteLine("");
            Console.Write("  After adding, count is: ");
            Interrupts.WriteNumber((uint)items.Count, 32);
            Console.Write(" and key is ");
            Interrupts.WriteNumber(items[items.Count - 1].Key, 32);
            Console.WriteLine("");
        }

        public bool TryGetValue(UInt32 key, out Value value)
        {
          //Console.Write("Number of items found: ");
          //Console.WriteLine(items.Count.ToString());
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
          Console.Write("Number of items found: ");
          Interrupts.WriteNumber((uint)items.Count, 32);
          Console.WriteLine("");
          for (int i = 0; i < items.Count; i++)
            {
            Console.Write("   Item ");
            Interrupts.WriteNumber((uint)i, 8);
            Console.Write(" Key ");
            Interrupts.WriteNumber(items[i].Key, 32);
            Console.WriteLine("");

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

        public void Remove(UInt32 key)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Key == key)
                {
                    items.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

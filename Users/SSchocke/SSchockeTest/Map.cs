using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSchockeTest
{
    public abstract class Map<TKey, TValue>
    {
        private int capacity;
        private int count;
        private TKey[] keys;
        private TValue[] values;

        public Map()
            : this(8)
        { }

        public Map(int initialCapacity)
        {
            count = 0;
            capacity = initialCapacity;
            keys = new TKey[capacity];
            values = new TValue[capacity];
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (ContainsKey(key) == true)
            {
                throw new ArgumentException("Key already exists", "key");
            }

            if (count == capacity)
            {
                growStorage();
            }

            keys[count] = key;
            values[count] = value;
            count++;
        }

        public void Clear()
        {
            count = 0;
            keys = new TKey[capacity];
            values = new TValue[capacity];
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            for (int i = 0; i < count; i++)
            {
                if (keysEqual(key, keys[i]) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsValue(TValue value)
        {
            throw new NotImplementedException("We really need to have RuntimeHelpers.Equals() for this");
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            int i = 0;
            while ((i < count) && (keysEqual(key, keys[i]) == false))
            {
                i++;
            }
            if (i < count - 1)
            {
                Array.Copy(keys, i + 1, keys, i, count - i - 1);
                Array.Copy(values, i + 1, values, i, count - i - 1);
            }
            count--;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            for (int i = 0; i < count; i++)
            {
                if (keysEqual(key, keys[i]) == true)
                {
                    value = values[i];
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        public int Count
        {
            get { return count; }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                for (int i = 0; i < count; i++)
                {
                    if (keysEqual(key, keys[i]) == true)
                    {
                        return values[i];
                    }
                }

                throw new KeyNotFoundException("Key does not exist in this collection");
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                for (int i = 0; i < count; i++)
                {
                    if (keysEqual(key, keys[i]) == true)
                    {
                        values[i] = value;
                        return;
                    }
                }

                Add(key, value);
            }
        }

        public TKey[] Keys
        {
            get
            {
                TKey[] usedKeys = new TKey[count];
                Array.Copy(keys, usedKeys, count);

                return usedKeys;
            }
        }
        public TValue[] Values
        {
            get
            {
                TValue[] usedValues = new TValue[count];
                Array.Copy(values, usedValues, count);

                return usedValues;
            }
        }

        protected abstract bool keysEqual(TKey key1, TKey key2);

        private void growStorage()
        {
            capacity *= 2;

            TKey[] newKeys = new TKey[capacity];
            TValue[] newValues = new TValue[capacity];
            Array.Copy(keys, newKeys, count);
            Array.Copy(values, newValues, count);

            keys = newKeys;
            values = newValues;
        }
    }
}

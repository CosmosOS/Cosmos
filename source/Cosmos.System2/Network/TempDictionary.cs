using System;
using System.Collections.Generic;

namespace Cosmos.System.Network
{
    internal class TempDictionary<TValue>
    {
        private List<UInt32> mKeys;
        private List<TValue> mValues;

        public int Count { get; private set; }

        public TempDictionary()
            :this(2)
        { }
        public TempDictionary(int initialSize)
        {
            this.mKeys = new List<UInt32>(initialSize);
            this.mValues = new List<TValue>(initialSize);
        }

        public UInt32[] Keys
        {
            get
            {
                return this.mKeys.ToArray();
            }
        }
        public TValue[] Values
        {
            get
            {
                return this.mValues.ToArray();
            }
        }

        public TValue this[UInt32 key]
        {
            get
            {
                for (int i = 0; i < mKeys.Count; i++)
                {
                    if (mKeys[i].CompareTo(key) == 0)
                    {
                        return mValues[i];
                    }
                }

                return default(TValue);
            }
            set
            {
                for (int i = 0; i < mKeys.Count; i++)
                {
                    if (mKeys[i].CompareTo(key) == 0)
                    {
                        mValues[i] = value;
                        return;
                    }
                }

                throw new ArgumentOutOfRangeException("key", "Key not found");
            }
        }

        public bool ContainsKey(UInt32 key)
        {
            for (int i = 0; i < mKeys.Count; i++)
            {
                if (mKeys[i] == key)
                {
                    return true;
                }
            }

            return false;
        }

        public void Add(UInt32 key, TValue val)
        {
            if (ContainsKey(key) == true)
            {
                throw new ArgumentException("key", "Key already exists");
            }

            mKeys.Add(key);
            mValues.Add(val);
        }

        public void Remove(UInt32 key)
        {
            int idx = mKeys.IndexOf(key);
            if (idx != -1)
            {
                this.Remove(idx);
            }
        }
        public void Remove(int index)
        {
            if (index > mKeys.Count - 1)
            {
                throw new ArgumentOutOfRangeException("index", "No such index");
            }

            mKeys.RemoveAt(index);
            mValues.RemoveAt(index);
        }

        public void Clear()
        {
            mKeys.Clear();
            mValues.Clear();
        }
    }
}

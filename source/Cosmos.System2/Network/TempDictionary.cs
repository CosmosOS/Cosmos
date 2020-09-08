using System;
using System.Collections.Generic;

namespace Cosmos.System.Network
{
    /// <summary>
    /// TempDictionary template class.
    /// </summary>
    /// <typeparam name="TValue">TempDictionary type name.</typeparam>
    internal class TempDictionary<TValue>
    {
        private List<UInt32> mKeys;
        private List<TValue> mValues;

        /// <summary>
        /// Get the number of elements in the list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Create new inctanse of the <see cref="TempDictionary{TValue}"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public TempDictionary()
            :this(2)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="TempDictionary{TValue}"/> class, with a specified initial size.
        /// </summary>
        /// <param name="initialSize">Initial size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if initialSize is less than 0.</exception>
        public TempDictionary(int initialSize)
        {
            this.mKeys = new List<UInt32>(initialSize);
            this.mValues = new List<TValue>(initialSize);
        }

        /// <summary>
        /// Get TempDictionary{TValue} keys array.
        /// </summary>
        public UInt32[] Keys
        {
            get
            {
                return this.mKeys.ToArray();
            }
        }

        /// <summary>
        /// Get TempDictionary{TValue} values array.
        /// </summary>
        public TValue[] Values
        {
            get
            {
                return this.mValues.ToArray();
            }
        }

        /// <summary>
        /// Get and set the element with the specified key.
        /// </summary>
        /// <param name="key">Key of an element.</param>
        /// <returns>TValue value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">(set) Thrown if no element with the specified key is found.</exception>
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

        /// <summary>
        /// Check if the key exists in the TempDictionary{TValue}.
        /// </summary>
        /// <param name="key">Key to check if exists.</param>
        /// <returns>bool value.</returns>
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

        /// <summary>
        /// Adds an object at the end of the TempDictionary{TValue}.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="val">Object value.</param>
        /// <exception cref="ArgumentException">Thrown if key already exists.</exception>
        public void Add(UInt32 key, TValue val)
        {
            if (ContainsKey(key) == true)
            {
                throw new ArgumentException("key", "Key already exists");
            }

            mKeys.Add(key);
            mValues.Add(val);
        }

        /// <summary>
        /// Removes the element with the specified key of the TempDictionary{TValue}.
        /// </summary>
        /// <param name="key">Key of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public void Remove(UInt32 key)
        {
            int idx = mKeys.IndexOf(key);
            if (idx != -1)
            {
                this.Remove(idx);
            }
        }

        /// <summary>
        /// Removes the element at the specified index of the TempDictionary{TValue}.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if no such index exists in the TempDictionary.</exception>
        public void Remove(int index)
        {
            if (index > mKeys.Count - 1)
            {
                throw new ArgumentOutOfRangeException("index", "No such index");
            }

            mKeys.RemoveAt(index);
            mValues.RemoveAt(index);
        }

        /// <summary>
        /// Removes all elements from the TempDictionary{TValue}.
        /// </summary>
        public void Clear()
        {
            mKeys.Clear();
            mValues.Clear();
        }
    }
}

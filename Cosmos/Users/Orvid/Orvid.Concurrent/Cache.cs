/*  
 Copyright 2008 The 'A Concurrent Hashtable' development team  
 (http://www.codeplex.com/CH/People/ProjectPeople.aspx)

 This library is licensed under the GNU Library General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.codeplex.com/CH/license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;
using System.Security;

#if !SILVERLIGHT
using System.Collections.Concurrent;
#endif


namespace Orvid.Concurrent.Collections
{
    class Slot
    {
        public object _Item;
        public int _GC2WhenLastUsed;
    }

    sealed class Level1CacheClass<TKey> : ConcurrentDictionary<TKey,Slot>, IMaintainable
    {
        public Level1CacheClass(IEqualityComparer<TKey> keyComparer)
            : base(keyComparer)
        {
            _GC2Count = GC.CollectionCount(2);
            MaintenanceWorker.Register(this);
        }

        int _GC2Count;

        /// <summary>
        /// Table maintenance, removes all items marked as Garbage.
        /// </summary>
        /// <returns>
        /// A boolean value indicating if the maintenance run could be performed without delay; false if there was a lock on the SyncRoot object
        /// and the maintenance run could not be performed.
        /// </returns>
        /// <remarks>
        /// This method is called regularly in sync with the garbage collector on a high-priority thread.
        /// 
        /// This override keeps track of GC.CollectionCount(2) to assess which items have been accessed recently.
        /// </remarks>
        void IMaintainable.DoMaintenance()
        {
            int newCount = GC.CollectionCount(2);
            if (newCount != _GC2Count)
            {
                _GC2Count = newCount;

                Slot dummy;

                foreach (var kvp in this)
                    if (kvp.Value._GC2WhenLastUsed - _GC2Count < -1)
                        this.TryRemove(kvp.Key, out dummy);
            }
        }

        public bool TryGetItem(TKey key, out object item)
        {
            Slot slot;

            if (base.TryGetValue(key, out slot))
            {
                item = slot._Item;
                slot._GC2WhenLastUsed = _GC2Count;
                return true;
            }

            item = null;
            return false;
        }

        public bool GetOldestItem(TKey key, ref object item)
        {
            var newSlot = new Slot() { _Item = item, _GC2WhenLastUsed = _GC2Count };
            var slot = base.GetOrAdd(key, newSlot);

            item = slot._Item;
            slot._GC2WhenLastUsed = _GC2Count;

            return object.ReferenceEquals(newSlot, slot);
        }
    }

    class ObjectComparerClass<TKey> : IEqualityComparer<object>
    {
        public IEqualityComparer<TKey> _KeyComparer;

        public new bool Equals(object x, object y)
        {
        	return x == null ? y == null : (y != null && _KeyComparer.Equals((TKey)x, (TKey)y)); 
        }

        public int GetHashCode(object obj)
        {
        	return _KeyComparer.GetHashCode((TKey)obj); 
        }
    }

    /// <summary>
    /// Cache; Retains values longer than WeakDictionary
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    /// <remarks>
    /// Use only for expensive values.
    /// </remarks>
    [Serializable]
    public sealed class Cache<TKey, TValue> : ISerializable
    {
        IEqualityComparer<TKey> _keyComparer;
        Level1CacheClass<TKey> _Level1Cache;
        WeakDictionary<object,bool, object> _Level2Cache;

        /// <summary>
        /// Constructs a new instance of <see cref="Cache{TKey,TValue}"/> using an explicit <see cref="IEqualityComparer{TKey}"/> of TKey to comparer keys.
        /// </summary>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{TKey}"/> of TKey to compare keys with.</param>
        public Cache(IEqualityComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer;
            _Level1Cache = new Level1CacheClass<TKey>(keyComparer);
            _Level2Cache = new WeakDictionary<object,bool, object>(new ObjectComparerClass<TKey> { _KeyComparer = keyComparer }, EqualityComparer<bool>.Default);
        }

        /// <summary>
        /// Constructs a new instance of <see cref="Cache{TKey,TValue}"/> with the default key comparer.
        /// </summary>
        public Cache() : this(EqualityComparer<TKey>.Default) { }

        Cache(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            _keyComparer = (IEqualityComparer<TKey>)serializationInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
            _Level1Cache = new Level1CacheClass<TKey>(_keyComparer);
            _Level2Cache = new WeakDictionary<object, bool, object>(new ObjectComparerClass<TKey> { _KeyComparer = _keyComparer }, EqualityComparer<bool>.Default);

            foreach (var kvp in (IEnumerable<KeyValuePair<TKey, TValue>>)serializationInfo.GetValue("Items", typeof(List<KeyValuePair<TKey, TValue>>)))
                this.GetOldest(kvp.Key, kvp.Value);
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Comparer", this._keyComparer);
            info.AddValue("Level2", _Level1Cache.Select(kvp => new KeyValuePair<TKey, TValue>( kvp.Key, (TValue)kvp.Value._Item ) ).ToList() );
        }

        /// <summary>
        /// Try to retrieve an item from the cache
        /// </summary>
        /// <param name="key">Key to find the item with.</param>
        /// <param name="item">Out parameter that will receive the found item.</param>
        /// <returns>A boolean value indicating if an item has been found.</returns>
        public bool TryGetItem(TKey key, out TValue item)
        {
            object storedItem;

            bool found = _Level1Cache.TryGetItem(key, out storedItem);

            if (!found)
            {
                var keyAsObject = (object)key;
                if (keyAsObject != null && _Level2Cache.TryGetValue(keyAsObject, false, out storedItem))
                {
                    found = true;
                    _Level1Cache.GetOldestItem(key, ref storedItem);
                }
            }

            if (found)
            {
                item = (TValue)storedItem;
                return true;
            }

            item = default(TValue);
            return false;
        }

        /// <summary>
        /// Tries to find an item in the cache but if it can not be found a new given item will be inserted and returned.
        /// </summary>
        /// <param name="key">The key to find an existing item or insert a new item with.</param>
        /// <param name="newItem">The new item to insert if an existing item can not be found.</param>
        /// <returns>The found item or the newly inserted item.</returns>
        public TValue GetOldest(TKey key, TValue newItem)
        {
            object item = (object)newItem;

            var keyAsObject = (object)key;
            if (keyAsObject != null)
                item = _Level2Cache.GetOrAdd(keyAsObject,false, item);                   

            _Level1Cache.GetOldestItem(key, ref item);
            

            return (TValue)item;
        }

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        public void Clear()
        {
            ((ICollection<KeyValuePair<Tuple<object,bool>,object>>)_Level2Cache).Clear();
            _Level1Cache.Clear();
        }
    }
}

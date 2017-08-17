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
using System.Collections;
using System.Runtime.Serialization;
using System.Security;

namespace Orvid.Concurrent.Collections
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class WeakKeyDictionary<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey, TValue> : DictionaryBase<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>
#if !SILVERLIGHT
    , ISerializable
#endif
        where TWeakKey1 : class
        where TWeakKey2 : class
        where TWeakKey3 : class
        where TWeakKey4 : class
        where TValue : class
    {
        sealed class InternalWeakDictionary :
            InternalWeakDictionaryStrongValueBase<
                Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, 
                Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, 
                TValue, 
                Stacktype<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>
            >
        {
            public InternalWeakDictionary(int concurrencyLevel, int capacity, KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> keyComparer)
                : base(concurrencyLevel, capacity, keyComparer)
            { 
                _comparer = keyComparer;
                MaintenanceWorker.Register(this);
            }

            public InternalWeakDictionary(KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> keyComparer)
                : base(keyComparer)
            { 
                _comparer = keyComparer;
                MaintenanceWorker.Register(this);
            }

            public KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> _comparer;

            protected override Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> FromExternalKeyToSearchKey(Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> externalKey)
            { return new SearchKey<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>().Set(externalKey, _comparer); }

            protected override Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> FromExternalKeyToStorageKey(Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> externalKey)
            { return new StorageKey<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>().Set(externalKey, _comparer); }

            protected override Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> FromStackKeyToSearchKey(Stacktype<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> externalKey)
            { return new SearchKey<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>().Set(externalKey, _comparer); }

            protected override Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> FromStackKeyToStorageKey(Stacktype<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> externalKey)
            { return new StorageKey<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>().Set(externalKey, _comparer); }

            protected override bool FromInternalKeyToExternalKey(Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> internalKey, out Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> externalKey)
            { return internalKey.Get(out externalKey); }

            protected override bool FromInternalKeyToStackKey(Key<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> internalKey, out Stacktype<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey> externalKey)
            { return internalKey.Get(out externalKey); }
        }

        readonly InternalWeakDictionary _internalDictionary;

        protected override IDictionary<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue> InternalDictionary
        { get { return _internalDictionary; } }

#if !SILVERLIGHT
        WeakKeyDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            var comparer = (KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>)serializationInfo.GetValue("Comparer", typeof(KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>));
            var items = (List<KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>>)serializationInfo.GetValue("Items", typeof(List<KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>>));
            _internalDictionary = new InternalWeakDictionary(comparer);
            _internalDictionary.InsertContents(items);
        }

        #region ISerializable Members

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Comparer", _internalDictionary._comparer);
            info.AddValue("Items", _internalDictionary.GetContents());
        }
        #endregion
#endif

        public WeakKeyDictionary()
            : this(EqualityComparer<TWeakKey1>.Default, EqualityComparer<TWeakKey2>.Default, EqualityComparer<TWeakKey3>.Default, EqualityComparer<TWeakKey4>.Default, EqualityComparer<TStrongKey>.Default)
        {}

        public WeakKeyDictionary(IEqualityComparer<TWeakKey1> weakKey1Comparer, IEqualityComparer<TWeakKey2> weakKey2Comparer, IEqualityComparer<TWeakKey3> weakKey3Comparer, IEqualityComparer<TWeakKey4> weakKey4Comparer, IEqualityComparer<TStrongKey> strongKeyComparer)
            : this(Enumerable.Empty<KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>>(), weakKey1Comparer, weakKey2Comparer, weakKey3Comparer, weakKey4Comparer, strongKeyComparer)
        {}

        public WeakKeyDictionary(IEnumerable<KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>> collection)
            : this(collection, EqualityComparer<TWeakKey1>.Default, EqualityComparer<TWeakKey2>.Default, EqualityComparer<TWeakKey3>.Default, EqualityComparer<TWeakKey4>.Default, EqualityComparer<TStrongKey>.Default)
        {}

        public WeakKeyDictionary(IEnumerable<KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>> collection, IEqualityComparer<TWeakKey1> weakKey1Comparer, IEqualityComparer<TWeakKey2> weakKey2Comparer, IEqualityComparer<TWeakKey3> weakKey3Comparer, IEqualityComparer<TWeakKey4> weakKey4Comparer, IEqualityComparer<TStrongKey> strongKeyComparer)
        {
            _internalDictionary = 
                new InternalWeakDictionary(
                    new KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>(weakKey1Comparer, weakKey2Comparer, weakKey3Comparer, weakKey4Comparer, strongKeyComparer)
                )
            ;

            _internalDictionary.InsertContents(collection);
        }

        public WeakKeyDictionary(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, EqualityComparer<TWeakKey1>.Default, EqualityComparer<TWeakKey2>.Default, EqualityComparer<TWeakKey3>.Default, EqualityComparer<TWeakKey4>.Default, EqualityComparer<TStrongKey>.Default)
        {}

        public WeakKeyDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>> collection, IEqualityComparer<TWeakKey1> weakKey1Comparer, IEqualityComparer<TWeakKey2> weakKey2Comparer, IEqualityComparer<TWeakKey3> weakKey3Comparer, IEqualityComparer<TWeakKey4> weakKey4Comparer, IEqualityComparer<TStrongKey> strongKeyComparer)
        {
            var contentsList = collection.ToList();
            _internalDictionary =
                new InternalWeakDictionary(
                    concurrencyLevel,
                    contentsList.Count,
                    new KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>(weakKey1Comparer, weakKey2Comparer, weakKey3Comparer, weakKey4Comparer, strongKeyComparer)
                )
            ;
            _internalDictionary.InsertContents(contentsList);
        }

        public WeakKeyDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TWeakKey1> weakKey1Comparer, IEqualityComparer<TWeakKey2> weakKey2Comparer, IEqualityComparer<TWeakKey3> weakKey3Comparer, IEqualityComparer<TWeakKey4> weakKey4Comparer, IEqualityComparer<TStrongKey> strongKeyComparer)
        {
            _internalDictionary =
                new InternalWeakDictionary(
                    concurrencyLevel,
                    capacity,
                    new KeyComparer<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>(weakKey1Comparer, weakKey2Comparer, weakKey3Comparer, weakKey4Comparer, strongKeyComparer)
                )
            ;
        }


        public bool ContainsKey(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey)
        { return _internalDictionary.ContainsKey(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey)); }

        public bool TryGetValue(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, out TValue value)
        { return _internalDictionary.TryGetValue(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), out value); }

        public TValue this[TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey]
        {
            get { return _internalDictionary.GetItem(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey)); }
            set { _internalDictionary.SetItem(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), value); }
        }

        public bool IsEmpty
        { get { return _internalDictionary.IsEmpty; } }

        public TValue AddOrUpdate(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, Func<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey, TValue> addValueFactory, Func<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey, TValue, TValue> updateValueFactory)
        {
            if (null == addValueFactory)
                throw new ArgumentNullException("addValueFactory");

            if (null == updateValueFactory)
                throw new ArgumentNullException("updateValueFactory");

            return
                _internalDictionary.AddOrUpdate(
                    Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), 
                    hr => addValueFactory(hr.Item1, hr.Item2, hr.Item3, hr.Item4, hr.Item5), 
                    (hr, v) => updateValueFactory(hr.Item1, hr.Item2, hr.Item3, hr.Item4, hr.Item5, v)
                )
            ;
        }

        public TValue AddOrUpdate(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, TValue addValue, Func<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey, TValue, TValue> updateValueFactory)
        {
            if (null == updateValueFactory)
                throw new ArgumentNullException("updateValueFactory");

            return
                _internalDictionary.AddOrUpdate(
                    Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey),
                    addValue,
                    (hr, v) => updateValueFactory(hr.Item1, hr.Item2, hr.Item3, hr.Item4, hr.Item5, v)
                )
            ;
        }

        public TValue GetOrAdd(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, TValue value)
        { return _internalDictionary.GetOrAdd(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), value); }

        public TValue GetOrAdd(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, Func<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey, TValue> valueFactory)
        {
            if (null == valueFactory)
                throw new ArgumentNullException("valueFactory");

            return _internalDictionary.GetOrAdd(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), hr => valueFactory(hr.Item1, hr.Item2, hr.Item3, hr.Item4, hr.Item5));
        }
        
        public KeyValuePair<Tuple<TWeakKey1, TWeakKey2, TWeakKey3, TWeakKey4, TStrongKey>, TValue>[] ToArray()
        { return _internalDictionary.ToArray(); }

        public bool TryAdd(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, TValue value)
        { return _internalDictionary.TryAdd(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), value); }

        public bool TryRemove(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, out TValue value)
        { return _internalDictionary.TryRemove(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), out value); }

        public bool TryUpdate(TWeakKey1 weakKey1, TWeakKey2 weakKey2, TWeakKey3 weakKey3, TWeakKey4 weakKey4, TStrongKey strongKey, TValue newValue, TValue comparisonValue)
        { return _internalDictionary.TryUpdate(Stacktype.Create(weakKey1, weakKey2, weakKey3, weakKey4, strongKey), newValue, comparisonValue ); }
    }
}

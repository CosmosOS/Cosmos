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
    public class WeakValueDictionary<TStrongKey, TValue> : DictionaryBase<TStrongKey, TValue>
#if !SILVERLIGHT
    , ISerializable
#endif
        where TValue : class
    {
        class InternalWeakDictionary :
            InternalWeakDictionaryWeakValueBase<
                StrongKey<TStrongKey>, 
                TStrongKey, 
                TValue,
                Stacktype<TStrongKey>
            >
        {
            public InternalWeakDictionary(int concurrencyLevel, int capacity, KeyComparer<TStrongKey> keyComparer)
                : base(concurrencyLevel, capacity, keyComparer)
            { _comparer = keyComparer; }

            public InternalWeakDictionary(KeyComparer<TStrongKey> keyComparer)
                : base(keyComparer)
            { _comparer = keyComparer; }

            public KeyComparer<TStrongKey> _comparer;

            protected override StrongKey<TStrongKey> FromExternalKeyToSearchKey(TStrongKey externalKey)
            { return new StrongKey<TStrongKey>() { _element = externalKey }; }

            protected override StrongKey<TStrongKey> FromExternalKeyToStorageKey(TStrongKey externalKey)
            { return new StrongKey<TStrongKey>() { _element = externalKey }; }

            protected override StrongKey<TStrongKey> FromStackKeyToSearchKey(Stacktype<TStrongKey> externalKey)
            { return new StrongKey<TStrongKey>() { _element = externalKey.Item1 }; }

            protected override StrongKey<TStrongKey> FromStackKeyToStorageKey(Stacktype<TStrongKey> externalKey)
            { return new StrongKey<TStrongKey>() { _element = externalKey.Item1 }; }

            protected override bool FromInternalKeyToExternalKey(StrongKey<TStrongKey> internalKey, out TStrongKey externalKey)
            {
                externalKey = internalKey._element;
                return true; 
            }

            protected override bool FromInternalKeyToStackKey(StrongKey<TStrongKey> internalKey, out Stacktype<TStrongKey> externalKey)
            {
                externalKey = Stacktype.Create(internalKey._element);
                return true;
            }
        }

        readonly InternalWeakDictionary _internalDictionary;

        protected override IDictionary<TStrongKey, TValue> InternalDictionary
        { get { return _internalDictionary; } }

#if !SILVERLIGHT
        WeakValueDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            var comparer = (KeyComparer<TStrongKey>)serializationInfo.GetValue("Comparer", typeof(KeyComparer<TStrongKey>));
            var items = (List<KeyValuePair<TStrongKey, TValue>>)serializationInfo.GetValue("Items", typeof(List<KeyValuePair<TStrongKey, TValue>>));
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

        public WeakValueDictionary()
            : this(EqualityComparer<TStrongKey>.Default)
        {}

        public WeakValueDictionary(IEqualityComparer<TStrongKey> strongKeyComparer)
            : this(Enumerable.Empty<KeyValuePair<TStrongKey,TValue>>(), strongKeyComparer)
        {}

        public WeakValueDictionary(IEnumerable<KeyValuePair<TStrongKey,TValue>> collection)
            : this(collection, EqualityComparer<TStrongKey>.Default)
        {}

        public WeakValueDictionary(IEnumerable<KeyValuePair<TStrongKey, TValue>> collection, IEqualityComparer<TStrongKey> strongKeyComparer)
        {
            _internalDictionary = 
                new InternalWeakDictionary(
                    new KeyComparer<TStrongKey>(strongKeyComparer)
                )
            ;

            _internalDictionary.InsertContents(collection);
        }

        public WeakValueDictionary(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, EqualityComparer<TStrongKey>.Default)
        {}

        public WeakValueDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TStrongKey, TValue>> collection, IEqualityComparer<TStrongKey> strongKeyComparer)
        {
            var contentsList = collection.ToList();
            _internalDictionary =
                new InternalWeakDictionary(
                    concurrencyLevel,
                    contentsList.Count,
                    new KeyComparer<TStrongKey>(strongKeyComparer)
                )
            ;
            _internalDictionary.InsertContents(contentsList);
        }

        public WeakValueDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TStrongKey> strongKeyComparer)
        {
            _internalDictionary =
                new InternalWeakDictionary(
                    concurrencyLevel,
                    capacity,
                    new KeyComparer<TStrongKey>(strongKeyComparer)
                )
            ;
        }


        public bool ContainsKey(TStrongKey strongKey)
        { return _internalDictionary.ContainsKey(Stacktype.Create(strongKey)); }

        public bool TryGetValue(TStrongKey strongKey, out TValue value)
        { return _internalDictionary.TryGetValue(Stacktype.Create(strongKey), out value); }

        public TValue this[TStrongKey strongKey]
        {
            get { return _internalDictionary.GetItem(Stacktype.Create(strongKey)); }
            set { _internalDictionary.SetItem(Stacktype.Create(strongKey), value); }
        }

        public bool IsEmpty
        { get { return _internalDictionary.IsEmpty; } }

        public TValue AddOrUpdate(TStrongKey strongKey, Func<TStrongKey, TValue> addValueFactory, Func<TStrongKey, TValue, TValue> updateValueFactory)
        {
            return
                _internalDictionary.AddOrUpdate(
                    Stacktype.Create(strongKey), 
                    ht => addValueFactory(ht.Item1), 
                    (ht, v) => updateValueFactory(ht.Item1, v)
                )
            ;
        }

        public TValue AddOrUpdate(TStrongKey strongKey, TValue addValue, Func<TStrongKey, TValue, TValue> updateValueFactory)
        {
            return
                _internalDictionary.AddOrUpdate(
                    Stacktype.Create(strongKey),
                    addValue,
                    (ht, v) => updateValueFactory(ht.Item1, v)
                )
            ;
        }

        public TValue GetOrAdd(TStrongKey strongKey, TValue value)
        { return _internalDictionary.GetOrAdd(Stacktype.Create(strongKey), value); }

        public TValue GetOrAdd(TStrongKey strongKey, Func<TStrongKey, TValue> valueFactory)
        {
            if (null == valueFactory)
                throw new ArgumentNullException("valueFactory");

            return _internalDictionary.GetOrAdd(Stacktype.Create(strongKey), ht => valueFactory(ht.Item1)); 
        }
        
        public KeyValuePair<TStrongKey, TValue>[] ToArray()
        { return _internalDictionary.ToArray(); }

        public bool TryAdd(TStrongKey strongKey, TValue value)
        { return _internalDictionary.TryAdd(Stacktype.Create(strongKey), value); }

        public bool TryRemove(TStrongKey strongKey, out TValue value)
        { return _internalDictionary.TryRemove(Stacktype.Create(strongKey), out value); }

        public bool TryUpdate(TStrongKey strongKey, TValue newValue, TValue comparisonValue)
        { return _internalDictionary.TryUpdate(Stacktype.Create(strongKey), newValue, comparisonValue); }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Collections;

//namespace UtilityClasses
//{
//    public class SimpleHashSet<T> : IEnumerable<T>
//    {
//        private class DefaultHashcodeProvider : IEqualityComparer<T>
//        {
//            public bool Equals(T x, T y)
//            {
//                throw new NotImplementedException();
//            }

//            public int GetHashCode(T obj)
//            {
//                return obj.GetHashCode();
//            }

//            public static readonly DefaultHashcodeProvider Instance = new DefaultHashcodeProvider();
//        }

//        private Dictionary<int, T> mItems = new Dictionary<int, T>();
//        private IEqualityComparer<T> mHashCodeProvider;

//        public SimpleHashSet()
//            : this(DefaultHashcodeProvider.Instance)
//        {
//        }

//        public SimpleHashSet(IEqualityComparer<T> aHashCodeProvider)
//        {
//            mHashCodeProvider = aHashCodeProvider;
//        }

//        public bool Contains(T aItem)
//        {
//            if (aItem == null)
//            {
//                throw new ArgumentNullException("aItem");
//            }
//            return mItems.ContainsKey(mHashCodeProvider.GetHashCode(aItem));
//        }

//        public void Add(int aHashCode, T aItem)
//        {
//            if (aItem == null)
//            {
//                throw new ArgumentNullException("aItem");
//            }
//            mItems.Add(aHashCode, aItem);
//        }

//        public void Add(T aItem)
//        {
//            if (aItem == null)
//            {
//                throw new ArgumentNullException("aItem");
//            }
//            mItems.Add(mHashCodeProvider.GetHashCode(aItem), aItem);
//        }

//        public int Count
//        {
//            get
//            {
//                return mItems.Count;
//            }
//        }

//        public IEnumerator<T> GetEnumerator()
//        {
//            return (from item in mItems
//                    select item.Value).GetEnumerator();
//        }

//        public T GetItemInList(T aItem)
//        {
//            if (aItem == null)
//            {
//                throw new ArgumentNullException("aItem");
//            }
//            T xResult;
//            if (mItems.TryGetValue(mHashCodeProvider.GetHashCode(aItem), out xResult))
//            {
//                return xResult;
//            }
//            else
//            {
//                return aItem;
//            }
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return (from item in mItems
//                    select item.Value).GetEnumerator();
//        }

//        public void Clear()
//        {
//            mItems.Clear();
//        }

//        public bool TryGetValue(int aHashValue, out T aResult)
//        {
//            return mItems.TryGetValue(aHashValue, out aResult);

//        }
//    }
//}
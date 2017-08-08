using System;

namespace Cosmos.Debug.Symbols
{
    public class CacheHelper<TKey, TValue>
        where TKey: IEquatable<TKey>
    {
        public CacheHelper(Func<TKey, TValue> getValueFunc)
        {
            if (getValueFunc == null)
            {
                throw new ArgumentNullException("getValueFunc");
            }
            mGetValueFunc = getValueFunc;
        }

        private readonly Func<TKey, TValue> mGetValueFunc;
        private TValue mCachedValue;
        private TKey mCachedKey;
        private bool mHasCachedValue = false;

        public TValue GetValue(TKey key)
        {
            if (mHasCachedValue && mCachedKey.Equals(key))
            {
                return mCachedValue;
            }

            mCachedValue = mGetValueFunc(key);
            mCachedKey = key;
            mHasCachedValue = true;
            return mCachedValue;
        }
    }
}

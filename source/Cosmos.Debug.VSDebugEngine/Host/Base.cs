using System;
using System.Collections.Specialized;

namespace Cosmos.Debug.VSDebugEngine.Host
{
    public abstract class Base
    {
        protected NameValueCollection mParams;
        protected bool mUseGDB;

        public EventHandler OnShutDown;

        public Base(NameValueCollection aParams, bool aUseGDB)
        {
            mParams = aParams;
            mUseGDB = aUseGDB;
        }

        public abstract void Start();

        public abstract void Stop();
    }
}
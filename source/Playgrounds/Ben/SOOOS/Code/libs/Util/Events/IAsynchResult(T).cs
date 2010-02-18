using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Util.API
{
    public class IAsyncResult<T>
       : IAsyncResult
       where T : class, new()
    {
        private T _myAsyncState;
        private object _asyncState;
        private volatile bool _isCompleted = false;
        private volatile bool _completedSynchronously = false;
        private object _syncroot = new object();
        private Exception _asyncException = null;
        private readonly AsyncCallback _callback;
        private LazyInit<ManualResetEvent> _waitEvent = new LazyInit<ManualResetEvent>(
                delegate { return new ManualResetEvent(false); });

        public IAsyncResult()
        {
            _myAsyncState = new T();
            _callback = null;
        }

        public IAsyncResult(AsyncCallback callback)
        {
            _callback = callback;
            _myAsyncState = new T();
        }

        public IAsyncResult(AsyncCallback callback, object asyncState)
        {
            _callback = callback;
            _myAsyncState = new T();
            _asyncState = asyncState;
        }

        public IAsyncResult(T myAsyncState, AsyncCallback callback)
        {
            _myAsyncState = myAsyncState;
            _callback = callback;
        }

        public IAsyncResult(T myAsyncState, AsyncCallback callback, object asyncState)
        {
            _myAsyncState = myAsyncState;
            _callback = callback;
            _asyncState = asyncState;
        }

        public Exception AsyncException
        {
            get { return _asyncException; }
            protected set { _asyncException = value; }
        }

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public T MyAsyncState
        {
            get { return _myAsyncState; }
            protected set { _myAsyncState = value; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return _waitEvent.Value; }
        }

        public bool CompletedSynchronously
        {
            get { return _completedSynchronously; }
            protected set { _completedSynchronously = value; }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        public virtual void SetComplete(Exception ex)
        {
            _asyncException = ex;
            SetComplete();
        }

        public virtual void SetComplete(bool completedSynchronously)
        {
            lock (_syncroot)
            {
                if (_isCompleted == true)
                    throw new InvalidOperationException("AsyncResult is already complete");

                _isCompleted = true;

                _completedSynchronously = completedSynchronously;
            }

            _waitEvent.Value.Set();
            if (null != _callback)
                _callback(this);
        }

        public virtual void SetComplete()
        {
            SetComplete(false);
        }

        object IAsyncResult.AsyncState
        {
            get { return _asyncState; }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { return AsyncWaitHandle; }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { return CompletedSynchronously; }
        }

        bool IAsyncResult.IsCompleted
        {
            get { return IsCompleted; }
        }
    }
}

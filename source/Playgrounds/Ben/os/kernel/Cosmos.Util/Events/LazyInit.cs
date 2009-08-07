using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Kernel.API
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal delegate T Initializer<T>();

    /// <summary>
    /// A Thread Safe Lazy Initializer. This struct might invoke your delegate 
    /// more than once, due to races, but it will ensure only one value wins.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This structure was written by Joe Duffy, and descirbed in his Blog at: 
    /// http://www.bluebytesoftware.com/blog/PermaLink,guid,a2787ef6-ade6-4818-846a-2b2fd8bb752b.aspx
    /// </remarks>
    internal struct LazyInit<T> where T : class
    {
        private Initializer<T> m_init;
        private T m_value;

        public LazyInit(Initializer<T> init)
        {
            m_init = init;
            m_value = null;
        }

        public T Value
        {
            get
            {
                if (m_value == null)
                {
                    T newValue = m_init();
                    if (Interlocked.CompareExchange(ref m_value, newValue, null) != null && newValue is IDisposable)
                    {
                        ((IDisposable)newValue).Dispose();
                    }
                }

                return m_value;
            }
        }
    }

    /// <summary>
    /// A Thread Safe Lazy Initializer.  LazyInitOnlyOnce does the extra work to ensure the initialization 
    /// routine only gets called once, though at a slightly higher cost: we might need to block a thread, 
    /// which means allocating a Win32 event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This structure was written by Joe Duffy, and descirbed in his Blog at: 
    /// http://www.bluebytesoftware.com/blog/PermaLink,guid,a2787ef6-ade6-4818-846a-2b2fd8bb752b.aspx
    /// </remarks>
    internal struct LazyInitOnlyOnce<T> where T : class
    {
        private Initializer<T> m_init;
        private T m_value;
        private object m_syncLock;

        public LazyInitOnlyOnce(Initializer<T> init)
        {
            m_init = init;
            m_value = null;
            m_syncLock = null;
        }

        public T Value
        {
            get
            {
                if (m_value == null)
                {
                    object newSyncLock = new object();
                    object syncLockToUse = Interlocked.CompareExchange(ref m_syncLock, newSyncLock, null);
                    if (syncLockToUse == null)
                        syncLockToUse = newSyncLock;

                    lock (syncLockToUse)
                    {
                        if (m_value == null)
                            m_value = m_init();

                        // It may also not be obvious why I null out the m_syncLock field before 
                        // exiting.  If we don't, the object will remain live as long as the 
                        // lazily initialized variable remains live.  Sadly this object might 
                        // hold on to precious resources: if there was contention, it may have 
                        // allocated a Win32 event internally.  We want the object to be GC'd 
                        // as soon as possible so that the event can be released.
                        m_syncLock = null;
                    }
                }

                return m_value;
            }
        }
    }
}

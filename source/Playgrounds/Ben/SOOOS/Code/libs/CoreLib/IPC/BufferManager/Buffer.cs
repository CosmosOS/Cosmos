using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CoreLib.IPC
{
    /// <summary>
    /// Note we cant use arrays ...since these objects are fixed. 
    /// 
    /// Note buffers can be ulong etc...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Immutable]
    [Trusted]
    [RefCounted]
    public struct Buffer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : struct
    {
        //internal so can only be created by Manager
        internal Buffer(UIntPtr start, int count)
        {

        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }




}

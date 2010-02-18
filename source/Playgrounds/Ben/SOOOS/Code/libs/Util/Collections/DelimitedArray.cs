using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util.Collections
{
    /// <summary>
    ///  Better but heavier version of ArraySegment
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelimitedArray<T>  :IList<T>
    { 
        private int _offset; 
        private T[] _array; 
        private int _count; 
        
        public DelimitedArray(T[] array, int offset, int count) 
        { 
            this._array = array; this._offset = offset; this._count = count; 
        } 
        

        
        public int Count { get { return this._count; } } 
        public T this[int index] 
        { 
            get 
            { 
                int idx = this._offset + index; 
                if (idx > this.Count - 1 || idx < 0) 
                { 
                    throw new IndexOutOfRangeException("Index '" + idx + "' was outside the bounds of the array."); 
                } 
                return this._array[idx]; 
            }
            set
            {
                throw new NotSupportedException("Its read only"); 
            }
        } 
        
        public IEnumerator<T> GetEnumerator() 
        { 
            for (int i = this._offset; i < this._offset + this.Count; i++) 
            { 
                yield return this._array[i]; 
            } 
        }


        public int IndexOf(T item)
        {
            for (int i = this._offset; i < this._offset + this.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals (this._array[i], item))
                    return i; 
            }
            return -1; 
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
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
            if (IndexOf(item) == -1)
                return false; 
            
            return true; 
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_array, _offset, array, arrayIndex, _count);
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); 
        }
    }
}

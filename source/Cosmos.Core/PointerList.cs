using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core
{
    /// <summary>
    /// PointerList class. Used for making list of pointers.
    /// </summary>
    /// <typeparam name="T">Non-pointer type of pointer.</typeparam>
    public unsafe class PointerList<T> : IEnumerable where T : unmanaged
    {
        private T*[] values;
        private int index = 0;
        private int maxLength;
        private int enumerator_index = -1;

        /// <summary>
        /// Count of pointers.
        /// </summary>
        public int Count
        {
            get
            {
                return index;
            }
        }

        private void expand()
        {
            T*[] newArray = new T*[maxLength + 1];

            if (maxLength > 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    newArray[i] = values[i];
                }
            }

            values = newArray;
        }

        public PointerList(int maxLength = 0)
        {
            this.maxLength = maxLength;

            if (maxLength > 0)
            {
                values = new T*[maxLength];
            }
        }

        public void Add(T* value)
        {
            if (index + 1 >= maxLength)
            {
                expand();
            }

            values[index++] = value;
        }

        public T* this[int key]
        {
            get
            {
                return values[key];
            }
        }


        public IEnumerator GetEnumerator()
        {
            return new PointerListEnum<T>(this);
        }
    }

    /// <summary>
    /// Enumerator for PointerList class.
    /// </summary>
    public unsafe class PointerListEnum<T> : IEnumerator where T : unmanaged
    {
        private int index = -1;
        private PointerList<T> list;

        public object Current => *list[index];

        public PointerListEnum(PointerList<T> list)
        {
            this.list = list;
        }

        bool IEnumerator.MoveNext()
        {
            index++;
            return index < list.Count;
        }

        void IEnumerator.Reset()
        {
            index = -1;
        }
    }
}

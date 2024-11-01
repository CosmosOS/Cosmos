using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core
{
    /// <summary>
    /// PointerList class. Used for making list of pointers.
    /// </summary>
    /// <typeparam name="T">Non-pointer type of pointer.</typeparam>
    public unsafe class PointerList<T> where T : unmanaged
    {
        private T*[] values;
        private int index = 0;
        private int maxLength;

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
    }
}

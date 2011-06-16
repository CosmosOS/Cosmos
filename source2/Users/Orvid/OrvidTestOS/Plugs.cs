using System;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using CPUAll = Cosmos.Compiler.Assembler;

namespace GuessKernel
{
    public class Queue<T>
    {
        T[] _array;
        int _head;
        int _tail;
        int _size;
        int _version;

        public Queue()
        {
            _array = new T[0];
        }

        public Queue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");

            _array = new T[capacity];
        }

        public void Clear()
        {
            Array.Clear(_array, 0, _array.Length);

            _head = _tail = _size = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    T t = _array[i];
                    if (t == null)
                        return true;
                }
            }
            else
            {
                for (int i = 0; i < this.Count; i++)
                {
                    T t = _array[i];
                    if (item.Equals(t))
                        return true;
                }
            }

            return false;
        }

        public T Dequeue()
        {
            T ret = Peek();

            // clear stuff out to make the GC happy
            _array[_head] = default(T);

            if (++_head == _array.Length)
                _head = 0;
            _size--;
            _version++;

            return ret;
        }

        public T Peek()
        {
            if (_size == 0)
                throw new InvalidOperationException();

            return _array[_head];
        }

        public void Enqueue(T item)
        {
            if (_size == _array.Length || _tail == _array.Length)
                SetCapacity(Math.Max(Math.Max(_size, _tail) * 2, 4));

            _array[_tail] = item;

            if (++_tail == _array.Length)
                _tail = 0;

            _size++;
            _version++;
        }

        public T[] ToArray()
        {
            T[] t = new T[_size];
            CopyTo(t, 0);
            return t;
        }

        private void CopyTo(T[] t, int p)
        {
            throw new NotImplementedException();
        }

        public void TrimExcess()
        {
            if (_size < _array.Length * 0.9)
                SetCapacity(_size);
        }

        void SetCapacity(int new_size)
        {
            if (new_size == _array.Length)
                return;

            if (new_size < _size)
                throw new InvalidOperationException("shouldn't happen");

            T[] new_data = new T[new_size];
            if (_size > 0)
                CopyTo(new_data, 0);

            _array = new_data;
            _tail = _size;
            _head = 0;
            _version++;
        }

        public int Count
        {
            get { return _size; }
        }
    }


    [Plug(Target = typeof(global::System.Environment))]
    class EnvironmentImpl
    {
        public static int TickCount
        {
            get
            {
                return GuessOS.Tick;
            }
        }
    }
}

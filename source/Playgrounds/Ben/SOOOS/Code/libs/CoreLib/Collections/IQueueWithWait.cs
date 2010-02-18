using System;
namespace CoreLib.Collections
{
    public interface IQueueWithWait<T>
    {
        void Dequeue(ref T outVal);
        void Enqueue(ref T value);
    }
}

using System;
namespace CoreLib.Collections
{
    public interface IQueue<T>
    {
        bool Dequeue(out T item);
        bool Enqueue(T item);
        bool IsEmpty { get; }
        bool IsFull { get; } 
    }


    public interface IWaitQueue<T>
    {
        void Dequeue(out T item);
        void Enqueue(T item);
      }
}

using System;
using System.Threading;

namespace CoreLib.Collections
{
    
    //public class EmptyException : Exception { }

    public class Node<T> where T : class
    {
        public T val;
        public Node<T> next;
        public Node(T val)
        {
            this.val = val;
            this.next = null;
        }
    }



    // note no count as that would need a lock..


    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 


    // Nodes make it not so efficient...we need to recycle them or make the queue fixed memory and use structs.
    public sealed class LockFreeQueue<T> where T : class
    {
        public Node<T> head, tail;
        public LockFreeQueue()
        {
            Node<T> node = new Node<T>(null);
            head = tail = node;
        }

        // add to rear by walking the queue
        public void Enqueue(T x)
        {
            Node<T> node = new Node<T>(x);
            while (true)
            {
                Node<T> last = tail;
                Node<T> next = last.next;
                if (last == tail)
                {
                    if (next == null)
                    {
                        if (next == Interlocked.CompareExchange(ref last.next, node, next))
                        {
                            Interlocked.CompareExchange(ref tail, node, last);
                            return;
                        }
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref tail, next, last);
                    }
                }
            }
        }


        // O(1) 
        public T Dequeue()
        {
            while (true)
            {
                Node<T> first = head;
                Node<T> last = tail;
                Node<T> next = first.next;
                if (first == head)
                {
                    if (first == last)
                    {
                        if (next == null)
                            return null;//  throw new EmptyException();
                        Interlocked.CompareExchange(ref tail, next, last);
                    }
                    else
                    {
                        T val = next.val;
                        if (first == Interlocked.CompareExchange(ref head, next, first))
                            return val;
                    }
                }
            }
        }
    } //LockFreeQueue
} 


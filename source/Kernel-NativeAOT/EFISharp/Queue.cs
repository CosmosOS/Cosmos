namespace EfiSharp
{
    //System.Collections.Queue requires too many extra features so using this for now
    //Adapted from https://www.geeksforgeeks.org/queue-set-1introduction-and-array-implementation/ to replace list with pointer array and be kind of generic.
    //This can store any type that can easily be converted to and from a pointer which I think is limited to primitive types not including string since there
    //is currently no method to convert char* arrays back to string.
    //TODO Make generic now that arrays are supported
    //TODO Replace with circular deque for Console.Read
    public class Queue
    {
        //private IntPtr* queue;
        private readonly char[] _queue;
        private int _front;
        private int _rear;
        private readonly int _max;

        public Queue(int size)
        {
            //IntPtr* newQueue = stackalloc IntPtr[size];
            //char* newQueue = stackalloc char[size];
            //queue = newQueue;
            _queue = new char[size];
            _front = 0;
            _rear = -1;
            _max = size;
        }

        // Function to add an item to the queue. 
        // It changes rear and size 
        //public void Enqueue(IntPtr item)
        public void Enqueue(char item)
        {
            if (_rear != _max - 1)
            {
                _queue[++_rear] = item;
            }
        }

        // Function to remove an item from queue. 
        // It changes front and size 
        //public IntPtr Dequeue()
        public char Dequeue()
        {
            //return front == rear + 1 ? IntPtr.Zero : queue[front++];
            return _front == _rear + 1 ? '\0' : _queue[_front++];
            //return '\0';
        }

        public bool IsEmpty()
        {
            return _front == _rear + 1;
        }
    }
}
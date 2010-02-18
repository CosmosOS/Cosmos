using System;
using CoreLib.Test;
using CoreLib.Collections;
using System.Diagnostics;


namespace ChessTest
{

    public class ChessTest
    {
        static public void Main(string[] args)
        {

            //for (uint i = 1; i < 100; i++)
            //   Console.WriteLine( new CircularFifoQueueTest().ThreadTest(i));

            PerfTest1(100 , 100000000);
         //   PerfTest2(30, 1000000);
            PerfTest3(100, 100000000);
     
            Console.ReadLine(); 
        }

        // For    PerfTest1(100 , 100000);
        //    PerfTest2(100, 100000);

        //Test 1 29ms
        //Test 2 2097ms



        //For 100M
        // 1second form Test , 2 second for test 3. 
        // using thsi queue for IPC would mean 30 cycles for a send and receive ( on 2 threads) 

        static public void PerfTest1(uint capacity , uint itemCount)
        {

            CircularFifoQueue<uint> target = new CircularFifoQueue<uint>(100);
            
            var watch = new Stopwatch();

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            int count = 0;
            uint inval = 3;

            var child = new System.Threading.Thread(
                o =>
                {
                    var queue = (o as CircularFifoQueue<uint>);


                    while (target.IsEmpty == false)
                    {
                        uint outval = 0;
                        if (target.Dequeue(ref outval))
                            count++;
                    }


                });
            watch.Start(); 
            child.Start(target);

            for (int i = 0; i < itemCount; i++)
            {
                target.Enqueue(ref  inval);
            }
            child.Join();
            watch.Stop();

            Console.WriteLine("Test 1 " + watch.ElapsedMilliseconds + "ms"); 
                

           
        }

        static public void PerfTest2(uint capacity, uint itemCount)
        {

            var target = new CircularFifoQueueWithWait<uint>(100);

            var watch = new Stopwatch();

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            int count = 0;
            uint inval = 3;

            var child = new System.Threading.Thread(
                o =>
                {
                    var queue = (o as CircularFifoQueue<uint>);


                  //  while (target.IsEmpty() == false)
                   // {
                        uint outval = 0;

                        for (int i = 0; i < itemCount; i++)
                        {
                            target.Dequeue(ref outval); //blocking
                            count++;
                        }
                  //  }


                });
            watch.Start();
            child.Start(target);

            for (int i = 0; i < itemCount; i++)
            {
                target.Enqueue(ref  inval);
            }
            child.Join();
            watch.Stop();

            Console.WriteLine("Test 2 " + watch.ElapsedMilliseconds + "ms");



        }


        static public void PerfTest3(uint capacity, uint itemCount)
        {

            SafeCircularFifoQueue<uint> target = new SafeCircularFifoQueue<uint>(100);

            var watch = new Stopwatch();

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            int count = 0;
            uint inval = 3;

            var child = new System.Threading.Thread(
                o =>
                {
                    var queue = (o as CircularFifoQueue<uint>);


                    while (target.IsEmpty == false)
                    {
                        uint outval = 0;
                        if (target.Dequeue(ref outval))
                            count++;
                    }


                });
            watch.Start();
            child.Start(target);

            for (int i = 0; i < itemCount; i++)
            {
                target.Enqueue(ref  inval);
            }
            child.Join();
            watch.Stop();

            Console.WriteLine("Test 1 " + watch.ElapsedMilliseconds + "ms");



        }


        static public void PerfTest4(uint capacity, uint itemCount)
        {

            SafeCircularFifoQueue<uint> target = new SafeCircularFifoQueue<uint>(100);

            var watch = new Stopwatch();

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            int count = 0;
            uint inval = 3;

            var child = new System.Threading.Thread(
                o =>
                {
                    var queue = (o as CircularFifoQueue<uint>);


                    while (target.IsEmpty == false)
                    {
                        uint outval = 0;
                        if (target.Dequeue(ref outval))
                            count++;
                    }


                });
            watch.Start();
            child.Start(target);

            for (int i = 0; i < itemCount; i++)
            {
                target.Enqueue(ref  inval);
            }
            child.Join();
            watch.Stop();

            Console.WriteLine("Test 1 " + watch.ElapsedMilliseconds + "ms");



        }

       
    }

}
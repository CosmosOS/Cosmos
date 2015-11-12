using System;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.MultidimensionalArrays
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. This is multidimensional test.");
        }
        void For(int[,] x)
       {
        	for (int i = 0; i < 10; i++)
                for (int o = 0; o < 10; o++)
                    x[i,o] = i+1 * 10 + o;
       }
        protected override void Run()
        {
            int[,] numbers = new int[10, 10];
            For(numbers);
            Assert.AreEqual(10, numbers[0,0], "Wrong:Multdimensional arrays doesnt work!");
            TestController.Completed();
        }
    }
}

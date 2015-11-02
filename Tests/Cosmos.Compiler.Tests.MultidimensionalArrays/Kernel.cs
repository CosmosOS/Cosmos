using System;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.MultidimensionalArrays
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. This is multidimensional test.");
        }

        protected override void Run()
        {
            int[,] numbers = new int[10, 10];
            for (int i = 0; i < 10; i++)
                for (int o = 0; o < 10; o++)
                    numbers[i, o] = i * 10 + o;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine();
                for (int o = 0; o < 10; o++)
                    Console.Write(numbers[i, o]);
            }
        }
    }
}

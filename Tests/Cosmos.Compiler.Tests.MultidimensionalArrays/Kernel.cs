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
        void For(int[,] x, bool Getin)
       {
	for (int i = 0; i < 10; i++)
                for (int o = 0; o < 10; o++)
                {
                      if(Getin=true)
                      {
                            x[i,o] = i * 10 + o;
                      }
                      else if(Getin=false)
                      {
                            Console.Write(x[i,o]+" ");
                      }
                }
       }
        protected override void Run()
        {
            int[,] numbers = new int[10, 10];
            For(numbers,true);
            For(numbers,false);
        }
    }
}

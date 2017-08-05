using System;
using System.Text;

namespace RingCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ringcheck <path-to-kernel>");
                return;
            }

            var xKernelAssemblyPath = args[1];
        }
    }
}

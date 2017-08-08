using System;
using System.Text;

namespace TheRingMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: theringmaster <path-to-kernel>");
                return;
            }

            var xKernelAssemblyPath = args[1];
        }
    }
}

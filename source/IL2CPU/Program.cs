using System;

namespace IL2CPU
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Cosmos.IL2CPU.Program.Run(args, Console.WriteLine, s =>
            {
                Console.Write("Error: ");
                Console.WriteLine(s);
            });
        }
    }
}

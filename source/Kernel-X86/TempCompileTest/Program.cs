using System;

namespace TempCompileTest {
    class Program {
        static void Main(string[] args) {
            var x = new Cosmos.System.FileSystem();
            Console.WriteLine(x.GetFileSomething());

            Console.WriteLine();
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
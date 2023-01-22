using System;
using System.IO;
using ZLibrary;

namespace ZTestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data;
            using (var reader = new FileStream("..\\..\\..\\..\\ZKernel\\ZORK1.DAT", FileMode.Open))
            {
                data = new byte[reader.Length];
                reader.Read(data, 0, data.Length);
                reader.Close();
                reader.Dispose();
            }

            var machine = new ZMachine(data);
            machine.Run();

            Console.ReadKey();
        }
    }
}

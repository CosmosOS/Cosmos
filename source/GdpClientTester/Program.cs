using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.GdbClient;
using Cosmos.GdbClient.BasicCommands;
using Cosmos.GdbClient.Tools;
using System.Threading;

namespace GdpClientTester
{
    class Program
    {
        private static Queue<string> messages = new Queue<string>();

        static void Main(string[] args)
        {
            GdbConnection connection = new GdbConnection();
            GdbController.Instance = new GdbController(connection);
            try
            {
                GdbController.Instance.Open();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("GDB not running");
                return;
            }

            new ContinueCommand().Send();
            Console.WriteLine("Running, press a key to trace!");
            Console.ReadLine();
            new BreakCommand().Send();

            ThreadStart start = new ThreadStart(Worker);
            start.BeginInvoke(null, null);

            int ticks = Environment.TickCount;
            int count = 1;

            while (true)
            {
                X86Registers regs = X86Registers.FromString(new GetRegistersCommand().Send());
                lock(messages)
                    messages.Enqueue(BitConverter.ToUInt32(regs.Registers[14], 0).ToString("x"));
                new StepCommand().Send();

                //int ticks2 = Environment.TickCount;
                //messages.Enqueue(((ticks2 - ticks) / count).ToString());
                //count ++;
             }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void Worker()
        {
            while (true)
            {
                string data = null;
                lock (messages)
                    if (messages.Count != 0)
                        data = messages.Dequeue();
                if(data != null)
                    Console.WriteLine(data);
            }
        }
    }
}

using System;
using Cosmos.Compiler.Builder;

namespace Cosmos.Playground.Xenni
{
	class Program
    {
		[STAThread]
		static void Main(string[] args)
        {
            BuildUI.Run();
        }

		// Main entry point of the kernel
		public static void Init()
        {
            new Cosmos.Sys.Boot().Execute();

            Console.WriteLine("");
            Console.WriteLine("Press Any Key to Test MultiArrays ...");
            Console.Read();
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Write One, Read One");
            int[,,,] WOROArray = new int[3, 3, 3, 3];
            Random r = new Random((int)Cosmos.Hardware.Global.TickCount + Cosmos.Hardware.RTC.GetSeconds());
            for (int x = 0;x < 3;x++)
            {
                for (int y = 0;y < 3;y++)
                {
                    for (int z = 0;z < 3;z++)
                    {
                        for (int a = 0;a < 3;a++)
                        {
                            int value = r.Next();
                            WOROArray[x, y, z, a] = value;
                            int stovalue = WOROArray[x, y, z, a];

                            if (stovalue == value)
                            {
                                Console.Write("[" + x + "," + y + "," + z + "," + a + "] ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("PASSED");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine(" ");
                            }
                            else
                            {
                                Console.Write("[" + x + "," + y + "," + z + "," + a + "] ");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("FAILED");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("; expected '" + value + "' but got '" + stovalue + "'");
                            }

                            Console.Read();
                        }
                    }
                }
            }
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Write One, Read One - RANDOM ACCESS [Q to quit]");
            int[,,,] WORORArray = new int[3, 3, 3, 3];
            while(Console.Read() != 113)
            {
                int x = r.Next() % 3;
                int y = r.Next() % 3;
                int z = r.Next() % 3;
                int a = r.Next() % 3;

                int value = r.Next();
                WORORArray[x, y, z, a] = value;
                int stovalue = WORORArray[x, y, z, a];

                if (stovalue == value)
                {
                    Console.Write("[" + x + "," + y + "," + z + "," + a + "] ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("PASSED");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" ");
                }
                else
                {
                    Console.Write("[" + x + "," + y + "," + z + "," + a + "] ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("FAILED");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("; expected '" + value + "' but got '" + stovalue + "'");
                }
            }
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Write All, Read All [Sequential Values]");
            int[,,,] WARAArray = new int[3, 3, 3, 3];

            for (int x = 2;x >= 0;x--)
            {
                for (int y = 2;y >= 0;y--)
                {
                    for (int z = 2;z >= 0;z--)
                    {
                        for (int a = 2;a >= 0;a--)
                        {
                            int value = (x * 3 * 3 * 3) + (y * 3 * 3) + (z * 3) + a;
                            WARAArray[x, y, z, a] = value;
                        }
                    }
                }
            }
            for (int x = 2;x >= 0;x--)
            {
                for (int y = 2;y >= 0;y--)
                {
                    for (int z = 2;z >= 0;z--)
                    {
                        for (int a = 2;a >= 0;a--)
                        {
                            int value = (x * 3 * 3 * 3) + (y * 3 * 3) + (z * 3) + a;
                            int stovalue = WARAArray[x, y, z, a];

                            if (stovalue == value)
                            {
                                Console.Write("[" + x + "," + y + "," + z + "," + a + "] ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("PASSED");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine(" ");
                            }
                            else
                            {
                                Console.Write("[" + x + "," + y + "," + z + "," + a + "] ");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("FAILED");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("; expected '" + value + "' but got '" + stovalue + "'");
                            }

                            Console.Read();
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Press Any Key to Restart ...");
            Console.Read();

            Cosmos.Sys.Deboot.Reboot();
		}
    }
}
using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware.PC.Bus;
using Cosmos.FileSystem;
using Cosmos.Hardware;
using System.Diagnostics;
using S = Cosmos.Hardware.TextScreen;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args) {
            BuildUI.Run();
        }
        #endregion

        class Player
        {
            public int x,y,d,i;
            public bool alive;
        }

        private class MRandom
        {
            int a = 214013;
            int x = 0x72535;
            int c = 2531011;


            public MRandom(int seed)
            {
                x = seed;
            }
            public int Next(int p)
            {
                x = (a * x + c);
                return x % p;
            }
        }
        // Main entry point of the kernel
        public static void Init()
        {
            Cosmos.Sys.Boot.Default();

            Console.WriteLine(Cosmos.Kernel.CPU.CPUVendor);

            S.Clear();
            for (int f = 8; f < 16; f++)
                for (int b = 0; b < 8; b++)
                {
                    S.SetColors((ConsoleColor)f, (ConsoleColor)b);
                    Console.Write("     Nibbles on COSMOS! coded by Stephen Remde     ");
                }

            wait(5000);


            MRandom r = new MRandom((int)Cosmos.Hardware.Global.TickCount + Cosmos.Hardware.RTC.GetSeconds());

            int playersc = 15;

            while (true)
            {
                S.SetColors(ConsoleColor.Black, ConsoleColor.Black);
                S.Clear();
                
                Player[] players = new Player[playersc];

                bool[] board = new bool[S.Columns * S.Rows];

                for (int i = 0; i < playersc; i++)
                {

                    players[i] = new Player()
                    {
                        x = r.Next(S.Columns),
                        y = r.Next(S.Rows),
                        d = r.Next(4),
                        i = i,
                        alive = true
                    };
                }

                bool playera = true;
                while (playera)
                {
                    wait(25);

                    playera = false;

                    foreach (Player p in players)
                    {
                        if (p.alive)
                        {
                            S.SetColors((ConsoleColor)(p.i + 1), (ConsoleColor)(p.i + 1));
                            S.PutChar(p.y, p.x, 'X');
                            
                            board[p.x + p.y * S.Columns] = true;
                        
                            p.alive = false;
                        
                            for (int dd = 0; dd < 4; dd++)
                            {
                                int nx = p.x;
                                int ny = p.y;
                                switch ((p.d + dd) % 4)
                                {
                                    case 0:
                                        nx--;
                                        break;
                                    case 1:
                                        ny--;
                                        break;
                                    case 2:
                                        nx++;
                                        break;
                                    case 3:
                                        ny++;
                                        break;
                                }
                                if (nx >= 0 && nx < S.Columns && ny >= 0 && ny < S.Rows &&
                                    board[nx + ny * S.Columns] == false)
                                {
                                    p.alive = true;
                                    p.x = nx;
                                    p.y = ny;
                                    p.d = (p.d + dd) % 4;
                                    playera = true;
                                    break;
                                }

                            }
                        }
                    }
                }                

                wait(2000);
            }
        }
        private static void wait(int ms)
        {
            long tick = Cosmos.Hardware.Global.TickCount + Cosmos.Hardware.PIT.TicksPerSecond * ms / 1000;
            while (Cosmos.Hardware.Global.TickCount < tick)
                ;
        }

        private  void c()
        {
            //byte mybyte = 0;
            //string s  = ((byte)0).ToString();
            //string s2 = ((object)mybyte).ToString();

            //object oo;
            //byte bb = (byte)5;
            //byte[] ba = new byte[1];
            //ba[0] = 6;
                        
            //oo = ba[0];
            //Console.WriteLine("" + oo.ToString());
            //Console.WriteLine(""+ba[0].ToString());
            //oo = bb;
            //Console.WriteLine("" + oo);
            //Console.WriteLine("" + bb);

            //byte by = 8;
            //Console.WriteLine(by);
            //Console.WriteLine(by.ToString());


#if false
            Console.WriteLine(u(a) + u(b) + u(c) + u(d));
            Cosmos.Kernel.CPU.GetCPUId(out d, out c, out b, out a, 1);
            Console.WriteLine(u(a) + u(b) + u(c) + u(d));



            Console.WriteLine("Done booting");


            Console.WriteLine("looking for devices");
            Debugger.Break();
            Cosmos.Hardware.Storage.ATA2.ATA.Initialize(Cosmos.Hardware.Global.Sleep);

            Console.WriteLine("looking for mbr");

            for (int i = 0; i < Cosmos.Hardware.Device.Devices.Count; i++)
            {
                Device dev = Cosmos.Hardware.Device.Devices[i];


                if (dev is Disk)
                {
                    Disk bd = dev as Disk;
                    MBR mbr = new MBR(bd);

                    Console.WriteLine("WARNING: ABOUT TO REWRITE MBR OF " + bd.Name);
                    Console.ReadLine();

                    mbr.DiskSignature = 0x12345678;
                    mbr.Partition[0].StartLBA = 1;
                    mbr.Partition[0].LengthLBA = (uint)(bd.BlockCount - 1);
                    mbr.Partition[0].PartitionType = 0x0c;
                    mbr.Partition[0].Bootable = true;

                    mbr.Save();

                    Console.WriteLine("wrote a fat partition of size + " + (mbr.Partition[0].LengthLBA) * 512); ;


                    Console.ReadLine();
                }
                else
                {

                    Console.WriteLine("skipping " +dev.Name);
                }
            }

            
			System.Diagnostics.Debugger.Break();
            // Kudzu: Moved to Kudzu.PCITest - maybe should go somehwere common, but dont want debug
            // in main Cosmos code
            //Cosmos.Hardware.PC.Bus.PCIBus.DebugLSPCI();
            
            
            PCIDeviceNormal rtlpci = PCIBus.GetPCIDevice(0, 3, 0) as PCIDeviceNormal;
            if (rtlpci != null)
                Console.WriteLine("got rtl pci");

            Cosmos.Kernel.MemoryAddressSpace mas = rtlpci.GetAddressSpace(1) as Cosmos.Kernel.MemoryAddressSpace;
            if (mas != null)
                Console.WriteLine("got mas");
            
            Console.WriteLine("dumping memory space:");
            
            //for (uint i = 0; i < mas.Size; i++) 
                // Conver to extensino method as per your commetns. :)
                //Console.Write(PCIBus.ToHex(mas.Read8Unchecked(i),2) +" ");

#endif
            while (true)
                ;
        }
    }
}
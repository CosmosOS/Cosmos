using System;
using Cosmos.Build.Windows;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            var xBuilder = new Builder();
            xBuilder.Build();
        }
        #endregion



       private static string[] classtext = new string[]           
       {
        "pre pci 2.0",		// 00
        "disk",		// 01
        "network",		// 02
        "display",		// 03
        "multimedia",	// 04
        "memory",		// 05
        "bridge",		// 06
        "communication",	// 07
        "system peripheral",// 08
        "input",		// 09
        "docking station",	// 0A
        "CPU",		// 0B
        "serial bus",	// 0C
       };

       private static string[][] subclasstext = new string[][]
        { 
            new string[] {},
            new string[] { "SCSI" ,"IDE" , "floppy","IPI","RAID", "other" },
            new string[] { "Ethernet", "TokenRing", "FDDI" , "ATM" , "other" },
            new string[] { "VGA", "SuperVGA","XGA", "other"},
            new string[] { "video" ,"audio", "other"},
            new string[] { "RAM", "Flash memory" , "other"},
            new string[] { "CPU/PCI" ,"PCI/ISA" , "PCI/EISA" , "PCI/MCA","PCI/PCI" , "PCI/PCMCIA", "PCI/NuBus", "PCI/CardBus", "other"},
            new string[] { "serial", "parallel", "other"},
            new string[] { "PIC", "DMAC" , "timer" ,"RTC", "other"},
            new string[] { "keyboard","digitizer","mouse", "other" },
            new string[] { "generic" , "other" },
            new string[] { "386", "486","Pentium" , "P6" ,"Alpha","coproc","other" },
            new string[] { "Firewire", "ACCESS.bus" , "SSA", "USB" ,"Fiber Channel" , "other"},

        };

        // Main entry point of the kernel
        public static void Init()
        {
            Console.WriteLine("Done booting");

            // all work fine
            Console.Write("pre pci 2.0 = '"); Console.Write(classtext[0]);            Console.WriteLine("'");
            Console.Write("disk = '"); Console.Write(classtext[1]);            Console.WriteLine("'");
            Console.Write("network = '"); Console.Write(classtext[2]);            Console.WriteLine("'");
            Console.Write("display = '"); Console.Write(classtext[3]);            Console.WriteLine("'");
            Console.Write("multimedia = '"); Console.Write(classtext[4]);Console.WriteLine("'");
            Console.Write("memory = '"); Console.Write(classtext[5]);Console.WriteLine("'");
            Console.Write("bridge = '"); Console.Write(classtext[6]);Console.WriteLine("'");
            Console.Write("communication = '"); Console.Write(classtext[7]);Console.WriteLine("'");
            Console.Write("system peripheral = '"); Console.Write(classtext[8]);Console.WriteLine("'");
            Console.Write("input = '"); Console.Write(classtext[9]);Console.WriteLine("'");
            Console.Write("docking station = '"); Console.Write(classtext[10]);Console.WriteLine("'");
            Console.Write("CPU = '"); Console.Write(classtext[11]); Console.WriteLine("'");
            Console.Write("serial bus = '"); Console.Write(classtext[12]); Console.WriteLine("'");

            // doesnt wrok
            Console.Write("network / Ethernet = '"); 
            Console.Write(classtext[2] + " / " + subclasstext[2][0]); 
            Console.WriteLine("'");

            while (true)
                ;
        }
    }
}
//TODO : rewrite class
/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using DuNodes.HAL;
using DuNodes.HAL.Core;
using DuNodes.System.Core;

namespace DuNodes.System.Console
{
    public static class Bootscreen
    {
 //       public static void Show(int durationinseconds)
 //       {
 //           int g = 0;
 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.CursorLeft = 22;
 //           // Ligne 1
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Red, ConsoleColor.Black);
 //           Console.Write("@@@@@ @  @@  @       @  ");
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Cyan, ConsoleColor.Black);
 //           Console.WriteLine("@@    @@   @@");
 //           Console.CursorLeft = 22;
 //           // Ligne 2
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Red, ConsoleColor.Black);
 //           Console.Write("@     @ @  @ @       @  ");
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Cyan, ConsoleColor.Black);
 //           Console.WriteLine("@ @  @  @ @  @");
 //           Console.CursorLeft = 22;
 //           // Ligne 3
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Red, ConsoleColor.Black);
 //           Console.Write("@@@@  @ @  @ @   @   @  ");
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Cyan, ConsoleColor.Black);
 //           Console.WriteLine("@  @ @  @ @  ");
 //           Console.CursorLeft = 22;
 //           // Ligne 4
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Red, ConsoleColor.Black);
 //           Console.Write("@     @ @  @  @ @ @ @   ");
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Cyan, ConsoleColor.Black);
 //           Console.WriteLine("@  @ @  @  @@");
 //           Console.CursorLeft = 22;
 //           // Ligne 5
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Red, ConsoleColor.Black);
 //           Console.Write("@     @ @  @   @   @    ");
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Cyan, ConsoleColor.Black);
 //           Console.WriteLine("@ @  @  @ @  @");
 //           Console.CursorLeft = 22;
 //           // Ligne 6
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Red, ConsoleColor.Black);
 //           Console.Write("@     @  @@    @   @    ");
 //           Cosmos.Hardware.Global.TextScreen.SetColors(ConsoleColor.Cyan, ConsoleColor.Black);
 //           Console.WriteLine("@@    @@   @@");

 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.WriteLine();
 //           Console.CursorTop = 24;
 //           Console.ForegroundColor = ConsoleColor.White;
 //           Console.WriteLine("Copyright " + (char)(byte)'©' + " 2013 zDimension");
 //           Console.ForegroundColor = ConsoleColor.Cyan;

 //           int h = 0;
 //           while (h != 1)
 //           {

 //               Console.CursorTop = 15;
 //               if (g == 0)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|_________________________________|");
 //                   g = 1;
 //               }
 //               else if (g == 1)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|_________Loading kernel..._______|");
 //                   g = 4;
 //               }
 //               /*else if (g == 2)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|________Checking memory..._______|");
 //                   g = 3;
 //               }
 //               else if (g == 3)
 //               {
 //                   uint mem = CPU.GetAmountOfRAM();
 //                   mem = mem + 2;

 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   string s = "";
 //                   s += mem;
 //                   s += " MB OK";
 //                   int i = 0;
 //                   double abc = 16.5;
                   

 //                   double bcd = s.Length / 2;
 //i = int.Parse("" + Math.Round(bcd));
 //double cde = abc - bcd;
 //int cdef = int.Parse("" + Math.Round(cde));
 //Console.WriteLine(cdef);
 //string ss = new string('_', cdef);
 //                   Console.WriteLine("|" + ss + "" + mem + " MB OK" + ss +"|");
 //                   g = 4;
 //               }*/
 //               else if (g == 4)
 //               {
 //                   Environment.DevMgr.Init();
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|______Checking hard drive..._____|");
 //                   GruntyOS.HAL.ATA.Detect(); // This will detect all ATA devices and add them to the device filesystem
 //                   GruntyOS.CurrentUser.Privilages = 0; // This has to be set, 1 = limited 0 = root
 //                   GruntyOS.CurrentUser.Username = "root"; // When using anything in the class File this will be the default username

 //                   GruntyOS.HAL.FileSystem.Root = new GruntyOS.HAL.RootFilesystem(); // initialize virtual filesystem
 //                   for (int i = 0; i < GruntyOS.HAL.Devices.dev.Count; i++)
 //                   {
 //                       if (GruntyOS.HAL.Devices.dev[i].dev is Cosmos.Hardware.BlockDevice.Partition)
 //                       {
 //                           Kernel.fd = new GruntyOS.HAL.GLNFS((Cosmos.Hardware.BlockDevice.Partition)GruntyOS.HAL.Devices.dev[i].dev);


 //                           if (GruntyOS.HAL.GLNFS.isGFS((Cosmos.Hardware.BlockDevice.Partition)GruntyOS.HAL.Devices.dev[i].dev))
 //                           {
 //                               //GruntyOS.HAL.FileSystem.Root.Mount("/", Kernel.fd);
 //                               Kernel.CreateFolders();
 //                               g = -5;
 //                           }
 //                           else
 //                           {
 //                               //Kernel.fd.Format("FLOWDOS");
 //                               g = 7;
 //                           }
 //                           GruntyOS.HAL.FileSystem.Root.Mount("/", Kernel.fd); // mount it as root (you can only have on partition mounted as root!!!!
 //                           GruntyOS.HAL.FileSystem.Root.Mount("/dev/", Kernel.devFS);
 //                           //GruntyOS.HAL.FileSystem.Root.Mount("/dev", Environment.GLNxDevFS);
 //                           /*if (FS.ByteToString(Kernel.fd.readFile("/sys/info.fd")) == "FLOWDOS")
 //                           {
 //                               g = 5;

 //                           }
 //                           else
 //                           {
 //                               g = 7;
 //                           }*/
 //                       }
 //                   }

 //               }
 //               else if (g == 5)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|___Detected a GLNFS Partition.___|");

 //                   g = 6;
 //               }
 //               else if (g == 6)
 //               {
 //                   Global.Dbg.Send("HELLO!");
 //                   Global.Dbg.Break();
 //                   //Debugger.Break();
 //                   if (FS.ByteToString(Kernel.fd.readFile("/sys/info.fd")) == "FLOWDOS")
 //                   {
 //                       g = -5;

 //                   }
 //                   else
 //                   {
 //                       g = 7;
 //                   }
 //               }
 //               else if (g == 7)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|__________Formatting...__________|");
 //                   Kernel.fd.Format("FLOWDOS");
 //                   Kernel.CreateFolders();
 //                   g = 8;
 //               }
 //               else if (g == 8)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|____________Booting...___________|");
 //                   //   Time.Wait(1);
 //                   //g = 6;
 //                   //break;
 //                   g = 9;
 //               }
 //               else if (g == 9)
 //               {
 //                   h = 1;
 //               }
 //               else if (g == -5)
 //               {
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine(" _________________________________");
 //                   Console.CursorLeft = 22;
 //                   Console.WriteLine("|__Detected a FlowDOS Partition.__|");

 //                   g = 8;
 //               }
 //               //Time.WaitMS(500);
 //               Time.Wait(1);

 //               //g++;
 //           }
 //       }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Reflection;
using System.Diagnostics;
using Cosmos.Sys.Network;
using Cosmos.Hardware;

namespace MatthijsTest
{
	public class Program
	{
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args)
		{
			Cosmos.Compiler.Builder.BuildUI.Run();
		}
		#endregion

		public static void Init(){

            var xInit = false;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }

            var xInt = (int)55;
            var xByte = (byte)55;
        Console.Write("Integer: ");
            Console.WriteLine(((int)xInt).ToString());
            Console.Write("Byte: ");
            Console.WriteLine(((byte)xByte).ToString());
            var xObj = (object)xByte;
            xByte = (byte)xObj;
            Console.Write("Byte 2: ");
            Console.WriteLine(((object)xByte).ToString());
            

            //TCPIPStack.Init();

            //TCPIPStack.ConfigIP(
//            var xCount = Device.Devices.Count;
            //for (int i = 0; i < Device.Devices.Count; i++)
            //{
            //    var xItem = Device.Devices[i];
            //    //if (xItem == null)
            //    //{
            //    //    Console.WriteLine("NULL item!");
            //    //    continue;
            //    //}
            //    //var xType = Device.Devices[i].Type;
            //    //if (Device.Devices[i].Type == Device.DeviceType.Network)
            //    {
            //        xCount++;
            //    }
            //}
            //if (xCount == 0)
            //{
            //    Console.WriteLine("No devices found!");
            //}
            //Console.Write("Network cards found: ");
            //Console.WriteLine(xCount.ToString());
		}

    }
}
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

					var xBoot = new Cosmos.Sys.Boot();
			xBoot.Execute();

            //TCPIPStack.Init();

            //TCPIPStack.ConfigIP(
            var xCount = 0;
            for (int i = 0; i < Device.Devices.Count; i++)
            {
                var xItem = Device.Devices[i];
                if (xItem == null)
                {
                    Console.WriteLine("NULL item!");
                    continue;
                }
                var xType = Device.Devices[i].Type;
                if (Device.Devices[i].Type == Device.DeviceType.Network)
                {
                    xCount++;
                }
            }
            Console.Write("Network cards found: ");
            Console.WriteLine(xCount.ToString());
		}

    }
}
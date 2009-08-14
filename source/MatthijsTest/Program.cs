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
			TCPIPStack.Init();
			Console.WriteLine("Initialized!");
		}

	}
}
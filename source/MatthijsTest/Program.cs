using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cosmos.Compiler.Builder;
using Cosmos.Sys.FileSystem;
using Cosmos.Sys.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;
using Cosmos.Sys.Network;

namespace MatthijsTest
{
    public class Program
    {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args)
        {
            //Init();
            BuildUI.Run();
        }

        #endregion

        public static unsafe void Init()
        {
            bool xTest = false;
            if (xTest)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }
            throw new NotImplementedException("Method needs plugging. Signature: bladibladibladibla");
            Console.WriteLine("Hello, World!");
            //Cosmos.Sys.Deboot.ShutDown();

        }
    }
}
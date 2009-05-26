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
            byte* xTemp = (byte*) 0x1;
            for (int i = 0; i < 32 * 1024; i++ )
            {
                if (xTemp[i] == 0x50 && xTemp[i+1] == 0x4D && xTemp[i+2] == 0x49 && xTemp[i+3]== 0x44)
                {
Console.WriteLine(i.ToString());
                }
            }
            //throw new NotImplementedException("Method needs plugging. Signature: bladibladibladibla");
            Console.WriteLine("Hello, World!");
            //Cosmos.Sys.Deboot.ShutDown();

        }
    }
}
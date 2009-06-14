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
            bool xTest = true;
            if (xTest)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }
            var xDirectories = Directory.GetDirectories("/0");

            for (int i = 0; i < xDirectories.Length; i++)
            {
                Console.WriteLine(xDirectories[i]);
            }
            Console.Write("Number of devices: ");
            Console.WriteLine(Device.Devices.Count.ToString());
            //var xItem = new Derived();
            //Console.WriteLine(xItem.Type);
            //Base xBase = xItem;
            //Console.WriteLine(xBase.Type);
        }


        public abstract class Base
        {
            public abstract string Type
            { get;
                //{
                //    return "Base";
                //}
            }
        }

        public class Derived: Base
        {
            public override string Type
            {
                get
                {
                    return "Derived";
                }
            }
        }

    }
}
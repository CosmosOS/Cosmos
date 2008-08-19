using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;

namespace FrodeTest.Test
{
    public class Ext2Test
    {
        public static void RunTest()
        {
            BlockDevice harddisk = null;

            //Get first storage device
            foreach(BlockDevice block in Cosmos.Hardware.BlockDevice.Devices)
                if (block.Type == Device.DeviceType.Storage)
                {
                    harddisk = block;
                    break;
                }

            if (harddisk == null)
            {
                Console.WriteLine("Unable to find harddrive to test");
                return;
            }

            Cosmos.Sys.FileSystem.Ext2.Ext2 ext = new Cosmos.Sys.FileSystem.Ext2.Ext2(harddisk);

            byte[] xBuffer = new byte[10];
            ext.ReadBlock(0, 0, xBuffer);

            Console.WriteLine("Read from disk:");
            foreach (byte b in xBuffer)
                Console.Write(b.ToString());
        }
    }
}

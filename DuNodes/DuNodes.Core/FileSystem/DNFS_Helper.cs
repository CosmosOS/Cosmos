using System;
using System.Linq;
using System.Xml.XPath;
using Cosmos.Common.Extensions;
using Cosmos.HAL.BlockDevice;
using DuNodes.HAL.Extensions;
using DuNodes.HAL.FileSystem;
using DuNodes.HAL.FileSystem.Base;


namespace DuNodes.System.FileSystem
{
    public class DNFS_Helper
    {
        public static void DetectDrives()
        {
            for (int i = 0; i < Partition.Devices.Count; i++)
            {
                if (Partition.Devices[i] is Partition)
                {

                    if (DNFS.isGFS(((Partition)BlockDevice.Devices[i])))
                    {
                        //TODO : Remove temp fix 
                        Console.Console.WriteLine(" !!! Temp FIX : Format each startup !!!  ", ConsoleColor.DarkRed, true);
                        new DNFS(((Partition)BlockDevice.Devices[i])).Format("DN");

                        DNFS fs = new DNFS(((Partition)BlockDevice.Devices[i]));
                        Devices.device dev = new Devices.device();
                        Console.Console.WriteLine("DNFS PARTITION FOUND", ConsoleColor.Blue, true);
                        dev.name = @"?\\Harddrive\Partition" + i.ToString();
                        dev.dev = ((Partition)BlockDevice.Devices[i]);
                        Devices.dev.Add(dev);
                        HAL.FileSystem.Base.FileSystem.Root = new RootFilesystem();
                        HAL.FileSystem.Base.FileSystem.Root.Mount("/", fs);
                    }
                    else
                    {
                        Console.Console.WriteLine("PARTITION FOUND BUT NOT DNFS", ConsoleColor.Blue, true);
                        Console.Console.WriteLine("Reboot will occur after format... ", ConsoleColor.Blue, true);
                        Console.Console.WriteLine("Please type Volume Name. :  ", ConsoleColor.Blue, true);
                        new DNFS(((Partition)BlockDevice.Devices[i])).Format(Console.Console.ReadLine());
                        KernelExtensionsHAL.Reboot();
                    }
                }
            }
        }
    }
}

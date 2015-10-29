using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using FAT = Cosmos.System.Filesystem.FAT;
using Cosmos.Hardware.BlockDevice;
using Cosmos.System.Filesystem.FAT;

namespace EdNuttingsTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void Run()
        {
            try
            {
                Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("Block devices found: " + BlockDevice.Devices.Count);

                AtaPio xATA = null;
                for (int i = 0; i < BlockDevice.Devices.Count; i++)
                {
                    var xDevice = BlockDevice.Devices[i];
                    if (xDevice is AtaPio)
                    {
                        xATA = (AtaPio)xDevice;
                    }
                }

                //Info
                Console.WriteLine("--------------------------");
                Console.WriteLine("Type: " + (xATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                Console.WriteLine("Serial No: " + xATA.SerialNo);
                Console.WriteLine("Firmware Rev: " + xATA.FirmwareRev);
                Console.WriteLine("Model No: " + xATA.ModelNo);
                Console.WriteLine("Block Size: " + xATA.BlockSize + " bytes");
                Console.WriteLine("Size: " + xATA.BlockCount * xATA.BlockSize / 1024 / 1024 + " MB");

                //Partition Detecting
                Partition xPartition = null;
                if (BlockDevice.Devices.Count > 0)
                {
                    for (int i = 0; i < BlockDevice.Devices.Count; i++)
                    {
                        var xDevice = BlockDevice.Devices[i];
                        if (xDevice is Partition)
                        {
                            xPartition = (Partition)xDevice;

                            Console.WriteLine("FAT FS");
                            var xFS = new FAT.FatFileSystem(xPartition);

                            Console.WriteLine("Mapping...");
                            Sys.Filesystem.FileSystem.AddMapping("C", xFS);


                            Console.WriteLine();
                            Console.WriteLine("Root directory");

                            var xListing = xFS.GetRoot();
                            FAT.Listing.FatFile xRootFile = null;
                            FAT.Listing.FatFile xKudzuFile = null;


                            for (int j = 0; j < xListing.Count; j++)
                            {
                                var xItem = xListing[j];
                                if (xItem is Sys.Filesystem.Listing.Directory)
                                {
                                    //Detecting Dir in HDD
                                    Console.WriteLine("<DIR> " + xListing[j].Name);
                                }
                                else if (xItem is Sys.Filesystem.Listing.File)
                                {
                                    //Detecting File in HDD
                                    Console.WriteLine("<FILE> " + xListing[j].Name + " (" + xListing[j].Size + ")");
                                    if (xListing[j].Name == "Root.txt")
                                    {
                                        xRootFile = (FAT.Listing.FatFile)xListing[j];
                                    }
                                    else if (xListing[j].Name == "Kudzu.txt")
                                    {
                                        xKudzuFile = (FAT.Listing.FatFile)xListing[j];
                                    }
                                }
                            }

                            try
                            {
                                Console.WriteLine();
                                Console.WriteLine("StreamReader - Root.txt File");
                                var xStream = new FAT.FatStream(xRootFile);
                                var xData = new byte[xRootFile.Size];
                                xStream.Read(xData, 0, (int)xData.Length);
                                var xText = Encoding.ASCII.GetString(xData);
                                Console.WriteLine(xText);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: " + e.Message);
                            }
                            try
                            {
                                Console.WriteLine();
                                Console.WriteLine("StreamReader - Kudzu.txt File");
                                var xStream = new FAT.FatStream(xKudzuFile);
                                var xData = new byte[xKudzuFile.Size];
                                xStream.Read(xData, 0, (int)xData.Length);
                                var xText = Encoding.ASCII.GetString(xData);
                                Console.WriteLine(xText);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: " + e.Message);
                            }

                            //var xKudzuStream = new Sys.Filesystem.FAT.FatStream(xKudzuFile);
                            //var xKudzuData = new byte[xKudzuFile.Size];
                            //xKudzuStream.Read(xKudzuData, 0, (int)xKudzuFile.Size);

                            try
                            {
                                //Console.WriteLine();
                                //Console.WriteLine("FileStream - Root File");
                                //var xRootFileStream = new System.IO.FileStream(@"c:\Root.txt", System.IO.FileMode.Open);
                                //var xData = new byte[xRootFileStream.Length];
                                //xRootFileStream.Read(xData, 0, (int)xRootFile.Size);
                                //var xText = Encoding.ASCII.GetString(xData);
                                //Console.WriteLine(xText);
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error: " + e.Message);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Block Device Found! ");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}

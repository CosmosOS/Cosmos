using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using Cosmos.Common;
using Cosmos.Hardware.BlockDevice;
using FAT = Cosmos.System.Filesystem.FAT;

namespace LWatson
{
	public class Kernel : Sys.Kernel
	{
		protected override void BeforeRun()
		{
			Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
		}

		protected override void Run()
		{
			Console.WriteLine("Running Array.Copy Test");
			char[] arr1 = new char[] { 'a', 'b', 'c' };
			char[] arr2 = new char[(long)Int32.MaxValue + 3];
			Array.Copy(arr1, (long)0, arr2, Int32.MaxValue, 3);
			Console.WriteLine("Element at index int32.MaxValue + 2: " + arr2[(long)Int32.MaxValue + 2]);
			Console.WriteLine("Running TestATA");
			TestATA();
			// var l = fs.GetRoot();
							
			Console.Write("Input: ");
			var input = Console.ReadLine();
			Console.Write("Text typed: ");
			Console.WriteLine(input);
		}
		protected void TestATA()
		{
			#region Comment(OLD)
			/*
      try
      {
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
        Console.WriteLine("--------------------------");
        Console.WriteLine("Type: " + (xATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
        Console.WriteLine("Serial No: " + xATA.SerialNo);
        Console.WriteLine("Firmware Rev: " + xATA.FirmwareRev);
        Console.WriteLine("Model No: " + xATA.ModelNo);
        Console.WriteLine("Block Size: " + xATA.BlockSize + " bytes");
        Console.WriteLine("Size: " + xATA.BlockCount * xATA.BlockSize / 1024 / 1024 + " MB");

        //Partition xPartition = null;
        //for (int i = 0; i < BlockDevice.Devices.Count; i++)
        //{
        //  var xDevice = BlockDevice.Devices[i];
        //  if (xDevice is Partition)
        //  {
        //    xPartition = (Partition)xDevice;
        //  }
        //}

        //var xFS = new FAT.FatFileSystem(xPartition);
        //Sys.Filesystem.FileSystem.AddMapping("C", xFS);

        //Console.WriteLine();
        //Console.WriteLine("Root directory");
        //var xListing = xFS.GetRoot();
        //FAT.Listing.FatFile xRootFile = null;
        //FAT.Listing.FatFile xKudzuFile = null;
        //for (int i = 0; i < xListing.Count; i++)
        //{
        //  var xItem = xListing[i];
        //  if (xItem is Sys.Filesystem.Listing.Directory)
        //  {
        //    Console.WriteLine("<" + xListing[i].Name + ">");
        //  }
        //  else if (xItem is Sys.Filesystem.Listing.File)
        //  {
        //    Console.WriteLine();
        //    Console.WriteLine(xListing[i].Name);
        //    Console.WriteLine(xListing[i].Size);
        //    if (xListing[i].Name == "Root.txt")
        //    {
        //      xRootFile = (FAT.Listing.FatFile)xListing[i];
        //    }
        //    else if (xListing[i].Name == "Kudzu.txt")
        //    {
        //      xKudzuFile = (FAT.Listing.FatFile)xListing[i];
        //    }
        //  }
        //}

        //{
        //  var xStream = new Sys.Filesystem.FAT.FatStream(xRootFile);
        //  var xData = new byte[xRootFile.Size];
        //  xStream.Read(xData, 0, (int)xRootFile.Size);
        //  var xText = Encoding.ASCII.GetString(xData);
        //  Console.WriteLine(xText);
        //}

        //{
        //  Console.WriteLine();
        //  Console.WriteLine("StreamReader");
        //  var xStream = new Sys.Filesystem.FAT.FatStream(xRootFile);
        //  var xReader = new System.IO.StreamReader(xStream);
        //  string xText = xReader.ReadToEnd();
        //  Console.WriteLine(xText);
        //}

        //var xKudzuStream = new Sys.Filesystem.FAT.FatStream(xKudzuFile);
        //var xKudzuData = new byte[xKudzuFile.Size];
        //xKudzuStream.Read(xKudzuData, 0, (int)xKudzuFile.Size);

        //string xLower = "Hello";
        //Console.WriteLine(xLower.ToUpper());
        //Console.WriteLine(xLower.ToLower());

        //var xFile = new System.IO.FileStream(@"c:\Root.txt", System.IO.FileMode.Open);

        //int dummy = 42;

        //var xWrite = new byte[512];
        //for (int i = 0; i < 512; i++)
        //{
        //  xWrite[i] = (byte)i;
        //}
        //xATA.WriteBlock(0, 1, xWrite);

        //var xRead = xATA.NewBlockArray(1);
        //xATA.ReadBlock(0, 1, xRead);
        //string xDisplay = "";
        //for (int i = 0; i < 512; i++)
        //{
        //  xDisplay = xDisplay + xRead[i].ToHex();
        //}
        //Console.WriteLine(xDisplay);

      }
      catch (Exception e)
      {
        Console.WriteLine("Exception: " + e.Message);
        Stop();
      }*/
			#endregion
			try
			{

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
							FileSystem.AddMapping(xFS);


							Console.WriteLine();
							Console.WriteLine("Root directory");

							var xListing = xFS.GetRoot();
							FAT.Listing.FatFile xRootFile = null;
							FAT.Listing.FatFile xKudzuFile = null;

							var xFile = default(FAT.Listing.FatFile);
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
									else if (xListing[j].Name == "Amanp.txt") xFile = (FAT.Listing.FatFile)xListing[j]; 
								}
							}

							try
							{
								Console.WriteLine();
								Console.WriteLine("StreamReader - Root File");
								var xStream = new Sys.Filesystem.FAT.FatStream(xRootFile);
								var xData = new byte[xRootFile.Size];
								xStream.Read(xData, 0, (int)xRootFile.Size);
								var xText = Encoding.ASCII.GetString(xData);
								Console.WriteLine(xText);
							}
							catch (Exception e)
							{
								Console.WriteLine("Error: " + e.Message);
							}

							var xKudzuStream = new Sys.Filesystem.FAT.FatStream(xKudzuFile);
							var xKudzuData = new byte[xKudzuFile.Size];
							xKudzuStream.Read(xKudzuData, 0, (int)xKudzuFile.Size);

							if (xFile != null)
							{
								//var xFs = new Sys.Filesystem.FAT.FatStream(xFile);
								//xFs.Write(new byte[1024], 0, 1024);
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

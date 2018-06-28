using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Playground.Kudzu.BreakpointsKernel;
//using Playground.Kudzu.BreakpointsKernel.FAT;
//using Playground.Kudzu.BreakpointsKernel.FAT.Listing;
using Sys = Cosmos.System;
//using Cosmos.Debug.Kernel;
//using Cosmos.Common;
//using Cosmos.HAL.BlockDevice;

//using TheFatStream = Playground.Kudzu.BreakpointsKernel.FAT.MyFatStream;
//using TheFatFile = Playground.Kudzu.BreakpointsKernel.FAT.Listing.MyFatFile;
//using TheFatFileSystem = Playground.Kudzu.BreakpointsKernel.FAT.MyFatFileSystem;

//using TheFatStream = Cosmos.System.FileSystem.FAT.FatFileStream;
//using TheFatFile = Cosmos.System.FileSystem.FAT.Listing.FatFile;
//using TheFatFileSystem = Cosmos.System.FileSystem.FAT.FatFileSystem;

namespace Kudzu.BreakpointsKernel
{
  public class BreakpointsOS : Sys.Kernel
  {
    public BreakpointsOS()
    {
      ClearScreen = false;
    }

    protected override void Run()
    {
      //Test xTest;

      //var xATA = new Cosmos.HAL.BlockDevice.AtaPio(Cosmos.Core.Global.BaseIOGroups.ATA1
      //  , Cosmos.HAL.BlockDevice.Ata.ControllerIdEnum.Primary
      //  , Cosmos.HAL.BlockDevice.Ata.BusPositionEnum.Master);
      //UInt64 xBlockSize = xATA.BlockSize;

      //Console.WriteLine("Running FieldInitTest");
      //xTest = new FieldInitTest();
      //xTest.Run();

      //Console.WriteLine("Running ListTest");
      //xTest = new ListTest();
      //xTest.Run();
      //Console.WriteLine("Running NullableTest");
      //xTest = new NullableTest();
      //xTest.Run();

      //Console.WriteLine("Running Int64Test");
      //xTest = new Int64Test();
      //xTest.Run();

      //Console.WriteLine("Running Trace1");
      //Trace1();
      //Console.WriteLine("Running TestSB");
      //TestSB();
      //Console.WriteLine("Running TestStringCtor");
      //TestStringCtor();
      //Console.WriteLine("Running TestCompare");
      //TestCompare();

      //Console.WriteLine("Running TestATA");
      ////TestATA();

      //Console.WriteLine("Press enter.");
      //Console.ReadLine();
      Stop();
    }

    protected override void BeforeRun()
    {
      Console.WriteLine("Cosmos boot complete. KudzuPlayground");
    }

    protected void TestSB()
    {
      if (String.Empty == null)
      {
        Console.WriteLine("String.Empty is not even assigned!");
        return;
      }
      Console.Write("String.Empty.Length: ");
      Console.WriteLine(String.Empty.Length);
      Console.Write("\"Test\".Length: ");
      Console.WriteLine("Test".Length);
      var xSB = new StringBuilder();
      xSB.Append("Hello");
      xSB.Append("Hello");
      var xDisplay = xSB.ToString();
      Console.WriteLine(xDisplay.Length);
      Console.WriteLine("First char: " + xSB[0]);
      Console.WriteLine(xDisplay);
    }

    private void Trace1()
    {
      int x = 4;
      Trace2();
      int y = 5;
      int z = 6;
    }

    private void Trace2()
    {
      int x2 = 4;
      Trace3();
      int y2 = 5;
      int z2 = 6;
    }

    private void Trace3()
    {
      int x3 = 4;
      int y3 = 5;
      int z3 = 6;
    }

    private void TestCompare()
    {
      UInt32 x = UInt32.MaxValue;
      int y = 0;
      if (y >= x)
      {
        Console.WriteLine("Compare failed.");
      }
      else
      {
        Console.WriteLine("Compare OK.");
      }
    }

    private void TestStringCtor()
    {
      char[] xChars = new char[5];
      xChars[0] = 'A';
      xChars[1] = 'B';
      xChars[2] = 'C';
      xChars[3] = 'D';
      xChars[4] = 'E';
      var xString = new string(xChars, 0, 3);
      Console.WriteLine(xString);
      Console.WriteLine(xString.Length);
    }

    //public void Format(Partition p)
    //{
    //  byte[] aData = p.NewBlockArray(1);
    //  p.ReadBlock(0, 1U, aData);

    //  aData[510] = 0xAA;
    //  aData[511] = 0x55;

    //  //The number of Bytes per sector (remember, all numbers are in the little-endian format).
    //  aData[11] = 0x01;
    //  aData[12] = 0xCA;
    //  aData[13] = 0x08; //Number of sectors per cluster.
    //  aData[14] = 0x01;
    //  aData[15] = 0xFF; //Number of reserved sectors. The boot record sectors are included in this value
    //  aData[16] = 0x02; //Number of File Allocation Tables (FAT's) on the storage media. Often this value is 2.
    //  aData[17] = 0x00;
    //  aData[18] = 0x0f; //Number of directory entries (must be set so that the root directory occupies entire sectors).
    //  aData[19] = 0xFF;
    //  aData[20] = 0xFF; //The total sectors in the logical volume. If this value is 0, it means there are more than 65535 sectors in the volume, and the actual count is stored in "Large Sectors (bytes 32-35).
    //  aData[22] = 0x0F;
    //  aData[23] = 0xFF; //Number of sectors per FAT. FAT12/FAT16 only.
    //  p.WriteBlock(0, 1U, aData);
    //}

    /// <summary>
    /// - Gets the first AtaPio device found
    /// - Prints info about it (kind, serial/model numbers, firmware version, sizes, etc)
    /// - Maps Fat32 to all partitions, finds a \Root.txt and a \Kudzu.txt file, and reads its contents
    /// </summary>
    //protected void TestATA()
    //{
    //  #region Comment(OLD)

    //  /*
    //  try
    //  {
    //    Console.WriteLine();
    //    Console.WriteLine("Block devices found: " + BlockDevice.Devices.Count);

    //    AtaPio xATA = null;
    //    for (int i = 0; i < BlockDevice.Devices.Count; i++)
    //    {
    //      var xDevice = BlockDevice.Devices[i];
    //      if (xDevice is AtaPio)
    //      {
    //        xATA = (AtaPio)xDevice;
    //      }
    //    }
    //    Console.WriteLine("--------------------------");
    //    Console.WriteLine("Type: " + (xATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
    //    Console.WriteLine("Serial No: " + xATA.SerialNo);
    //    Console.WriteLine("Firmware Rev: " + xATA.FirmwareRev);
    //    Console.WriteLine("Model No: " + xATA.ModelNo);
    //    Console.WriteLine("Block Size: " + xATA.BlockSize + " bytes");
    //    Console.WriteLine("Size: " + xATA.BlockCount * xATA.BlockSize / 1024 / 1024 + " MB");

    //    //Partition xPartition = null;
    //    //for (int i = 0; i < BlockDevice.Devices.Count; i++)
    //    //{
    //    //  var xDevice = BlockDevice.Devices[i];
    //    //  if (xDevice is Partition)
    //    //  {
    //    //    xPartition = (Partition)xDevice;
    //    //  }
    //    //}

    //    //var xFS = new FAT.FatFileSystem(xPartition);
    //    //Sys.Filesystem.FileSystem.AddMapping("C", xFS);

    //    //Console.WriteLine();
    //    //Console.WriteLine("Root directory");
    //    //var xListing = xFS.GetRoot();
    //    //FAT.Listing.FatFile xRootFile = null;
    //    //FAT.Listing.FatFile xKudzuFile = null;
    //    //for (int i = 0; i < xListing.Count; i++)
    //    //{
    //    //  var xItem = xListing[i];
    //    //  if (xItem is Sys.Filesystem.Listing.Directory)
    //    //  {
    //    //    Console.WriteLine("<" + xListing[i].Name + ">");
    //    //  }
    //    //  else if (xItem is Sys.Filesystem.Listing.File)
    //    //  {
    //    //    Console.WriteLine();
    //    //    Console.WriteLine(xListing[i].Name);
    //    //    Console.WriteLine(xListing[i].Size);
    //    //    if (xListing[i].Name == "Root.txt")
    //    //    {
    //    //      xRootFile = (FAT.Listing.FatFile)xListing[i];
    //    //    }
    //    //    else if (xListing[i].Name == "Kudzu.txt")
    //    //    {
    //    //      xKudzuFile = (FAT.Listing.FatFile)xListing[i];
    //    //    }
    //    //  }
    //    //}

    //    //{
    //    //  var xStream = new Sys.Filesystem.FAT.FatFileStream(xRootFile);
    //    //  var xData = new byte[xRootFile.Size];
    //    //  xStream.Read(xData, 0, (int)xRootFile.Size);
    //    //  var xText = Encoding.ASCII.GetString(xData);
    //    //  Console.WriteLine(xText);
    //    //}

    //    //{
    //    //  Console.WriteLine();
    //    //  Console.WriteLine("StreamReader");
    //    //  var xStream = new Sys.Filesystem.FAT.FatFileStream(xRootFile);
    //    //  var xReader = new System.IO.StreamReader(xStream);
    //    //  string xText = xReader.ReadToEnd();
    //    //  Console.WriteLine(xText);
    //    //}

    //    //var xKudzuStream = new Sys.Filesystem.FAT.FatFileStream(xKudzuFile);
    //    //var xKudzuData = new byte[xKudzuFile.Size];
    //    //xKudzuStream.Read(xKudzuData, 0, (int)xKudzuFile.Size);

    //    //string xLower = "Hello";
    //    //Console.WriteLine(xLower.ToUpper());
    //    //Console.WriteLine(xLower.ToLower());

    //    //var xFile = new System.IO.FileStream(@"c:\Root.txt", System.IO.FileMode.Open);

    //    //int dummy = 42;

    //    //var xWrite = new byte[512];
    //    //for (int i = 0; i < 512; i++)
    //    //{
    //    //  xWrite[i] = (byte)i;
    //    //}
    //    //xATA.WriteBlock(0, 1, xWrite);

    //    //var xRead = xATA.NewBlockArray(1);
    //    //xATA.ReadBlock(0, 1, xRead);
    //    //string xDisplay = "";
    //    //for (int i = 0; i < 512; i++)
    //    //{
    //    //  xDisplay = xDisplay + xRead[i].ToHex();
    //    //}
    //    //Console.WriteLine(xDisplay);

    //  }
    //  catch (Exception e)
    //  {
    //    Console.WriteLine("Exception: " + e.Message);
    //    Stop();
    //  }*/

    //  #endregion

    //  try
    //  {

    //    Console.WriteLine();
    //    Console.WriteLine("Block devices found: " + BlockDevice.Devices.Count);

    //    AtaPio xATA = null;
    //    for (int i = 0; i < BlockDevice.Devices.Count; i++)
    //    {
    //      var xDevice = BlockDevice.Devices[i];
    //      if (xDevice is AtaPio)
    //      {
    //        xATA = (AtaPio) xDevice;
    //      }
    //    }

    //    //Info
    //    Console.WriteLine("--------------------------");
    //    Console.WriteLine("Type: " + (xATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
    //    Console.WriteLine("Serial No: " + xATA.SerialNo);
    //    Console.WriteLine("Firmware Rev: " + xATA.FirmwareRev);
    //    Console.WriteLine("Model No: " + xATA.ModelNo);
    //    Console.WriteLine("Block Size: " + xATA.BlockSize + " bytes");
    //    Console.WriteLine("Size: " + xATA.BlockCount*xATA.BlockSize/1024/1024 + " MB");

    //    //Partition Detecting
    //    Partition xPartition = null;
    //    if (BlockDevice.Devices.Count > 0)
    //    {
    //      for (int i = 0; i < BlockDevice.Devices.Count; i++)
    //      {
    //        var xDevice = BlockDevice.Devices[i];
    //        if (xDevice is Partition)
    //        {
    //          xPartition = (Partition) xDevice;

    //          Console.WriteLine("FAT FS");
    //          var xFS = new TheFatFileSystem(xPartition);

    //          Console.WriteLine("Mapping...");
    //          Sys.FileSystem.FileSystem.AddMapping("C", xFS);


    //          Console.WriteLine();
    //          Console.WriteLine("Root directory");

    //          var xListing = xFS.GetRoot();
    //          TheFatFile xRootFile = null;
    //          TheFatFile xKudzuFile = null;


    //          for (int j = 0; j < xListing.Count; j++)
    //          {
    //            var xItem = xListing[j];
    //            if (xItem is Sys.FileSystem.Listing.Directory)
    //            {
    //              //Detecting Dir in HDD
    //              Console.WriteLine("<DIR> " + xListing[j].Name);
    //            }
    //            else if (xItem is Sys.FileSystem.Listing.File)
    //            {
    //              //Detecting File in HDD
    //              Console.WriteLine("<FILE> " + xListing[j].Name + " (" + xListing[j].Size + ")");
    //              if (xListing[j].Name == "Root.txt")
    //              {
    //                xRootFile = (TheFatFile) xListing[j];
    //                Console.WriteLine("Root file found");
    //              }
    //              else if (xListing[j].Name == "Kudzu.txt")
    //              {
    //                xKudzuFile = (TheFatFile) xListing[j];
    //                Console.WriteLine("Kudzu file found");
    //              }
    //            }
    //          }

    //          try
    //          {
    //            Console.WriteLine();
    //            Console.WriteLine("StreamReader - Root File");
    //            if (xRootFile == null)
    //            {
    //              Console.WriteLine("RootFile not found!");
    //            }
    //            var xStream = new TheFatStream(xRootFile);
    //            var xData = new byte[xRootFile.Size];
    //            var size = (int) xRootFile.Size;
    //            Console.WriteLine("Size: " + size);
    //            var sizeInt = (int)xRootFile.Size;
    //            xStream.Read(xData, 0, sizeInt);
    //            var xText = Encoding.ASCII.GetString(xData);
    //            Console.WriteLine(xText);
    //          }
    //          catch (Exception e)
    //          {
    //            Console.WriteLine("Error: " + e.Message);
    //          }

    //          if (xKudzuFile == null)
    //          {
    //            Console.WriteLine("KudzuFile not found!");
    //          }
    //          var xKudzuStream = new TheFatStream(xKudzuFile);
    //          var xKudzuData = new byte[xKudzuFile.Size];
    //          xKudzuStream.Read(xKudzuData, 0, (int) xKudzuFile.Size);

    //          var xFile = new System.IO.FileStream(@"c:\Root.txt", System.IO.FileMode.Open);

    //        }
    //      }
    //    }
    //    else
    //    {
    //      Console.WriteLine("No Block Device Found! ");
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    Console.WriteLine("Error: " + e.Message);
    //  }
    //}

    protected override void AfterRun()
    {
      Console.Write("Done");
      Console.ReadLine();
    }

  }
}

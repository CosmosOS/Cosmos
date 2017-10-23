//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Playground.Kudzu.BreakpointsKernel.FAT.Listing
//{
//    public class MyFatFile : Cosmos.System.FileSystem.Listing.File
//  {
//    public readonly MyFatFileSystem FileSystem;
//    public readonly UInt64 FirstClusterNum;

//    // Size is UInt32 because FAT doesn't support bigger.
//    // Dont change to UInt64
//    public MyFatFile(MyFatFileSystem aFileSystem, string aName, UInt32 aSize, UInt64 aFirstCluster)
//      : base(aFileSystem, aName, aSize)
//    {
//      FileSystem = aFileSystem;
//      FirstClusterNum = aFirstCluster;
//    }

//    //TODO: Seperate out the file mechanics from the Listing class
//    // so a file can exist without a listing instance
//    public List<UInt64> GetFatTable()
//    {
//      uint xBytesPerCluster = (FileSystem.SectorsPerCluster * FileSystem.BytesPerSector);
//      ulong numberOfClusters = (Size / xBytesPerCluster);
//      var xListCapacity = (int)numberOfClusters;
//      var xResult = new List<UInt64>();//xListCapacity
//      UInt64 xClusterNum = FirstClusterNum;

//      byte[] xSector = new byte[FileSystem.BytesPerSector];
//      UInt32? xSectorNum = null;

//      UInt32 xNextSectorNum;
//      UInt32 xNextSectorOffset;
//      do
//      {
//        if (FileSystem == null)
//        {
//          Console.WriteLine("$this is null! (1)");
//        }
//        FileSystem.GetFatTableSector(xClusterNum, out xNextSectorNum, out xNextSectorOffset);
//        if (FileSystem == null)
//        {
//          Console.WriteLine("$this is null! (2)");
//        }
//        if (xSectorNum.HasValue == false || xSectorNum != xNextSectorNum)
//        {
//          if (FileSystem == null)
//          {
//            Console.WriteLine("$this is null! (3)");
//          }
//          var nextSectorNum = (ulong)xNextSectorNum;
//          if (this == null)
//          {
//            Console.WriteLine("$this is null!");
//          }
//          if (FileSystem == null)
//          {
//            Console.WriteLine("$this.FileSystem is null");
//          }
//          FileSystem.ReadFatTableSector(nextSectorNum, xSector);
//          xSectorNum = xNextSectorNum;
//        }

//        xResult.Add(xClusterNum);
//        if (FileSystem == null)
//        {
//          Console.WriteLine("$this.FileSystem is null (2)");
//        }
//        var xFS = FileSystem;
//        if (xFS == null)
//        {
//          Console.WriteLine("xFS is null (2)");
//        }
//        xClusterNum = xFS.GetFatEntry(xSector, xClusterNum, xNextSectorOffset);
//      } while (!FileSystem.FatEntryIsEOF(xClusterNum));

//      return xResult;
//    }

//  }
//}
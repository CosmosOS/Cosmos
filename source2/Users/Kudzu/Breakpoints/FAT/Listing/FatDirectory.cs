using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Filesystem;

namespace Playground.Kudzu.BreakpointsKernel.FAT.Listing
{
  public class MyFatDirectory : Cosmos.System.Filesystem.Listing.Directory
  {
    public MyFatDirectory(FileSystem aFileSystem, string aName)
      : base(aFileSystem, aName)
    {
    }
  }
}

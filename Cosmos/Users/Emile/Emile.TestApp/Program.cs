using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emile.TestApp
{
  internal class Program
  {
    private static unsafe void Main(string[] args)
    {
      var xMemoryBlock = new int[1024*1024*2];
      var xMemoryBlockSize = xMemoryBlock.Length * 4;
      // we have an 8 MB block now
      fixed (int* xStart = &xMemoryBlock[0])
      {
        ReallySimpleAllocator.Initialize(xStart, xMemoryBlockSize);

        // now you can just use if however you like:


        // allocate a 1 kilobyte block
        var xBlock1 = ReallySimpleAllocator.Allocate(1024);

        // another one
        var xBlock2 = ReallySimpleAllocator.Allocate(1024);

        ReallySimpleAllocator.Free(xBlock1);
        xBlock1 = ReallySimpleAllocator.Allocate(1024);

        // etc



        // there's no need to cleanup.
      }

    }
  }
}

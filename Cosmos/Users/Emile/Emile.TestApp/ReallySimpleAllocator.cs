using System;

namespace Emile.TestApp
{
  public unsafe static class ReallySimpleAllocator
  {
    public static void Initialize(int* startAddress, int length)
    {
      throw new NotImplementedException();
    }

    public static int* Allocate(int length)
    {
      throw new NotImplementedException();
    }

    public static void Free(int* pointer)
    {
      throw new NotImplementedException();
    }
  }
}

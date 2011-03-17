using System;
using System.Collections.Generic;
using IO = System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.IO {
  [Plug(Target = typeof(IO::FileStream))]
  public class FileStreamImpl {

    static public void Ctor(IO::FileStream aThis, String aPathname, IO::FileMode aMode) {
    }

    static public void CCtor() {
      // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
    }

    static public int Read(IO::FileStream aThis, byte[] aBuffer, int aOffset, int aCount) {
      return 0;
    }

    //static void Init(IO::FileStream aThis, string path, IO::FileMode mode, IO::FileAccess access, int rights, bool useRights, IO::FileShare share, int bufferSize
    //  , IO::FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }

  }
}

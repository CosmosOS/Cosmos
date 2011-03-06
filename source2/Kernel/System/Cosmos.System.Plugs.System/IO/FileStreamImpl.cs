using System;
using System.Collections.Generic;
using IO = System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.IO {
  [Plug(Target = typeof(IO::FileStream))]
  public class FileStreamImpl {

    static public void FileStream(String aPathname, IO::FileMode aMode) {
      global::System.Console.WriteLine("File open");
    }


    //static void Init(IO::FileStream aThis, string path, IO::FileMode mode, IO::FileAccess access, int rights, bool useRights, IO::FileShare share, int bufferSize
    //  , IO::FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }

  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.IO;
using SentinelKernel.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    [Plug(Target = typeof(DriveInfo))]
    public static class DriveInfoImpl
    {
        [PlugMethod(Signature = "System_Void__System_IO_DriveInfo__ctor_System_String_")]
        public static void Ctor(DriveInfo aThis, string aDriveName)
        {
            
        }

        public static string get_DriveFormat(ref DriveInfo aThis)
        {
            return "DriveFormat not implemented";
        }
    }
}

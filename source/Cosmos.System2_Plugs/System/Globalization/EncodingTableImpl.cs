#define COSMOSDEBUG

using System;

using Cosmos.Debug.Kernel;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{

    [Plug("System.Globalization.EncodingTable, System.Private.CoreLib")]
    public static class EncodingTableImpl
    {
 
        private static Debugger mDebugger = new Debugger("System", "SingleByteEncoding");

        /*
         * This is Table is pratically empty in Net Core, but instatiate a Dictionary that Cosmos yet does not
         * support when it will support them probably this plug will be not needed anymore.
         */
        public static void Cctor()
        {
        }

        public static object GetCodePageDataItem(int codepage)
        {
            mDebugger.SendInternal($"GetCodePageDataItem for codepage {codepage}");
            return null;
        }

        public static int GetCodePageFromName(string name)
        {
            mDebugger.SendInternal($"GetCodePageFromName for name {name}");
            return -1;
        }

        public static int GetNumEncodingItems()
        {
            throw new NotImplementedException("GetNumEncodingItems");
        }
    }
}

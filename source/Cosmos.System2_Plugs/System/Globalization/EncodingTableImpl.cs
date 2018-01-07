using Cosmos.Debug.Kernel;
using Cosmos.System2_Plugs.System.Text;
using IL2CPU.API.Attribs;
using System;
using System.Collections;
using System.Text;

namespace Cosmos.System2_Plugs.System.Globalization
{
    [Plug(TargetName = "System.Globalization.EncodingTable")]
    public static class EncodingTableImpl
    {
        /*
         * This is Table is pratically empty in Net Core, but instatiate a Dictionary that Cosmos yet does not
         * support when it will support them probably this plug will be not needed anymore.
         */
        public static void Cctor()
        {
        }

        public static object GetCodePageDataItem(int codepage)
        {
            return null;
        }

        public static int GetCodePageFromName(string name)
        {
            return -1;
        }
    }
}

using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{

    [Plug("System.Text.EncodingTable, System.Private.CoreLib")]
    public static class EncodingTableImpl
    {
        #region Methods

        /*
         * This is Table is pratically empty in Net Core, but instatiate a Dictionary that Cosmos yet does not
         * support when it will support them probably this plug will be not needed anymore.
         */
        public static void Cctor()
        {
        }

        public static object GetCodePageDataItem(int codepage)
        {
            debugger.SendInternal($"GetCodePageDataItem for codepage {codepage}");
            return null;
        }

        public static int GetCodePageFromName(string name)
        {
            debugger.SendInternal($"GetCodePageFromName for name {name}");
            return -1;
        }

        #endregion

        #region Fields

        private static readonly Debugger debugger = new("SingleByteEncoding");

        #endregion
    }
}
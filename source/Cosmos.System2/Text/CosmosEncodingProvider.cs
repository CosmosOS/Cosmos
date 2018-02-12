#define COSMOSDEBUG
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    public class CosmosEncodingProvider : EncodingProvider
    {
        private static readonly EncodingProvider s_singleton = new CosmosEncodingProvider();
        private static Debugger myDebugger = new Debugger("System", "CosmosEncodingProvider");

        internal CosmosEncodingProvider() { }

        public static EncodingProvider Instance
        {
            get { return s_singleton; }
        }

        public override Encoding GetEncoding(int codepage)
        {
            myDebugger.SendInternal($"Getting Encoding for codepage {codepage}");
            if (codepage < 0 || codepage > 65535)
                return null;

            /* Let's check on our EncodingTable, if codepage is not found null is returned */
            return EncodingTable.GetEncoding(codepage);
        }

        public override Encoding GetEncoding(string name)
        {
            myDebugger.SendInternal($"Getting Encoding for codepage with name {name}");
            int codepage = EncodingTable.GetCodePageFromDesc(name);
            if (codepage == -1)
                return null;

            return GetEncoding(codepage);
        }
    }
}

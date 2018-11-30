#define COSMOSDEBUG
using System.Collections;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{   
    /*
     * Ideally we should use Dictionary or HashTable here but are yet not working in Cosmos so I have done
     * this replacement class for now...
     */
    internal static class EncodingTable
    {
        private static Debugger mDebugger = new Debugger("System", "EncodingTable");
        private static Hashtable CodepageCache = new Hashtable();
        private static Hashtable NamesCache = new Hashtable();

        static EncodingTable()
        {
            mDebugger.SendInternal("Inizializing Encoding Table");

            Add(437, "IBM437", new CP437Enconding());
            Add(858, "IBM00858", new CP858Enconding());
        }

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        /*
         * TODO restore as struct when unbox is really fixed...
         */
        // private struct Values
        private class Values
        {
            public string   desc;
            public Encoding encoding;

            public Values(string desc, Encoding encoding)
            {
                this.desc = desc;
                this.encoding = encoding;
            }
        }

        const int MaxCodepageChacheSize = 2048;
        //static values[] CodepageCache = new values[MaxCodepageChacheSize];

        public static void Add(int codepage, string desc, Encoding encoding)
        {
            mDebugger.SendInternal($"Adding codepage {codepage} desc {desc}");
            CodepageCache[codepage] = new Values(desc, encoding);
            NamesCache[desc] = codepage;
            //CodepageCache[codepage] = new values(desc, encoding);
            mDebugger.SendInternal("Done");
        }

        public static string GetDescription(int codepage)
        {
            mDebugger.SendInternal($"Getting description for codepage {codepage}");
            return ((Values)CodepageCache[codepage]).desc;
        }

        public static Encoding GetEncoding(int codepage)
        {
            mDebugger.SendInternal($"Getting encoding for codepage {codepage}");
            return ((Values)CodepageCache[codepage]).encoding;
        }

        public static int GetCodePageFromDesc(string desc)
        {
            mDebugger.SendInternal($"Getting codepage for desc {desc}");

            // This will be nicer if we could use Nullable<int> here 
            object val = NamesCache[desc];

            return val != null ? (int)val : -1;
        }
    }
}

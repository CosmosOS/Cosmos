#define COSMOSDEBUG
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

        static EncodingTable()
        {
            mDebugger.SendInternal("Inizializing Encoding Table");

            Add(437, "IBM437", new CP437Enconding());
            Add(858, "IBM0858", new CP858Enconding());
        }

        private struct values
        {
            public string   desc;
            public Encoding encoding;

            public values(string desc, Encoding encoding)
            {
                this.desc = desc;
                this.encoding = encoding;
            }
        };

        const int MaxCodepageChacheSize = 2048;
        static values[] CodepageCache = new values[MaxCodepageChacheSize];

        public static void Add(int codepage, string desc, Encoding encoding)
        {
            mDebugger.SendInternal($"Adding codepage {codepage} desc {desc}");
            CodepageCache[codepage] = new values(desc, encoding);
        }

        public static string GetDescription(int codepage) => CodepageCache[codepage].desc;

        public static Encoding GetEncoding(int codepage) => CodepageCache[codepage].encoding;

        public static int GetCodePageFromDesc(string desc)
        {
            for (int idx = 0; idx < MaxCodepageChacheSize; idx++)
            {
                if (CodepageCache[idx].desc == desc)
                    return idx;
            }

            /* Not found! */
            return -1;
        }
    }
}

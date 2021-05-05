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
    /// <summary>
    /// EncodingTable class. Used to manage codepage list.
    /// </summary>
    internal static class EncodingTable
    {
        /// <summary>
        /// Debugger instance of the "System" ring with the "EncodingTable" tag.
        /// </summary>
        private static Debugger mDebugger = new Debugger("System", "EncodingTable");
        private static Hashtable CodepageCache = new Hashtable();
        private static Hashtable NamesCache = new Hashtable();

        /// <summary>
        /// Create new instance of the <see cref="EncodingTable"/> class.
        /// </summary>
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
        /// <summary>
        /// Struct which used to hold description and encoding.
        /// </summary>
        private struct values
        {
            /// <summary>
            /// Description.
            /// </summary>
            public string   desc;
            /// <summary>
            /// Encoding.
            /// </summary>
            public Encoding encoding;

            /// <summary>
            /// Create new instance of the <see cref="values"/> struct.
            /// </summary>
            /// <param name="desc">Description.</param>
            /// <param name="encoding">Encoding.</param>
            public values(string desc, Encoding encoding)
            {
                this.desc = desc;
                this.encoding = encoding;
            }
        }

        /// <summary>
        /// Max codepage cache size.
        /// </summary>
        const int MaxCodepageChacheSize = 2048;

        /// <summary>
        /// Add encoding to the encoding table.
        /// </summary>
        /// <param name="codepage">Codepage.</param>
        /// <param name="desc">Desciption.</param>
        /// <param name="encoding">Encoding.</param>
        public static void Add(int codepage, string desc, Encoding encoding)
        {
            mDebugger.SendInternal($"Adding codepage {codepage} desc {desc}");
            CodepageCache[codepage] = new Values(desc, encoding);
            NamesCache[desc] = codepage;
            //CodepageCache[codepage] = new values(desc, encoding);
            mDebugger.SendInternal("Done");
        }
      
        /// <summary>
        /// Get description, using codepage.
        /// </summary>
        /// <param name="codepage">Codepage.</param>
        /// <returns>string value.</returns>
        public static string GetDescription(int codepage) => CodepageCache[codepage].desc;

        /// <summary>
        /// Get encoding, using codepage.
        /// </summary>
        /// <param name="codepage">Codepage.</param>
        /// <returns>Encoding value.</returns>
        public static Encoding GetEncoding(int codepage) => CodepageCache[codepage].encoding;

        /// <summary>
        /// Get code page from description.
        /// </summary>
        /// <param name="desc">Description.</param>
        /// <returns>int value, -1 if not found.</returns>
        public static int GetCodePageFromDesc(string desc)
        {
            mDebugger.SendInternal($"Getting codepage for desc {desc}");

            // This will be nicer if we could use Nullable<int> here 
            object val = NamesCache[desc];

            return val != null ? (int)val : -1;
        }
    }
}

//#define COSMOSDEBUG
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /*
     * Ideally we should use Dictionary or HashTable here but are yet not working in Cosmos so I have done
     * this replacement class for now...
     */

    /// <summary>
    /// Used to manage the codepage list.
    /// </summary>
    internal static class EncodingTable
    {
        static readonly Debugger mDebugger = new("System", "EncodingTable");

        /// <summary>
        /// Create new instance of the <see cref="EncodingTable"/> class.
        /// </summary>
        static EncodingTable()
        {
            mDebugger.SendInternal("Initializing the encoding table...");
            Add(437, "IBM437", new CP437Encoding());
            Add(858, "IBM0858", new CP858Enconding());
        }

        /// <summary>
        /// Used to hold the description and encoding.
        /// </summary>
        private struct DescriptionEncodingPair
        {
            public string Description;
            public Encoding Encoding;

            public DescriptionEncodingPair(string desc, Encoding encoding)
            {
                Description = desc;
                Encoding = encoding;
            }
        };

        const int MaxCodepageChacheSize = 2048;
        static readonly DescriptionEncodingPair[] CodepageCache = new DescriptionEncodingPair[MaxCodepageChacheSize];

        /// <summary>
        /// Add encoding to the encoding table.
        /// </summary>
        /// <param name="codepage">Codepage.</param>
        /// <param name="desc">Desciption.</param>
        /// <param name="encoding">Encoding.</param>
        public static void Add(int codepage, string desc, Encoding encoding)
        {
            mDebugger.SendInternal($"Adding codepage {codepage} w/ description {desc}");
            CodepageCache[codepage] = new DescriptionEncodingPair(desc, encoding);
        }

        /// <summary>
        /// Gets the description of the given codepage.
        /// </summary>
        /// <param name="codepage">The codepage.</param>
        public static string GetDescription(int codepage) => CodepageCache[codepage].Description;

        /// <summary>
        /// Gets the encoding of the given codepage.
        /// </summary>
        /// <param name="codepage">The codepage.</param>
        public static Encoding GetEncoding(int codepage) => CodepageCache[codepage].Encoding;

        /// <summary>
        /// Get a code page by its description.
        /// </summary>
        /// <param name="desc">The description.</param>
        /// <returns>The codepage, or -1 if not found.</returns>
        public static int GetCodePageFromDesc(string desc)
        {
            for (int idx = 0; idx < MaxCodepageChacheSize; idx++)
            {
                if (CodepageCache[idx].Description == desc) {
                    return idx;
                }
            }

            return -1;
        }
    }
}

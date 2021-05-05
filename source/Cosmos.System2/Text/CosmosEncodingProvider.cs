#define COSMOSDEBUG
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// CosmosEncodingProvider class. Used to provide <see cref="Encoder"/>, by using its name or codepage. See also: <seealso cref="EncodingProvider"/>.
    /// </summary>
    public class CosmosEncodingProvider : EncodingProvider
    {
        /// <summary>
        /// Encoding provider.
        /// </summary>
        private static readonly EncodingProvider s_singleton = new CosmosEncodingProvider();
        /// <summary>
        /// Debugger instance of the "System" ring with the "CosmosEncodingProvider" tag.
        /// </summary>
        private static Debugger myDebugger = new Debugger("System", "CosmosEncodingProvider");

        /// <summary>
        /// Create new instance of the <see cref="CosmosEncodingProvider"/> class.
        /// </summary>
        internal CosmosEncodingProvider() { }

        /// <summary>
        /// Get CosmosEncodingProvider instance. Returns EncodingProvider.
        /// </summary>
        public static EncodingProvider Instance
        {
            get { return s_singleton; }
        }

        /// <summary>
        /// Get encoding, using its codepage.
        /// </summary>
        /// <param name="codepage">Codepage.</param>
        /// <returns>Encoding value.</returns>
        public override Encoding GetEncoding(int codepage)
        {
            myDebugger.SendInternal($"Getting Encoding for codepage {codepage}");
            if (codepage < 0 || codepage > 65535)
                return null;

            /* Let's check on our EncodingTable, if codepage is not found null is returned */
            return EncodingTable.GetEncoding(codepage);
        }

        /// <summary>
        /// Get encoding, using its name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Encoding value.</returns>
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

//#define COSMOSDEBUG
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// Used to provide an <see cref="Encoder"/> by using its name or codepage.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="EncodingProvider"/>.
    /// </remarks>
    public class CosmosEncodingProvider : EncodingProvider
    {
        static readonly EncodingProvider singleton = new CosmosEncodingProvider();
        static readonly Debugger debugger = new("System", "CosmosEncodingProvider");

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEncodingProvider"/> class.
        /// </summary>
        internal CosmosEncodingProvider() { }

        /// <summary>
        /// Gets the main <see cref="CosmosEncodingProvider"/> instance.
        /// Returns an <see cref="EncodingProvider"/>.
        /// </summary>
        public static EncodingProvider Instance => singleton;

        /// <summary>
        /// Gets an encoding by its codepage.
        /// </summary>
        /// <param name="codepage">The codepage.</param>
        public override Encoding GetEncoding(int codepage)
        {
            debugger.SendInternal($"Getting Encoding for codepage {codepage}");
            if (codepage is < 0 or > 65535)
            {
                return null;
            }

            // Let's check on our EncodingTable, if codepage is not found null is returned
            return EncodingTable.GetEncoding(codepage);
        }

        /// <summary>
        /// Gets an encoding by using its name.
        /// </summary>
        /// <param name="name">The name of the target encoding.</param>
        public override Encoding GetEncoding(string name)
        {
            debugger.SendInternal($"Getting Encoding for codepage with name {name}");
            int codepage = EncodingTable.GetCodePageFromDesc(name);
            return codepage == -1 ? null : GetEncoding(codepage);
        }
    }
}

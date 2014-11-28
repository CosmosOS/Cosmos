using System;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.Debugging.CorSymbolStore 
{
    [
        ComImport,
        Guid("8E3F80CA-7517-432a-BA07-285134AAEA8E"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(true),
        CLSCompliant(false)
    ]
    public interface IDiaReadExeAtRVACallback
    {
        /// <summary>
        /// Reads module data at the specified relative virtual address
        /// </summary>
        /// <param name="relativeVirtualAddress">The RVA to begin reading from</param>
        /// <param name="cbData">The number of bytes of data to read</param>
        /// <param name="pcbData">The number of bytes of data actually read</param>
        /// <param name="data">A buffer of size at least cbData which is filled with the read data</param>
        /// <remarks>DIA would prefer to have the following pseudo implementation of this interface:
        /// 1) If not enough of the file is available to validate image RVAs, throw any exception and return 0 bytes read
        /// 2) else if relativeVirtualAddress is not valid, throw any exception and return 0 bytes read
        /// 3) else if relativeVirtualAddress is valid, but not readable, throw any exception and return 0 bytes read
        /// 4) else if cbData is 0, return with 0 bytes read
        /// 5) else let X be the count of contiguous bytes in the virtual address space starting at relativeVirtualAddress
        ///    that are at valid, readable addresses.
        ///   a) If X >= cbData return S_OK and cbData bytes read.
        ///   b) else if relativeVirtualAddress + X + 1 is an invalid RVA, optionally throw any exception and return X bytes read
        ///   c) else if relativeVirtualAddress + X + 1 is not readable, throw any exception and return X bytes read
        /// </remarks>
        void ReadExecutableAtRVA(
            uint relativeVirtualAddress,
            uint cbData,
            ref uint pcbData,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] data);
    }
}

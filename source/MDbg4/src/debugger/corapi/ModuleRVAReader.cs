using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Samples.Debugging.CorSymbolStore;

namespace Microsoft.Samples.Debugging.CorDebug
{
    /// <summary>
    /// This class tries to read a given module at certain RVAs by assuming
    /// that it is layed out in memory in the way LoadLibrary would place it. This
    /// means that a given RVA can be located in the process memory space merely
    /// by adding the module base address as an offset. In practice the CLR does
    /// use this mapping up through .Net FX 3.5 SP1 and for silverlight 2 for
    /// modules which it loaded from disk. WARNING: Nothing guarantees that the
    /// CLR must do this so this is unreliable going forward.
    /// </summary>
    [CLSCompliant(false)]
    public class ModuleRVAReader : IDiaReadExeAtRVACallback
    {
        CorModule m_module;

        public ModuleRVAReader(CorModule moduleToRead)
        {
            if(moduleToRead == null)
                throw new ArgumentNullException("moduleToRead");
            m_module = moduleToRead;
        }
    
        #region IDiaReadExeAtRVACallback Members



        /// <summary>
        /// Reads module data at the specified relative virtual address
        /// </summary>
        /// <param name="relativeVirtualAddress">The RVA to begin reading from</param>
        /// <param name="cbData">The number of bytes of data to read</param>
        /// <param name="pcbData">The number of bytes of data actually read</param>
        /// <param name="data">A buffer of size at least cbData which is filled with the read data</param>
        /// <remarks>See the interface for the spec on this method. Note that this impl isn't the best
        /// as it doesn't validate reading at relativeVirtualAddress when cbData = 0 and even when it
        /// does validate RVAs, it only does cursory bounds checking. There are dead spots within the
        /// image that are not checked for here (though hopefully reading would fail because the pages
        /// won't be mapped)</remarks>
        void IDiaReadExeAtRVACallback.ReadExecutableAtRVA(uint relativeVirtualAddress, uint cbData,
            ref uint pcbData, byte[] data)
        {
            pcbData = 0;

            // validate RVA
 	        if(relativeVirtualAddress > m_module.Size)
                throw new ArgumentOutOfRangeException("relativeVirtualAddress");
            // validate data
            if(data == null)
                throw new ArgumentNullException("data");
            if(data.Length < cbData)
                throw new ArgumentException("data");

            // truncate read if it would otherwise extend into invalid RVA range
            uint bytesToRead = cbData;
            checked
            {
                if(cbData + relativeVirtualAddress > m_module.Size)
                {
                    // we know bytesToRead is >= 0 because of the first check above
                    bytesToRead = (uint)m_module.Size - relativeVirtualAddress;
                }
            }

            // early bail out if not reading any bytes
            if(bytesToRead == 0)
            {
                // if we read nothing because of clipping the range then throw otherwise return normally
                if(bytesToRead != cbData)
                    throw new ArgumentOutOfRangeException("cbData");
                else
                    return;
            }

            // read the data
            // although we are allowed to do a partial read I think it is safer not to. If this winds up
            // being some sort of perf issue we can revist this, but I doubt it is a problem
            uint bytesRead = 0;
            long bytesLastRead = 0;
            try
            {
                do
                {
                    Debug.Assert(bytesRead < bytesToRead);
                    uint bytesLeft = bytesToRead - bytesRead;
                    byte[] buffer = new byte[bytesLeft];
                    bytesLastRead = m_module.Process.ReadMemory(relativeVirtualAddress + m_module.BaseAddress, buffer);
                    Array.Copy(buffer, 0, data, bytesRead, bytesLastRead);
                    bytesRead += (uint)bytesLastRead;
                } while( bytesRead < bytesToRead && bytesLastRead != 0);
            }
            finally
            {
                pcbData = bytesRead;
            }

            // if we did not read everything because of clipping the range then throw
            if(pcbData != cbData)
                throw new ArgumentOutOfRangeException("cbData");
        }

        #endregion
    }
}

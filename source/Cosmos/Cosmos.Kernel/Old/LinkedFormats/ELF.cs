using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Cosmos.Kernel.LinkedFormats
{
    /// <summary>
    /// Allows an ELF file to be loaded.
    /// </summary>
    public class ELF
    {
        /// <summary>
        /// Loads an ELF file into the next available position in memory.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The starting position for the code.</returns>
        public long Load(Stream reader)
        {
            if (reader.CanRead == false)
                throw new InvalidOperationException("Stream provided is not readable.");

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="position"></param>
        /// <returns>Whether or not the code was loaded.</returns>
        public bool Load(Stream reader, long position)
        {
            return false;
        }
    }
}

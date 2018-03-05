using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Drivers.Storage
{
    /// <summary>
    /// Represents a file system.
    /// </summary>
    public abstract class FileSystem
    {
        /// <summary>
        /// Determines if the specified device
        /// implements this filesystem.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract bool Identify(Storage target);

        /// <summary>
        /// Reads the specified bytes.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public abstract long Read(byte[] buffer, long offset, long length, params string[] filename);

        /// <summary>
        /// Writes out bytes.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public abstract long Write(byte[] buffer, long offset, long length, params string[] filename);

    }
}

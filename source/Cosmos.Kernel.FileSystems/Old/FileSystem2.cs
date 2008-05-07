using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystem
{
    /// <summary>
    /// Represents a filesystem with well defined interfaces.
    /// </summary>
    public abstract class FileSystem2 : FileSystem
    {
        public abstract string Label
        {
            get;
            set;
        }

        /// <summary>
        /// Open the drive for reading.
        /// </summary>
        /// <remarks>
        /// Writing should not be supported in the kernel.
        /// </remarks>
        public abstract void Open();

        /// <summary>
        /// Closes the file system for reading.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets the files in a specific directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>A list of files.</returns>
        public abstract File[] GetFiles(Path path);

        /// <summary>
        /// Reads data from a specific file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="data">The buffer in which to place the data.</param>
        /// <param name="start">The starting offset in the buffer.</param>
        /// <param name="count">The amount of bytes to read.</param>
        /// <returns></returns>
        public abstract int ReadData(Path path, byte[] data, int start, int count);
    }
}

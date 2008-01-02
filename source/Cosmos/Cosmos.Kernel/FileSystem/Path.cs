using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystem
{
    /// <summary>
    /// Represents a path in a filesystem agnostic fashion.
    /// </summary>
    public class Path
    {
        private string[] _parts;
        /// <summary>
        /// Gets the steps.
        /// </summary>
        public string[] Parts
        {
            get
            {
                return _parts;
            }
        }

        /// <summary>
        /// Creates a new path.
        /// </summary>
        /// <param name="args">Each step in getting to the file.</param>
        public Path(params string[] parts)
        {
            _parts = parts;
        }
    }
}

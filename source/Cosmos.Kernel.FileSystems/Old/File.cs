using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystem
{
    /// <summary>
    /// Represents a single file.
    /// </summary>
    public class File
    {
        private string _name;
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Drivers.Storage
{
    /// <summary>
    /// Represents a file system.
    /// </summary>
    public abstract class Storage : Driver
    {
        public abstract void Read(byte[] data, long offset, long count);
        public abstract void Write(byte[] data, long offset, long count);

        public void Write(byte[] data, long offset)
        {
            Write(data, offset, data.Length);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.Listing
{
    public abstract class Base
    {
        public readonly FileSystem FileSystem;
        public readonly string Name;

        public readonly Directory BaseDirectory;

        protected Base(FileSystem aFileSystem, string aName, Directory baseDirectory)
        {
            FileSystem = aFileSystem;
            Name = aName;
            BaseDirectory = baseDirectory;
        }

        // Size might be updated in an ancestor destructor or on demand,
        // so its not a readonly field
        protected UInt64 mSize;
        public UInt64 Size
        {
            get { return mSize; }
            set
            {
                mSize = value;
            }
        }
    }
}

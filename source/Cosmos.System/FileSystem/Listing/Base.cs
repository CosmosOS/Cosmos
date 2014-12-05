//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Cosmos.System.FileSystem.Listing
//{
//    public abstract class Base
//    {
//        public readonly FileSystem FileSystem;
//        public readonly string Name;

//        protected Base(FileSystem aFileSystem, string aName)
//        {
//            FileSystem = aFileSystem;
//            Name = aName;
//        }

//        // Size might be updated in an ancestor destructor or on demand,
//        // so its not a readonly field
//        protected UInt64 mSize;
//        public UInt64 Size
//        {
//            get { return mSize; }
//        }
//    }
//}
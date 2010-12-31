using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using System.IO;

namespace TestConsoleApp
{
    public class ReferenceImpl: ITaskItem
    {
        private Dictionary<string, string> mMetadata;
        public ReferenceImpl(string file)
        {
            mMetadata = new Dictionary<string, string>();
            mMetadata.Add("FullPath", Path.GetFullPath(file));
        }
        public System.Collections.IDictionary CloneCustomMetadata()
        {
            throw new NotImplementedException();
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            throw new NotImplementedException();
        }

        public string GetMetadata(string metadataName)
        {
            return mMetadata[metadataName];
        }

        public string ItemSpec
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int MetadataCount
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ICollection MetadataNames
        {
            get
            {
                return mMetadata.Keys;
            }
        }

        public void RemoveMetadata(string metadataName)
        {
            throw new NotImplementedException();
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    internal class BuildFileUtils
    {

        internal void CopyFile(string aFrom, string aTo)
        {
            string xDir = Path.GetDirectoryName(aTo);
            if (!Directory.Exists(xDir))
            {
                Directory.CreateDirectory(xDir);
            }
            File.Copy(aFrom, aTo);
        }

        internal void RemoveFile(string aPathname)
        {
            if (File.Exists(aPathname))
            {
                RemoveReadOnlyAttribute(aPathname);
                File.Delete(aPathname);
            }
        }


        internal void RemoveReadOnlyAttribute(string aPathname)
        {
            var xAttribs = File.GetAttributes(aPathname);
            if ((xAttribs & FileAttributes.ReadOnly) > 0)
            {
                // This works because we only do this if Read only is already set
                File.SetAttributes(aPathname, xAttribs ^ FileAttributes.ReadOnly);
            }
        }

    }
}

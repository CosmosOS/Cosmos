using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using SentinelKernel.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    [Plug(Target=typeof(Path))]
    public static class PathImpl {


        /// <summary>
        /// Get the directory part of a path to a file. No trailing slash in returned string.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string aPath)
        {
            if (aPath == null || aPath.Length <= 1)
            {
                return "/";
            }
            int xIndex = aPath.LastIndexOfAny(new char[] { '/', '\\' });
            if (xIndex == -1)
            {
                return aPath;
            }
            return aPath.Substring(0, xIndex);
        }

        public static void Cctor(
            [FieldAccess(Name = "System.Char System.IO.Path.DirectorySeparatorChar")]
            ref char aDirectorySeparatorChar,
            [FieldAccess(Name = "System.Char[] System.IO.Path.InvalidPathCharsWithAdditionalChecks")]
            ref char[] aInvalidPathCharsWithAdditionalChecks,
            [FieldAccess(Name = "System.Char[] System.IO.Path.RealInvalidPathChars")]
            ref char[] aRealInvalidPathChars)
        {
            aDirectorySeparatorChar = '\\';
            aInvalidPathCharsWithAdditionalChecks = new[] {'"'};
            aRealInvalidPathChars = new[] { '"' };
        }

        //public static string GetFileName(string aPath)
        //{
        //    int xIndex = aPath.LastIndexOfAny(new char[] { '/', '\\' });
        //    if (xIndex == -1)
        //    {
        //        return aPath;
        //    }
        //    return aPath.Substring(xIndex + 1, aPath.Length - xIndex - 1);
        //}

        //public static string GetPathRoot(string aPath)
        //{
        //    if (aPath.IsAbsolutePath())
        //        return new String(new char[] { aPath[0], aPath[1], aPath[2] });
        //    else
        //        return String.Empty;
        //}

        //public static bool IsPathRooted(string aPath)
        //{
        //    return aPath.IsAbsolutePath();
        //}

        public static string GetFullPath(string aPath)
        {
            //Plug is used to avoid call to FileIOPermission
            return GetFullPathInternal(aPath);
        }

        public static string GetFullPathInternal(string aPath)
        {
            //Exact copy of .NET's version of GetFullPathInternal
            if (aPath == null)
            {
                throw new ArgumentNullException("path");
            }
            return NormalizePath(aPath, true);
        }

        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            if (aPath.IsRelativePath())
                return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + aPath;
            else
                return aPath.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        }

        public static string GetRandomFileName()
        {
            return "random.tmp";
        }

        public static string GetTempFileName()
        {
            return "\0\tempfile.tmp";
        }

        public static string GetTempPath()
        {
            return @"\0\Temp";
        }
    }
}

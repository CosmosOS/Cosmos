using System;
using System.IO;

using Cosmos.Common;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(Path))]
    public static class PathImpl
    {
#if false
        public static void Cctor(
      //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidFileNameChars")] ref char[] aInvalidFileNameChars,
      //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidPathCharsWithAdditionalChecks")] ref char[] aInvalidPathCharsWithAdditionalChecks,
      //[FieldAccess(Name = "System.Char System.IO.Path.PathSeparator")] ref char aPathSeparator,
      //[FieldAccess(Name = "System.Char[] System.IO.Path.RealInvalidPathChars")] ref char[] aRealInvalidPathChars,
      //[FieldAccess(Name = "System.Int32 System.IO.Path.MaxPath")] ref int aMaxPath
      [FieldAccess(Name = "System.Char System.IO.Path.AltDirectorySeparatorChar")] ref char
          aAltDirectorySeparatorChar,
      [FieldAccess(Name = "System.Char System.IO.Path.DirectorySeparatorChar")] ref char aDirectorySeparatorChar,
      [FieldAccess(Name = "System.Char System.IO.Path.VolumeSeparatorChar")] ref char aVolumeSeparatorChar)
        {
            //aInvalidFileNameChars = VFSManager.GetInvalidFileNameChars();
            //aInvalidPathCharsWithAdditionalChecks = VFSManager.GetInvalidPathCharsWithAdditionalChecks();
            //aPathSeparator = VFSManager.GetPathSeparator();
            //aRealInvalidPathChars = VFSManager.GetRealInvalidPathChars();
            //aMaxPath = VFSManager.GetMaxPath();
            aAltDirectorySeparatorChar = VFSManager.GetAltDirectorySeparatorChar();
            aDirectorySeparatorChar = VFSManager.GetDirectorySeparatorChar();
            aVolumeSeparatorChar = VFSManager.GetVolumeSeparatorChar();
        }
#endif

        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            // For now let's return the Path not normalized
	        return aPath;
        }

        public static string NormalizePath(string path, bool fullCheck, int maxPathLength, bool expandShortPaths)
        {
            return path;
        }

        public static string GetTempPath()
        {
            return @"\Temp";
        }

        public static char[] GetInvalidFileNameChars()
        {
            return VFSManager.GetInvalidFileNameChars();
        }

        public static char[] GetInvalidPathChars()
        {
            return VFSManager.GetRealInvalidPathChars();
        }

        public static string GetTempFileName()
        {
            // We return always the same value for now we need Random to work to get real random values
            return "tempfile.tmp";
        }

        public static string GetRandomFileName()
        {
            // We return always the same value for now we need Random to work to get real random values
            return "random.tmp";
        }

        public static string GetFullPath(string path)
        {
            return NormalizePath(path, true);
        }

    }
}

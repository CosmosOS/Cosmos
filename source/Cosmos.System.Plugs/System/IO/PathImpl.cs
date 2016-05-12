//define COSMOSDEBUG

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
        private static readonly Debugger mDebugger = new Debugger("System", "Path Plugs");
        public static void Cctor(
            //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidFileNameChars")] ref char[] aInvalidFileNameChars,
            [FieldAccess(Name = "System.Char[] System.IO.Path.InvalidPathCharsWithAdditionalChecks")] ref char[] aInvalidPathCharsWithAdditionalChecks,
            //[FieldAccess(Name = "System.Char System.IO.Path.PathSeparator")] ref char aPathSeparator,
            [FieldAccess(Name = "System.Char[] System.IO.Path.RealInvalidPathChars")] ref char[] aRealInvalidPathChars,
            //[FieldAccess(Name = "System.Int32 System.IO.Path.MaxPath")] ref int aMaxPath
            [FieldAccess(Name = "System.Char System.IO.Path.AltDirectorySeparatorChar")] ref char aAltDirectorySeparatorChar,
            [FieldAccess(Name = "System.Char System.IO.Path.DirectorySeparatorChar")] ref char aDirectorySeparatorChar,
            [FieldAccess(Name = "System.Char System.IO.Path.VolumeSeparatorChar")] ref char aVolumeSeparatorChar)
        {
            //aInvalidFileNameChars = VFSManager.GetInvalidFileNameChars();
            aInvalidPathCharsWithAdditionalChecks = VFSManager.GetInvalidPathCharsWithAdditionalChecks();
            //aPathSeparator = VFSManager.GetPathSeparator();
            aRealInvalidPathChars = VFSManager.GetRealInvalidPathChars();
            //aMaxPath = VFSManager.GetMaxPath();
            aAltDirectorySeparatorChar = VFSManager.GetAltDirectorySeparatorChar()[0];
            aDirectorySeparatorChar = VFSManager.GetDirectorySeparatorChar()[0];
            aVolumeSeparatorChar = VFSManager.GetVolumeSeparatorChar();
        }

        //public static string ChangeExtension(string aPath, string aExtension)
        //{
        //    if (aPath != null)
        //    {
        //        CheckInvalidPathChars(aPath, false);
        //        string xText = aPath;
        //        int xNum = aPath.Length;
        //        while (--xNum >= 0)
        //        {
        //            char xC = aPath[xNum];
        //            if (xC == '.')
        //            {
        //                xText = aPath.Substring(0, xNum);
        //                break;
        //            }
        //            if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
        //                || xC == Path.VolumeSeparatorChar)
        //            {
        //                break;
        //            }
        //        }
        //        if (aExtension != null && aPath.Length != 0)
        //        {
        //            if (aExtension.Length == 0 || aExtension[0] != '.')
        //            {
        //                xText += ".";
        //            }
        //            xText += aExtension;
        //        }
        //        Global.mFileSystemDebugger.SendInternal($"Path.ChangeExtension : aPath = {aPath}, aExtension = {aExtension}, returning {xText}");
        //        return xText;
        //    }
        //    return null;
        //}

        //public static void CheckInvalidPathChars(string aPath, bool aCheckAdditional)
        //{
        //    if (aPath == null)
        //    {
        //        throw new ArgumentNullException("aPath");
        //    }

        //    if (HasIllegalCharacters(aPath, aCheckAdditional))
        //    {
        //        throw new ArgumentException("The path contains invalid characters.", "aPath");
        //    }
        //}

        //public static void CheckSearchPattern(string aSearchPattern)
        //{
        //    int xNum;
        //    while ((xNum = aSearchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
        //    {
        //        if (xNum + 2 == aSearchPattern.Length)
        //        {
        //            throw new ArgumentException("The search pattern is invalid.", aSearchPattern);
        //        }

        //        if (aSearchPattern[xNum + 2] == Path.DirectorySeparatorChar
        //            || aSearchPattern[xNum + 2] == Path.AltDirectorySeparatorChar)
        //        {
        //            throw new ArgumentException("The search pattern is invalid.", aSearchPattern);
        //        }

        //        aSearchPattern = aSearchPattern.Substring(xNum + 2);
        //    }
        //}

        //public static string Combine(string aPath1, string aPath2)
        //{
        //    if (aPath1 == null || aPath2 == null)
        //    {
        //        throw new ArgumentNullException((aPath1 == null) ? "path1" : "path2");
        //    }

        //    CheckInvalidPathChars(aPath1, false);
        //    CheckInvalidPathChars(aPath2, false);
        //    string result = CombineNoChecks(aPath1, aPath2);
        //    Global.mFileSystemDebugger.SendInternal($"Path.Combine : aPath1 = {aPath1}, aPath2 = {aPath2}, returning {result}");
        //    return result;
        //}

        //public static string CombineNoChecks(string aPath1, string aPath2)
        //{
        //    if (aPath2.Length == 0)
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.CombineNoChecks : aPath2 has 0 length, returning {aPath1}");
        //        return aPath1;
        //    }

        //    if (aPath1.Length == 0)
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.CombineNoChecks : aPath1 has 0 length, returning {aPath2}");
        //        return aPath2;
        //    }

        //    if (IsPathRooted(aPath2))
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.CombineNoChecks : aPath2 is root, returning {aPath2}");
        //        return aPath2;
        //    }

        //    string xResult = string.Empty;
        //    char xC = aPath1[aPath1.Length - 1];
        //    if (xC != Path.DirectorySeparatorChar && xC != Path.AltDirectorySeparatorChar
        //        && xC != Path.VolumeSeparatorChar)
        //    {
        //        xResult = string.Concat(aPath1, "\\", aPath2);
        //        Global.mFileSystemDebugger.SendInternal($"Path.CombineNoChecks : aPath1 = {aPath1}, aPath2 = {aPath2}, returning {xResult}");
        //        return xResult;
        //    }

        //    xResult = string.Concat(aPath1, aPath2);
        //    Global.mFileSystemDebugger.SendInternal($"Path.CombineNoChecks : aPath1 = {aPath1}, aPath2 = {aPath2}, returning {xResult}");
        //    return xResult;
        //}

        //public static string GetDirectoryName(string aPath)
        //{
        //    if (aPath != null)
        //    {
        //        CheckInvalidPathChars(aPath, false);
        //        string xPath = NormalizePath(aPath, false);
        //        int xRootLength = GetRootLength(xPath);
        //        int xNum = xPath.Length;
        //        Global.mFileSystemDebugger.SendInternal($"Path.GetDirectoryName : aPath = {aPath}, xRootLength = {xRootLength}, xPathLength = {xNum}");

        //        // If length of aPath is the same of the lenght of the Root Path is the root Path itself!
        //        if (xNum == xRootLength)
        //        {
        //            Global.mFileSystemDebugger.SendInternal($"Path.GetDirectoryName : aPath = {aPath}, is the root directory");
        //            return null;
        //        }

        //        if (xNum > xRootLength)
        //        {
        //            while (xNum > xRootLength && xPath[--xNum] != Path.DirectorySeparatorChar
        //                   && xPath[xNum] != Path.AltDirectorySeparatorChar)
        //            {
        //            }
        //        }
        //        string result = xPath.Substring(0, xNum);
        //        Global.mFileSystemDebugger.SendInternal($"Path.GetDirectoryName : aPath = {aPath}, returning {result}");
        //        return result;
        //    }

        //    Global.mFileSystemDebugger.SendInternal($"Path.GetDirectoryName : aPath is null");
        //    return null;
        //}

        //public static string GetExtension(string aPath)
        //{
        //    if (aPath == null)
        //    {
        //        return null;
        //    }

        //    CheckInvalidPathChars(aPath, false);
        //    int xLength = aPath.Length;
        //    int xNum = xLength;
        //    while (--xNum >= 0)
        //    {
        //        char xC = aPath[xNum];
        //        if (xC == '.')
        //        {
        //            if (xNum != xLength - 1)
        //            {
        //                return aPath.Substring(xNum + 1, xLength - xNum);
        //            }

        //            return string.Empty;
        //        }

        //        if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
        //            || xC == Path.VolumeSeparatorChar)
        //        {
        //            break;
        //        }
        //    }
        //    return string.Empty;
        //}

        //public static string GetFileName(string aPath)
        //{
        //    Global.mFileSystemDebugger.SendInternal($"Path.GetFileName : aPath = {aPath}");
        //    if (aPath != null)
        //    {
        //        CheckInvalidPathChars(aPath, false);
        //        int xLength = aPath.Length;
        //        int xNum = xLength;
        //        while (--xNum >= 0)
        //        {
        //            char xC = aPath[xNum];
        //            if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
        //                || xC == Path.VolumeSeparatorChar)
        //            {
        //                return aPath.Substring(xNum + 1, xLength - xNum - 1);
        //            }
        //        }
        //    }

        //    Global.mFileSystemDebugger.SendInternal($"Path.GetFileName : returning {aPath}");
        //    return aPath;
        //}

        //public static string GetFileNameWithoutExtension(string aPath)
        //{
        //    Global.mFileSystemDebugger.SendInternal($"Path.GetFileNameWithoutExtension : aPath = {aPath}");

        //    aPath = GetFileName(aPath);
        //    if (aPath == null)
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.GetFileNameWithoutExtension : returning null");
        //        return null;
        //    }
        //    int xLength;
        //    if ((xLength = aPath.LastIndexOf('.')) == -1)
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.GetFileNameWithoutExtension : returning {aPath}");
        //        return aPath;
        //    }

        //    string xResult = aPath.Substring(0, xLength);
        //    Global.mFileSystemDebugger.SendInternal($"Path.GetFileNameWithoutExtension : returning {xResult}");

        //    return xResult;
        //}

        public static string GetFullPath(string aPath)
        {
            if (aPath == null)
            {
                Global.mFileSystemDebugger.SendInternal($"Path.GetFullPath : aPath is null");
                throw new ArgumentNullException("aPath");
            }

            string result = NormalizePath(aPath, true);
            Global.mFileSystemDebugger.SendInternal($"Path.GetFullPath : aPath = {aPath}, returning {result}");
            return result;
        }

        public static char[] GetInvalidFileNameChars()
        {
            return VFSManager.GetInvalidFileNameChars();
        }

        public static char[] GetInvalidPathChars()
        {
            return VFSManager.GetRealInvalidPathChars();
        }

        //public static string GetPathRoot(string aPath)
        //{
        //    if (aPath == null)
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.GetPathRoot : aPath is null");
        //        throw new ArgumentNullException("aPath");
        //    }

        //    aPath = NormalizePath(aPath, false);
        //    int xRootLength = GetRootLength(aPath);
        //    string xResult = aPath.Substring(0, xRootLength);
        //    if (xResult[xResult.Length - 1] != Path.DirectorySeparatorChar)
        //    {
        //        xResult = string.Concat(xResult, Path.DirectorySeparatorChar.ToString());
        //    }

        //    Global.mFileSystemDebugger.SendInternal($"Path.GetPathRoot : aPath = {aPath}, xResult = {xResult}");
        //    return xResult;
        //}

        public static string GetRandomFileName()
        {
            return "random.tmp";
        }

        //public static int GetRootLength(string aPath)
        //{
        //    if (aPath == null)
        //    {
        //        Global.mFileSystemDebugger.SendInternal($"Path.GetRootLength : aPath is null");
        //        throw new ArgumentNullException("aPath");
        //    }

        //    CheckInvalidPathChars(aPath, false);
        //    int i = 0;
        //    int length = aPath.Length;
        //    if (length >= 1 && IsDirectorySeparator(aPath[0]))
        //    {
        //        i = 1;
        //        if (length >= 2 && IsDirectorySeparator(aPath[1]))
        //        {
        //            i = 2;
        //            int num = 2;
        //            while (i < length)
        //            {
        //                if ((aPath[i] == Path.DirectorySeparatorChar || aPath[i] == Path.AltDirectorySeparatorChar)
        //                    && --num <= 0)
        //                {
        //                    break;
        //                }
        //                i++;
        //            }
        //        }
        //    }
        //    else if (length >= 2 && aPath[1] == Path.VolumeSeparatorChar)
        //    {
        //        i = 2;
        //        if (length >= 3 && IsDirectorySeparator(aPath[2]))
        //        {
        //            i++;
        //        }
        //    }

        //    Global.mFileSystemDebugger.SendInternal($"Path.GetRootLength : returning {i}");
        //    return i;
        //}

        public static string GetTempFileName()
        {
            return "tempfile.tmp";
        }

        public static string GetTempPath()
        {
            return @"\Temp";
        }

        public static string RemoveLongPathPrefix(string aPath)
        {
            return aPath;
        }

        //public static bool HasExtension(string aPath)
        //{
        //    if (aPath != null)
        //    {
        //        CheckInvalidPathChars(aPath, false);
        //        int xNum = aPath.Length;
        //        while (--xNum >= 0)
        //        {
        //            char xC = aPath[xNum];
        //            if (xC == '.')
        //            {
        //                return xNum != aPath.Length - 1;
        //            }

        //            if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
        //                || xC == Path.VolumeSeparatorChar)
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    return false;
        //}

        //public static bool HasIllegalCharacters(string aPath, bool aCheckAdditional)
        //{
        //    if (aCheckAdditional)
        //    {
        //        char[] xInvalidWithAdditional = VFSManager.GetInvalidPathCharsWithAdditionalChecks();
        //        for (int i = 0; i < xInvalidWithAdditional.Length; i++)
        //        {
        //            for (int j = 0; j < aPath.Length; j++)
        //            {
        //                if (xInvalidWithAdditional[i] == aPath[j])
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }

        //    char[] xInvalid = VFSManager.GetRealInvalidPathChars();
        //    for (int i = 0; i < xInvalid.Length; i++)
        //    {
        //        for (int j = 0; j < aPath.Length; j++)
        //        {
        //            if (xInvalid[i] == aPath[j])
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        public static bool IsDirectorySeparator(char aC)
        {
            if (aC.ToString() == Path.DirectorySeparatorChar.ToString())
            {
                return true;
            }

            if (aC.ToString() == Path.AltDirectorySeparatorChar.ToString())
            {
                return true;
            }

            return false;
        }

        //public static bool IsPathRooted(string aPath)
        //{
        //    if (aPath != null)
        //    {
        //        CheckInvalidPathChars(aPath, false);
        //        int xLength = aPath.Length;
        //        if ((xLength >= 1
        //             && (aPath[0] == Path.DirectorySeparatorChar || aPath[0] == Path.AltDirectorySeparatorChar))
        //            || (xLength >= 2 && aPath[1] == Path.VolumeSeparatorChar))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private static bool IsRelative(string aPath)
        {
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length < 3)
            {
                return true;
            }

            if (aPath[1] != Path.VolumeSeparatorChar)
            {
                return true;
            }

            if (aPath[2] != Path.DirectorySeparatorChar)
            {
                return true;
            }

            return false;
        }

        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            if (aPath == null)
            {
                Global.mFileSystemDebugger.SendInternal($"Path.NormalizePath : aPath is null");
                throw new ArgumentNullException("aPath");
            }

            string result = aPath;
            if (IsRelative(result))
            {
                result = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + result;
                Global.mFileSystemDebugger.SendInternal($"Path.NormalizePath : aPath is relative, aPath = {aPath}, result = {result}");
            }

            if (IsDirectorySeparator(result[result.Length - 1]))
            {
                Global.mFileSystemDebugger.SendInternal($"Path.NormalizePath : Found directory seprator");
                if (result.Length > 3)
                {
                    result = result.Remove(result.Length - 1);
                }
            }

            Global.mFileSystemDebugger.SendInternal($"Path.NormalizePath : aPath = {aPath}, returning {result}");
            return result;
        }
    }
}

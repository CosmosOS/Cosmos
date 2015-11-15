using global::System;
using global::System.IO;

using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(Path))]
    public static class PathImpl
    {
        public static void Cctor(
            //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidFileNameChars")] ref char[] aInvalidFileNameChars,
            //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidPathCharsWithAdditionalChecks")] ref char[] aInvalidPathCharsWithAdditionalChecks,
            //[FieldAccess(Name = "System.Char System.IO.Path.PathSeparator")] ref char aPathSeparator,
            //[FieldAccess(Name = "System.Char[] System.IO.Path.RealInvalidPathChars")] ref char[] aRealInvalidPathChars,
            //[FieldAccess(Name = "System.Int32 System.IO.Path.MaxPath")] ref int aMaxPath
            [FieldAccess(Name = "System.Char System.IO.Path.AltDirectorySeparatorChar")] ref char aAltDirectorySeparatorChar,
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

        public static string ChangeExtension(string aPath, string aExtension)
        {
            FatHelpers.Debug("-- Path.ChangeExtension --");

            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                string xText = aPath;
                int xNum = aPath.Length;
                while (--xNum >= 0)
                {
                    char xC = aPath[xNum];
                    if (xC == '.')
                    {
                        xText = aPath.Substring(0, xNum);
                        break;
                    }
                    if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar || xC == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
                if (aExtension != null && aPath.Length != 0)
                {
                    if (aExtension.Length == 0 || aExtension[0] != '.')
                    {
                        xText += ".";
                    }
                    xText += aExtension;
                }
                return xText;
            }
            return null;
        }

        public static void CheckInvalidPathChars(string aPath, bool aCheckAdditional)
        {
            FatHelpers.Debug("-- Path.CheckInvalidPathChars --");

            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (HasIllegalCharacters(aPath, aCheckAdditional))
            {
                throw new ArgumentException("The path contains invalid characters.", "aPath");
            }
        }

        public static void CheckSearchPattern(string aSearchPattern)
        {
            FatHelpers.Debug("-- Path.CheckSearchPattern --");

            int xNum;
            while ((xNum = aSearchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
            {
                if (xNum + 2 == aSearchPattern.Length)
                {
                    throw new ArgumentException("The search pattern is invalid.", aSearchPattern);
                }

                if (aSearchPattern[xNum + 2] == Path.DirectorySeparatorChar || aSearchPattern[xNum + 2] == Path.AltDirectorySeparatorChar)
                {
                    throw new ArgumentException("The search pattern is invalid.", aSearchPattern);
                }

                aSearchPattern = aSearchPattern.Substring(xNum + 2);
            }
        }

        public static string Combine(string aPath1, string aPath2)
        {
            FatHelpers.Debug("-- Path.Combine --");

            if (aPath1 == null || aPath2 == null)
            {
                throw new ArgumentNullException((aPath1 == null) ? "path1" : "path2");
            }

            CheckInvalidPathChars(aPath1, false);
            CheckInvalidPathChars(aPath2, false);
            return CombineNoChecks(aPath1, aPath2);
        }

        public static string CombineNoChecks(string aPath1, string aPath2)
        {
            FatHelpers.Debug("-- Path.CombineNoChecks --");

            if (aPath2.Length == 0)
            {
                return aPath1;
            }

            if (aPath1.Length == 0)
            {
                return aPath2;
            }

            if (IsPathRooted(aPath2))
            {
                return aPath2;
            }

            char xC = aPath1[aPath1.Length - 1];
            if (xC != Path.DirectorySeparatorChar && xC != Path.AltDirectorySeparatorChar && xC != Path.VolumeSeparatorChar)
            {
                return aPath1 + "\\" + aPath2;
            }

            return aPath1 + aPath2;
        }

        public static string GetDirectoryName(string aPath)
        {
            FatHelpers.Debug("-- Path.GetDirectoryName --");

            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                string xPath = NormalizePath(aPath, false);
                int xRootLength = GetRootLength(xPath);
                int xNum = xPath.Length;
                if (xNum > xRootLength)
                {
                    xNum = xPath.Length;
                    if (xNum == xRootLength)
                    {
                        return null;
                    }

                    while (xNum > xRootLength && xPath[--xNum] != Path.DirectorySeparatorChar
                           && xPath[xNum] != Path.AltDirectorySeparatorChar)
                    {
                    }

                    return xPath.Substring(0, xNum);
                }
            }

            return null;
        }

        public static string GetExtension(string aPath)
        {
            FatHelpers.Debug("-- Path.GetExtension --");

            if (aPath == null)
            {
                return null;
            }

            CheckInvalidPathChars(aPath, false);
            int xLength = aPath.Length;
            int xNum = xLength;
            while (--xNum >= 0)
            {
                char xC = aPath[xNum];
                if (xC == '.')
                {
                    if (xNum != xLength - 1)
                    {
                        return aPath.Substring(xNum, xLength - xNum);
                    }

                    return string.Empty;
                }

                if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar || xC == Path.VolumeSeparatorChar)
                {
                    break;
                }
            }
            return string.Empty;
        }

        public static string GetFileName(string aPath)
        {
            FatHelpers.Debug("-- Path.GetFileName --");

            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                int xLength = aPath.Length;
                int xNum = xLength;
                while (--xNum >= 0)
                {
                    char xC = aPath[xNum];
                    if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
                        || xC == Path.VolumeSeparatorChar)
                    {
                        return aPath.Substring(xNum + 1, xLength - xNum - 1);
                    }
                }
            }

            return aPath;
        }

        public static string GetFileNameWithoutExtension(string aPath)
        {
            FatHelpers.Debug("-- Path.GetFileNameWithoutExtension --");

            aPath = GetFileName(aPath);
            if (aPath == null)
            {
                return null;
            }
            int xLength;
            if ((xLength = aPath.LastIndexOf('.')) == -1)
            {
                return aPath;
            }
            return aPath.Substring(0, xLength);
        }

        public static string GetFullPath(string aPath)
        {
            FatHelpers.Debug("-- Path.GetFullPath --");

            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            return NormalizePath(aPath, true);
        }

        public static char[] GetInvalidFileNameChars()
        {
            FatHelpers.Debug("-- Path.GetInvalidFileNameChars --");

            return VFSManager.GetInvalidFileNameChars();
        }

        public static char[] GetInvalidPathChars()
        {
            FatHelpers.Debug("-- Path.GetInvalidPathChars --");

            return VFSManager.GetRealInvalidPathChars();
        }

        public static string GetPathRoot(string aPath)
        {
            FatHelpers.Debug("-- Path.GetPathRoot --");

            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            aPath = NormalizePath(aPath, false);
            int xRootLength = GetRootLength(aPath);
            string xResult = aPath.Substring(0, xRootLength);
            if (xResult[xResult.Length - 1] != Path.DirectorySeparatorChar)
            {
                xResult = string.Concat(xResult, Path.DirectorySeparatorChar);
            }

            return xResult;
        }

        public static string GetRandomFileName()
        {
            FatHelpers.Debug("-- Path.GetRandomFileName --");

            return "random.tmp";
        }

        public static int GetRootLength(string aPath)
        {
            FatHelpers.Debug("-- Path.GetRootLength --");

            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            CheckInvalidPathChars(aPath, false);
            int i = 0;
            int length = aPath.Length;
            if (length >= 1 && IsDirectorySeparator(aPath[0]))
            {
                i = 1;
                if (length >= 2 && IsDirectorySeparator(aPath[1]))
                {
                    i = 2;
                    int num = 2;
                    while (i < length)
                    {
                        if ((aPath[i] == Path.DirectorySeparatorChar || aPath[i] == Path.AltDirectorySeparatorChar) && --num <= 0)
                        {
                            break;
                        }
                        i++;
                    }
                }
            }
            else if (length >= 2 && aPath[1] == Path.VolumeSeparatorChar)
            {
                i = 2;
                if (length >= 3 && IsDirectorySeparator(aPath[2]))
                {
                    i++;
                }
            }

            return i;
        }

        public static string GetTempFileName()
        {
            FatHelpers.Debug("-- Path.GetTempFileName --");

            return "tempfile.tmp";
        }

        public static string GetTempPath()
        {
            FatHelpers.Debug("-- Path.GetTempPath --");

            return @"\Temp";
        }

        public static bool HasExtension(string aPath)
        {
            FatHelpers.Debug("-- Path.HasExtension --");
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                int xNum = aPath.Length;
                while (--xNum >= 0)
                {
                    char xC = aPath[xNum];
                    if (xC == '.')
                    {
                        return xNum != aPath.Length - 1;
                    }

                    if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
                        || xC == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
            }

            return false;
        }

        public static bool HasIllegalCharacters(string aPath, bool aCheckAdditional)
        {
            FatHelpers.Debug("-- Path.HasIllegalCharacters --");
            if (aCheckAdditional)
            {
                FatHelpers.Debug("-- Path.HasIllegalCharacters : Check additional --");
                return aPath.IndexOfAny(VFSManager.GetInvalidPathCharsWithAdditionalChecks()) >= 0;
            }

            return aPath.IndexOfAny(VFSManager.GetRealInvalidPathChars()) >= 0;
        }

        public static bool IsDirectorySeparator(char aC)
        {
            FatHelpers.Debug("-- Path.IsDirectorySeparator --");
            return aC == Path.DirectorySeparatorChar || aC == Path.AltDirectorySeparatorChar;
        }

        public static bool IsPathRooted(string aPath)
        {
            FatHelpers.Debug("-- Path.IsPathRooted --");
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                int xLength = aPath.Length;
                if ((xLength >= 1
                     && (aPath[0] == Path.DirectorySeparatorChar || aPath[0] == Path.AltDirectorySeparatorChar))
                    || (xLength >= 2 && aPath[1] == Path.VolumeSeparatorChar))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsRelative(string aPath)
        {
            FatHelpers.Debug("-- Path.IsRelative --");
            return (aPath.Length < 3 || aPath[1] != Path.VolumeSeparatorChar || aPath[2] != Path.DirectorySeparatorChar);
        }

        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            FatHelpers.Debug("-- Path.NormalizePath --");
            if (IsRelative(aPath))
            {
                return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + aPath;
            }

            return aPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}

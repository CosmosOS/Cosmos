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
            [FieldAccess(Name = "System.Char System.IO.Path.AltDirectorySeparatorChar")] ref char aAltDirectorySeparatorChar,
            [FieldAccess(Name = "System.Char System.IO.Path.DirectorySeparatorChar")] ref char aDirectorySeparatorChar,
            //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidFileNameChars")] ref char[] aInvalidFileNameChars,
            //[FieldAccess(Name = "System.Char[] System.IO.Path.InvalidPathCharsWithAdditionalChecks")] ref char[] aInvalidPathCharsWithAdditionalChecks,
            //[FieldAccess(Name = "System.Char System.IO.Path.PathSeparator")] ref char aPathSeparator,
            //[FieldAccess(Name = "System.Char[] System.IO.Path.RealInvalidPathChars")] ref char[] aRealInvalidPathChars,
            [FieldAccess(Name = "System.Char System.IO.Path.VolumeSeparatorChar")] ref char aVolumeSeparatorChar
            //[FieldAccess(Name = "System.Int32 System.IO.Path.MaxPath")] ref int aMaxPath
            )
        {
            aAltDirectorySeparatorChar = VFSManager.GetAltDirectorySeparatorChar();
            aDirectorySeparatorChar = VFSManager.GetDirectorySeparatorChar();
            //aInvalidFileNameChars = VFSManager.GetInvalidFileNameChars();
            //aInvalidPathCharsWithAdditionalChecks = VFSManager.GetInvalidPathCharsWithAdditionalChecks();
            //aPathSeparator = VFSManager.GetPathSeparator();
            //aRealInvalidPathChars = VFSManager.GetRealInvalidPathChars();
            aVolumeSeparatorChar = VFSManager.GetVolumeSeparatorChar();
            //aMaxPath = VFSManager.GetMaxPath();
        }

        public static string ChangeExtension(string aPath, string aExtension)
        {
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
            if (aPath == null)
            {
                throw new Exception("Path can not be null.");
            }
            if (HasIllegalCharacters(aPath, aCheckAdditional))
            {
                throw new Exception("The path contains invalid characters.");
            }
        }

        public static void CheckSearchPattern(string aSearchPattern)
        {
            int xNum;
            while ((xNum = aSearchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
            {
                if (xNum + 2 == aSearchPattern.Length)
                {
                    throw new Exception("The search pattern is invalid.");
                }
                if (aSearchPattern[xNum + 2] == VFSManager.GetDirectorySeparatorChar() || aSearchPattern[xNum + 2] == VFSManager.GetAltDirectorySeparatorChar())
                {
                    throw new Exception("The search pattern is invalid.");
                }
                aSearchPattern = aSearchPattern.Substring(xNum + 2);
            }
        }

        public static string Combine(string aPath1, string aPath2)
        {
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
            if (aPath2.Length == 0)
            {
                return aPath1;
            }
            if (aPath1.Length == 0)
            {
                return aPath2;
            }
            if (Path.IsPathRooted(aPath2))
            {
                return aPath2;
            }
            char xC = aPath1[aPath1.Length - 1];
            if (xC != VFSManager.GetDirectorySeparatorChar() && xC != VFSManager.GetAltDirectorySeparatorChar() && xC != VFSManager.GetVolumeSeparatorChar())
            {
                return aPath1 + "\\" + aPath2;
            }
            return aPath1 + aPath2;
        }

        public static string GetDirectoryName(string aPath)
        {
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
                    while (xNum > xRootLength && xPath[--xNum] != VFSManager.GetDirectorySeparatorChar() && xPath[xNum] != VFSManager.GetAltDirectorySeparatorChar())
                    {
                    }
                    return xPath.Substring(0, xNum);
                }
            }
            return null;
        }

        public static string GetExtension(string aPath)
        {
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

                if (xC == VFSManager.GetDirectorySeparatorChar() || xC == VFSManager.GetAltDirectorySeparatorChar() || xC == VFSManager.GetVolumeSeparatorChar())
                {
                    break;
                }
            }
            return string.Empty;
        }

        public static string GetFileName(string aPath)
        {
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                int xLength = aPath.Length;
                int xNum = xLength;
                while (--xNum >= 0)
                {
                    char xC = aPath[xNum];
                    if (xC == VFSManager.GetDirectorySeparatorChar() || xC == VFSManager.GetAltDirectorySeparatorChar() || xC == VFSManager.GetVolumeSeparatorChar())
                    {
                        return aPath.Substring(xNum + 1, xLength - xNum - 1);
                    }
                }
            }
            return aPath;
        }

        public static string GetFileNameWithoutExtension(string aPath)
        {
            aPath = Path.GetFileName(aPath);
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
            string xFullPathInternal = GetFullPathInternal(aPath);
            return xFullPathInternal;
        }

        public static string GetFullPathInternal(string aPath)
        {
            if (aPath == null)
            {
                throw new Exception("Path can not be null.");
            }
            return NormalizePath(aPath, true);
        }

        public static char[] GetInvalidFileNameChars()
        {
            return VFSManager.GetInvalidFileNameChars();
        }

        public static char[] GetInvalidPathChars()
        {
            return VFSManager.GetRealInvalidPathChars();
        }

        public static string GetPathRoot(string aPath)
        {
            if (aPath == null)
            {
                return null;
            }
            FatHelpers.Debug("In PathImpl.GetPathRoot");
            aPath = NormalizePath(aPath, false);
            FatHelpers.Debug("Path normalized");
            var xRootLength = GetRootLength(aPath);
            FatHelpers.Debug("RootLength retrieved");
            FatHelpers.Debug("Value: " + xRootLength);
            var xResult = aPath.Substring(0, xRootLength);
            if (xResult[xResult.Length - 1] != Path.DirectorySeparatorChar)
            {
                FatHelpers.Debug("Adding directory separator");
                xResult = String.Concat(xResult, Path.DirectorySeparatorChar);
            }
            return xResult;
        }

        public static string GetRandomFileName()
        {
            return "random.tmp";
        }

        public static int GetRootLength(string aPath)
        {
            FatHelpers.Debug("In PathImpl.GetRootLength");
            CheckInvalidPathChars(aPath, false);
            FatHelpers.Debug("Checked for invalid path characters");
            FatHelpers.Debug("String length = " + aPath.Length);
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
                        if ((aPath[i] == VFSManager.GetDirectorySeparatorChar() || aPath[i] == VFSManager.GetAltDirectorySeparatorChar()) && --num <= 0)
                        {
                            break;
                        }
                        i++;
                    }
                }
            }
            else
            {
                if (length >= 2 && aPath[1] == VFSManager.GetVolumeSeparatorChar())
                {
                    FatHelpers.Debug("Taking the '2' path");
                    i = 2;
                    if (length >= 3 && IsDirectorySeparator(aPath[2]))
                    {
                        i++;
                    }
                }
            }
            return i;
        }

        public static string GetTempFileName()
        {
            return "tempfile.tmp";
        }

        public static string GetTempPath()
        {
            return @"\Temp";
        }

        public static bool HasExtension(string aPath)
        {
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
                    if (xC == VFSManager.GetDirectorySeparatorChar() || xC == VFSManager.GetAltDirectorySeparatorChar() || xC == VFSManager.GetVolumeSeparatorChar())
                    {
                        break;
                    }
                }
            }
            return false;
        }

        public static bool HasIllegalCharacters(string aPath, bool aCheckAdditional)
        {
            if (aCheckAdditional)
            {
                return aPath.IndexOfAny(VFSManager.GetInvalidPathCharsWithAdditionalChecks()) >= 0;
            }
            return aPath.IndexOfAny(VFSManager.GetRealInvalidPathChars()) >= 0;
        }

        public static bool IsDirectorySeparator(char aC)
        {
            return aC == VFSManager.GetDirectorySeparatorChar() || aC == VFSManager.GetAltDirectorySeparatorChar();
        }

        public static bool IsPathRooted(string aPath)
        {
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath, false);
                int xLength = aPath.Length;
                if ((xLength >= 1 && (aPath[0] == VFSManager.GetDirectorySeparatorChar() || aPath[0] == VFSManager.GetAltDirectorySeparatorChar())) || (xLength >= 2 && aPath[1] == VFSManager.GetVolumeSeparatorChar()))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsRelative(string aPath)
        {
            return (aPath.Length < 3 || aPath[1] != VFSManager.GetVolumeSeparatorChar() || aPath[2] != VFSManager.GetDirectorySeparatorChar());
        }

        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            if (IsRelative(aPath))
            {
                return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + aPath;
            }

            return aPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}

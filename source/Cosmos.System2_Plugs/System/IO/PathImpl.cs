using Cosmos.System.FileSystem.VFS;
using Cosmos.System;
using IL2CPU.API.Attribs;
using IL2CPU.API;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(Path))]
    public static class PathImpl
    {
        public static void Cctor(
            [FieldAccess(Name = "System.Char System.IO.Path.AltDirectorySeparatorChar")] ref char aAltDirectorySeparatorChar,
            [FieldAccess(Name = "System.Char System.IO.Path.DirectorySeparatorChar")] ref char aDirectorySeparatorChar,
            [FieldAccess(Name = "System.Char System.IO.Path.VolumeSeparatorChar")] ref char aVolumeSeparatorChar)
        {
            aAltDirectorySeparatorChar = VFSManager.GetAltDirectorySeparatorChar();
            aDirectorySeparatorChar = VFSManager.GetDirectorySeparatorChar();
            aVolumeSeparatorChar = VFSManager.GetVolumeSeparatorChar();
        }

        public static string ChangeExtension(string aPath, string aExtension)
        {
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath);
                string xText = aPath;
                int xNum = aPath.Length;
                while (--xNum >= 0)
                {
                    char xC = aPath[xNum];
                    if (xC == '.')
                    {
                        xText = aPath[..xNum];
                        break;
                    }
                    if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
                        || xC == Path.VolumeSeparatorChar)
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
                Global.Debugger.SendInternal($"Path.ChangeExtension : aPath = {aPath}, aExtension = {aExtension}, returning {xText}");
                return xText;
            }
            return null;
        }

        public static string Combine(string aPath1, string aPath2)
        {
            if (string.IsNullOrEmpty(aPath1) || string.IsNullOrEmpty(aPath2))
            {
                throw new ArgumentNullException(aPath1 == null ? "path1" : "path2");
            }

            CheckInvalidPathChars(aPath1);
            CheckInvalidPathChars(aPath2);
            string result = CombineNoChecks(aPath1, aPath2);
            Global.Debugger.SendInternal($"Path.Combine : aPath1 = {aPath1}, aPath2 = {aPath2}, returning {result}");
            return result;
        }

        private static string CombineNoChecks(string aPath1, string aPath2)
        {
            if (aPath2.Length == 0)
            {
                Global.Debugger.SendInternal($"Path.CombineNoChecks : aPath2 has 0 length, returning {aPath1}");
                return aPath1;
            }
            if (aPath1.Length == 0)
            {
                Global.Debugger.SendInternal($"Path.CombineNoChecks : aPath1 has 0 length, returning {aPath2}");
                return aPath2;
            }
            if (IsPathRooted(aPath2))
            {
                Global.Debugger.SendInternal($"Path.CombineNoChecks : aPath2 is root, returning {aPath2}");
                return aPath2;
            }

            char xC = aPath1[^1];
            string result;

            if (xC != Path.DirectorySeparatorChar && xC != Path.AltDirectorySeparatorChar && xC != Path.VolumeSeparatorChar)
            {
                result = string.Concat(aPath1, "\\", aPath2);
                Global.Debugger.SendInternal($"Path.CombineNoChecks : aPath1 = {aPath1}, aPath2 = {aPath2}, returning {result}");
                return result;
            }

            result = string.Concat(aPath1, aPath2);
            Global.Debugger.SendInternal($"Path.CombineNoChecks : aPath1 = {aPath1}, aPath2 = {aPath2}, returning {result}");
            return result;
        }

        public static string GetExtension(string aPath)
        {
            Global.Debugger.SendInternal("Path.GetExtension");

            if (aPath == null)
            {
                return null;
            }

            CheckInvalidPathChars(aPath);
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
            Global.Debugger.SendInternal($"Path.GetFileName : aPath = {aPath}");
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath);
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

            Global.Debugger.SendInternal($"Path.GetFileName : returning {aPath}");
            return aPath;
        }

        public static string GetFileNameWithoutExtension(string aPath)
        {
            Global.Debugger.SendInternal($"Path.GetFileNameWithoutExtension : aPath = {aPath}");

            aPath = GetFileName(aPath);
            if (aPath == null)
            {
                Global.Debugger.SendInternal($"Path.GetFileNameWithoutExtension : returning null");
                return null;
            }
            int xLength;
            if ((xLength = aPath.LastIndexOf('.')) == -1)
            {
                Global.Debugger.SendInternal($"Path.GetFileNameWithoutExtension : returning {aPath}");
                return aPath;
            }

            string xResult = aPath.Substring(0, xLength);
            Global.Debugger.SendInternal($"Path.GetFileNameWithoutExtension : returning {xResult}");

            return xResult;
        }

        public static string GetFullPath(string aPath)
        {
            if (aPath == null)
            {
                Global.Debugger.SendInternal($"Path.GetFullPath : aPath is null");
                throw new ArgumentNullException("aPath");
            }

            string result = NormalizePath(aPath, true);
            Global.Debugger.SendInternal($"Path.GetFullPath : aPath = {aPath}, returning {result}");
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

        public static string GetPathRoot(string aPath)
        {
            if (aPath == null)
            {
                Global.Debugger.SendInternal($"Path.GetPathRoot : aPath is null");
                throw new ArgumentNullException(nameof(aPath));
            }

            aPath = NormalizePath(aPath, false);
            int xRootLength = GetRootLength(aPath);
            string xResult = aPath.Substring(0, xRootLength);
            if (xResult[xResult.Length - 1] != Path.DirectorySeparatorChar)
            {
                xResult = string.Concat(xResult, Path.DirectorySeparatorChar);
            }

            Global.Debugger.SendInternal($"Path.GetPathRoot : aPath = {aPath}, xResult = {xResult}");
            return xResult;
        }

        public static string GetRandomFileName()
        {
            return "random.tmp";
        }

        public static string GetTempFileName()
        {
            return "tempfile.tmp";
        }

        public static string GetTempPath()
        {
            return @"0:\Temp";
        }

        public static bool HasExtension(string aPath)
        {
            if (aPath != null)
            {
                CheckInvalidPathChars(aPath);
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

        [PlugMethod(IsOptional = true)]
        public static bool HasIllegalCharacters(string aPath, bool aCheckAdditional)
        {
            if (aCheckAdditional)
            {
                char[] xInvalidWithAdditional = VFSManager.GetInvalidPathCharsWithAdditionalChecks();
                for (int i = 0; i < xInvalidWithAdditional.Length; i++)
                {
                    for (int j = 0; j < aPath.Length; j++)
                    {
                        if (xInvalidWithAdditional[i] == aPath[j])
                        {
                            return true;
                        }
                    }
                }
            }

            char[] xInvalid = VFSManager.GetRealInvalidPathChars();
            for (int i = 0; i < xInvalid.Length; i++)
            {
                for (int j = 0; j < aPath.Length; j++)
                {
                    if (xInvalid[i] == aPath[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool IsDirectorySeparator(char aC)
        {
            if (aC == Path.DirectorySeparatorChar)
            {
                return true;
            }

            if (aC == Path.AltDirectorySeparatorChar)
            {
                return true;
            }

            return false;
        }

        public static bool IsPathRooted(string aPath)
        {
            Global.Debugger.SendInternal("Path.IsPathRooted");

            if (aPath != null)
            {
                CheckInvalidPathChars(aPath);
                int xLength = aPath.Length;
                if ((xLength >= 1 && (aPath[0] == Path.DirectorySeparatorChar || aPath[0] == Path.AltDirectorySeparatorChar)) || (xLength >= 2 && aPath[1] == Path.VolumeSeparatorChar))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRelative(string aPath)
        {
            Global.Debugger.SendInternal("-- Path.IsRelative -- aPath = " + aPath);
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length < 2) // We have to do in two steps so we dont parse '0:' as a path incorrectly
            {
                return true;
            }

            if (aPath[1] == Path.VolumeSeparatorChar)
            {
                return false;
            }

            if (aPath.Length < 3)
            {
                return true;
            }

            if (aPath[2] != Path.DirectorySeparatorChar)
            {
                return true;
            }

            return false;
        }

        internal static void CheckInvalidPathChars(string aPath, bool aCheckAdditional = false)
        {
            Global.Debugger.SendInternal("Path.CheckInvalidPathChars");

            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            Global.Debugger.SendInternal("aPath =");
            Global.Debugger.SendInternal(aPath);

            var xChars = VFSManager.GetRealInvalidPathChars();

            for (int i = 0; i < xChars.Length; i++)
            {
                if (aPath.IndexOf(xChars[i]) >= 0)
                {
                    throw new ArgumentException("The path contains invalid characters.", aPath);
                }
            }
        }

        public static string GetDirectoryName(string aPath)
        {
            Global.Debugger.SendInternal("Path.GetDirectoryName");

            if (aPath != null)
            {
                Global.Debugger.SendInternal("aPath =");
                Global.Debugger.SendInternal(aPath);

                CheckInvalidPathChars(aPath);
                string xText = NormalizePath(aPath, false);
                if (aPath.Length > 0)
                {
                    try
                    {
                        string text2 = aPath;
                        int num = 0;
                        while (num < text2.Length && text2[num] != '?' && text2[num] != '*')
                        {
                            num++;
                        }
                        if (num > 0)
                        {
                            Path.GetFullPath(text2.Substring(0, num));
                        }
                    }
                    catch (PathTooLongException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                aPath = xText;
                int rootLength = GetRootLength(aPath);
                int num2 = aPath.Length;
                if (num2 > rootLength)
                {
                    num2 = aPath.Length;
                    if (num2 == rootLength)
                    {
                        return null;
                    }
                    while (num2 > rootLength && aPath[--num2] != Path.DirectorySeparatorChar && aPath[num2] != Path.AltDirectorySeparatorChar)
                    {
                    }
                    return aPath.Substring(0, num2);
                }
            }
            return null;
        }

        internal static int GetRootLength(string aPath)
        {
            Global.Debugger.SendInternal("Path.GetRootLength");
            Global.Debugger.SendInternal("aPath =" + aPath);

            int i = 0;
            int xLength = aPath.Length;
            if (xLength >= 1 && IsDirectorySeparator(aPath[0]))
            {
                i = 1;
                if (xLength >= 2 && IsDirectorySeparator(aPath[1]))
                {
                    i = 2;
                    int xNum = 2;
                    while (i < xLength)
                    {
                        if ((aPath[i] == Path.DirectorySeparatorChar || aPath[i] == Path.AltDirectorySeparatorChar) && --xNum <= 0)
                        {
                            break;
                        }
                        i++;
                    }
                }
            }
            else if (xLength >= 2 && aPath[1] == VFSManager.GetVolumeSeparatorChar())
            {
                i = 2;
                if (xLength >= 3 && IsDirectorySeparator(aPath[2]))
                {
                    i++;
                }
            }
            return i;
        }

        static string NormalizePath(string aPath, bool aFullCheck)
        {
            Global.Debugger.SendInternal("-- Path.NormalizePath -- aPath = " + aPath);

            if (aPath == null)
            {
                Global.Debugger.SendInternal("aPath is null");
                throw new ArgumentNullException("aPath");
            }

            string result = aPath;
            if (IsRelative(result))
            {
                result = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + result;
                Global.Debugger.SendInternal("aPath is relative");
                Global.Debugger.SendInternal("aPath =" + aPath);
                Global.Debugger.SendInternal("result = " + result);
            }

            if (IsDirectorySeparator(result[result.Length - 1]))
            {
                Global.Debugger.SendInternal("Found directory seprator");
                if (result.Length > 3)
                {
                    result = result.Remove(result.Length - 1);
                }
            }

            Global.Debugger.SendInternal("aPath = " + aPath);
            Global.Debugger.SendInternal("result = " + result);
            return result;
        }
    }
}
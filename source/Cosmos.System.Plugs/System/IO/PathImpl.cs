using System;
using System.IO;

using Cosmos.Common;
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
                FatHelpers.Debug("-- Path.ChangeExtension : aPath = " + aPath + ", aExtension = " + aExtension + ", result = " + xText + " --");
                return xText;
            }
            return null;
        }

        public static void CheckInvalidPathChars(string aPath, bool aCheckAdditional)
        {
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
            int xNum;
            while ((xNum = aSearchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
            {
                if (xNum + 2 == aSearchPattern.Length)
                {
                    throw new ArgumentException("The search pattern is invalid.", aSearchPattern);
                }

                if (aSearchPattern[xNum + 2] == Path.DirectorySeparatorChar
                    || aSearchPattern[xNum + 2] == Path.AltDirectorySeparatorChar)
                {
                    throw new ArgumentException("The search pattern is invalid.", aSearchPattern);
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
            string result = CombineNoChecks(aPath1, aPath2);
            FatHelpers.Debug("-- Path.Combine : aPath1 = " + aPath1 + ", aPath2 = " + aPath2 + ", result = " + result + " --");
            return result;
        }

        public static string CombineNoChecks(string aPath1, string aPath2)
        {
            if (aPath2.Length == 0)
            {
                FatHelpers.Debug("-- Path.CombineNoChecks : aPath2 has 0 length, result = " + aPath1 + " --");
                return aPath1;
            }

            if (aPath1.Length == 0)
            {
                FatHelpers.Debug("-- Path.CombineNoChecks : aPath1 has 0 length, result = " + aPath2 + " --");
                return aPath2;
            }

            if (IsPathRooted(aPath2))
            {
                FatHelpers.Debug("-- Path.CombineNoChecks : aPath2 is root, result = " + aPath2 + " --");
                return aPath2;
            }

            string xResult = string.Empty; 
            char xC = aPath1[aPath1.Length - 1];
            if (xC != Path.DirectorySeparatorChar && xC != Path.AltDirectorySeparatorChar
                && xC != Path.VolumeSeparatorChar)
            {
                xResult = string.Concat(aPath1, "\\", aPath2);
                FatHelpers.Debug("-- Path.CombineNoChecks : aPath1 = " + aPath1 + ", aPath2 = " + aPath2 + ", result = " + xResult + " --");
                return xResult;
            }

            xResult = string.Concat(aPath1, aPath2);
            FatHelpers.Debug("-- Path.CombineNoChecks : aPath1 = " + aPath1 + ", aPath2 = " + aPath2 + ", result = " + xResult + " --");
            return xResult;
        }

        public static string GetDirectoryName(string aPath)
        {
            FatHelpers.Debug("PathImpl -- GetDirectoryName");
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

                    string result = xPath.Substring(0, xNum);
                    FatHelpers.Debug("-- Path.GetDirectoryName : aPath = " + aPath + ", result = " + result + " --");
                    return result;
                }
            }

            FatHelpers.Debug("-- Path.GetDirectoryName : aPath is null --");
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

                if (xC == Path.DirectorySeparatorChar || xC == Path.AltDirectorySeparatorChar
                    || xC == Path.VolumeSeparatorChar)
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
            if (aPath == null)
            {
                FatHelpers.Debug("-- Path.GetFullPath : aPath is null --");
                throw new ArgumentNullException("aPath");
            }

            string result = NormalizePath(aPath, true);
            FatHelpers.Debug("-- Path.GetFullPath : aPath = " + aPath + ", result = " + result + " --");
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
                FatHelpers.Debug("-- Path.GetPathRoot : aPath is null --");
                throw new ArgumentNullException("aPath");
            }

            aPath = NormalizePath(aPath, false);
            int xRootLength = GetRootLength(aPath);
            string xResult = aPath.Substring(0, xRootLength);
            if (xResult[xResult.Length - 1] != Path.DirectorySeparatorChar)
            {
                xResult = string.Concat(xResult, Path.DirectorySeparatorChar.ToString());
            }

            FatHelpers.Debug("-- Path.GetPathRoot : aPath = " + aPath + ", xResult = " + xResult + " --");
            return xResult;
        }

        public static string GetRandomFileName()
        {
            return "random.tmp";
        }

        public static int GetRootLength(string aPath)
        {
            if (aPath == null)
            {
                FatHelpers.Debug("-- Path.GetRootLength : aPath is null --");
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
                        if ((aPath[i] == Path.DirectorySeparatorChar || aPath[i] == Path.AltDirectorySeparatorChar)
                            && --num <= 0)
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

            FatHelpers.Debug("-- Path.GetRootLength : result = " + i.ToString() + " --");
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

        public static bool IsPathRooted(string aPath)
        {
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
            if (aPath.Length < 3)
            {
                return true;
            }

            string xC = aPath[1].ToString();
            if (xC != Path.VolumeSeparatorChar.ToString())
            {
                return true;
            }

            xC = aPath[2].ToString();
            if (xC != Path.DirectorySeparatorChar.ToString())
            {
                return true;
            }

            return false;
        }

        //public static string NormalizePath(string aPath, bool aFullCheck, int aMaxPathLength, bool aExpandShortPaths)
        //{
        //    FatHelpers.Debug("-- Path.NormalizePath : aPath = " + aPath + " --");
        //    if (aPath == null)
        //    {
        //        throw new ArgumentNullException("aPath");
        //    }

        //    return aPath;

        //    // TODO: Fix this.
        //    // If we're doing a full path check, trim whitespace and look for illegal path characters.
        //    if (aFullCheck)
        //    {
        //        aPath = aPath.TrimEnd(VFSManager.GetTrimEndChars());
        //        CheckInvalidPathChars(aPath, false);
        //    }

        //    int index = 0;
        //    string newBuffer = string.Empty;

        //    uint numSpaces = 0;
        //    uint numDots = 0;
        //    bool fixupDirectorySeparator = false;
        //    uint numSigChars = 0;
        //    int lastSigChar = -1; // Index of last significant character.
        //    bool startedWithVolumeSeparator = false;
        //    bool firstSegment = true;
        //    int lastDirectorySeparatorPos = 0;
        //    char currentChar = aPath[0];
        //    if (aPath.Length > 0 && (currentChar == Path.DirectorySeparatorChar || aPath[0] == Path.AltDirectorySeparatorChar))
        //    {
        //        newBuffer = string.Concat(newBuffer, "\\");
        //        index++;
        //        lastSigChar = 0;
        //        FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //    }

        //    // Normalize the string, stripping out redundant dots, spaces, and slashes.
        //    while (index < aPath.Length)
        //    {
        //        currentChar = aPath[index];
        //        FatHelpers.Debug("-- Path.NormalizePath : index = " + index.ToString() + ", currentChar = " + currentChar.ToString() + " --");

        //        // We handle both directory separators and dots specially. For directory separators, we consume consecutive appearances.  
        //        // For dots, we consume all dots beyond the second in succession. All other characters are added as is.
        //        // In addition we consume all spaces after the last other char in a directory name up until the directory separator.
        //        if (currentChar == Path.DirectorySeparatorChar || currentChar == Path.AltDirectorySeparatorChar)
        //        {
        //            FatHelpers.Debug("-- Path.NormalizePath : currentChar is a directory separator or alt directory separator --");
        //            if (numSigChars == 0)
        //            {
        //                FatHelpers.Debug("-- Path.NormalizePath : numSigChars = 0 --");
        //                // Dot and space handling
        //                if (numDots > 0)
        //                {
        //                    FatHelpers.Debug("-- Path.NormalizePath : numDots > 0 --");
        //                    // Look for ".[space]*" or "..[space]*"
        //                    int start = lastSigChar + 1;
        //                    if (aPath[start] != '.')
        //                    {
        //                        throw new ArgumentException("Illegal path.", "aPath");
        //                    }

        //                    // Only allow "[dot]+[space]*", and normalize the legal ones to "." or ".."
        //                    if (numDots >= 2)
        //                    {
        //                        FatHelpers.Debug("-- Path.NormalizePath : numDots >= 2 --");
        //                        // Reject "C:..."
        //                        if (startedWithVolumeSeparator && numDots > 2)
        //                        {
        //                            throw new ArgumentException("Illegal path.", "aPath");
        //                        }

        //                        if (aPath[start + 1] == '.')
        //                        {
        //                            // Search for a space in the middle of the dots and throw
        //                            for (int i = start + 2; i < start + numDots; i++)
        //                            {
        //                                if (aPath[i] != '.')
        //                                {
        //                                    throw new ArgumentException("Illegal path.", "aPath");
        //                                }
        //                            }

        //                            numDots = 2;
        //                        }
        //                        else
        //                        {
        //                            if (numDots > 1)
        //                            {
        //                                throw new ArgumentException("Illegal path.", "aPath");
        //                            }
        //                            numDots = 1;
        //                        }
        //                    }

        //                    if (numDots == 2)
        //                    {
        //                        FatHelpers.Debug("-- Path.NormalizePath : numDots == 2 --");
        //                        newBuffer = string.Concat(newBuffer, ".");
        //                        index++;
        //                        FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //                    }

        //                    newBuffer = string.Concat(newBuffer, ".");
        //                    index++;
        //                    FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //                    fixupDirectorySeparator = false;

        //                    // Continue in this case, potentially writing out '\'.
        //                }

        //                if (numSpaces > 0 && firstSegment)
        //                {
        //                    FatHelpers.Debug("-- Path.NormalizePath : numSpaces > 0 && firstSegment --");
        //                    // Handle strings like " \\server\share".
        //                    if (index + 1 < aPath.Length &&(aPath[index + 1] == Path.DirectorySeparatorChar || aPath[index + 1] == Path.AltDirectorySeparatorChar))
        //                    {
        //                        newBuffer = string.Concat(newBuffer, Path.DirectorySeparatorChar.ToString());
        //                        index++;
        //                        FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //                    }
        //                }
        //            }
        //            numDots = 0;
        //            numSpaces = 0;  // Suppress trailing spaces

        //            if (!fixupDirectorySeparator)
        //            {
        //                FatHelpers.Debug("-- Path.NormalizePath : fixupDirectorySeparator is false --");
        //                fixupDirectorySeparator = true;
        //                newBuffer = string.Concat(newBuffer, Path.DirectorySeparatorChar.ToString());
        //                index++;
        //                FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //            }
        //            numSigChars = 0;
        //            lastSigChar = index;
        //            startedWithVolumeSeparator = false;
        //            firstSegment = false;

        //            // For short file names, we must try to expand each of them as
        //            // soon as possible.  We need to allow people to specify a file
        //            // name that doesn't exist using a path with short file names
        //            // in it, such as this for a temp file we're trying to create:
        //            // C:\DOCUME~1\USERNA~1.RED\LOCALS~1\Temp\bg3ylpzp
        //            // We could try doing this afterwards piece by piece, but it's
        //            // probably a lot simpler to do it here.
        //            //if (mightBeShortFileName)
        //            //{
        //            //    newBuffer.TryExpandShortFileName();
        //            //    mightBeShortFileName = false;
        //            //}

        //            int thisPos = newBuffer.Length - 1;
        //            if (thisPos - lastDirectorySeparatorPos > VFSManager.GetMaxPath())
        //            {
        //                throw new PathTooLongException("Path is too long.");
        //            }
        //            lastDirectorySeparatorPos = thisPos;
        //        } // if (Found directory separator)
        //        else if (currentChar == '.')
        //        {
        //            FatHelpers.Debug("-- Path.NormalizePath : currentChar == '.' --");
        //            // Reduce only multiple .'s only after slash to 2 dots. For
        //            // instance a...b is a valid file name.
        //            numDots++;
        //            // Don't flush out non-terminal spaces here, because they may in
        //            // the end not be significant.  Turn "c:\ . .\foo" -> "c:\foo"
        //            // which is the conclusion of removing trailing dots & spaces,
        //            // as well as folding multiple '\' characters.
        //        }
        //        else if (currentChar == ' ')
        //        {
        //            FatHelpers.Debug("-- Path.NormalizePath : currentChar == ' ' --");
        //            numSpaces++;
        //        }
        //        else
        //        {  // Normal character logic
        //            //if (currentChar == '~' && expandShortPaths)
        //            //    mightBeShortFileName = true;

        //            FatHelpers.Debug("-- Path.NormalizePath : currentChar == normal character --");
        //            fixupDirectorySeparator = false;

        //            // To reject strings like "C:...\foo" and "C  :\foo"
        //            if (firstSegment && currentChar == Path.VolumeSeparatorChar)
        //            {
        //                // Only accept "C:", not "c :" or ":"
        //                // Get a drive letter or ' ' if index is 0.
        //                char driveLetter = (index > 0) ? aPath[index - 1] : ' ';
        //                bool validPath = ((numDots == 0) && (numSigChars >= 1) && (driveLetter != ' '));
        //                if (!validPath)
        //                {
        //                    throw new ArgumentException("Illegal path.");
        //                }

        //                startedWithVolumeSeparator = true;
        //                // We need special logic to make " c:" work, we should not fix paths like "  foo::$DATA"
        //                if (numSigChars > 1)
        //                { // Common case, simply do nothing
        //                    int spaceCount = 0; // How many spaces did we write out, numSpaces has already been reset.
        //                    while ((spaceCount < newBuffer.Length) && newBuffer[spaceCount] == ' ')
        //                    {
        //                        spaceCount++;
        //                    }
        //                    if (numSigChars - spaceCount == 1)
        //                    {
        //                        index = 0;
        //                        newBuffer = string.Empty;
        //                        newBuffer = string.Concat(newBuffer, driveLetter.ToString()); // Overwrite spaces, we need a special case to not break "  foo" as a relative path.
        //                        index++;
        //                        FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //                    }
        //                }
        //                numSigChars = 0;
        //            }
        //            else
        //            {
        //                numSigChars += 1 + numDots + numSpaces;
        //                FatHelpers.Debug("-- Path.NormalizePath : numSigChars = " + numSigChars.ToString() + " --");
        //            }

        //            // Copy any spaces & dots since the last significant character
        //            // to here.  Note we only counted the number of dots & spaces,
        //            // and don't know what order they're in.  Hence the copy.
        //            if (numDots > 0 || numSpaces > 0)
        //            {
        //                int numCharsToCopy = (lastSigChar >= 0) ? index - lastSigChar - 1 : index;
        //                if (numCharsToCopy > 0)
        //                {
        //                    for (int i = 0; i < numCharsToCopy; i++)
        //                    {
        //                        newBuffer = string.Concat(newBuffer, aPath[lastSigChar + 1 + i].ToString());
        //                        index++;
        //                        FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //                    }
        //                }
        //                numDots = 0;
        //                numSpaces = 0;
        //            }

        //            newBuffer = string.Concat(newBuffer, currentChar.ToString());
        //            index++;
        //            FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //            lastSigChar = index;
        //        }

        //        index++;
        //    } // end while

        //    // Drop any trailing dots and spaces from file & directory names, EXCEPT
        //    // we MUST make sure that "C:\foo\.." is correctly handled.
        //    // Also handle "C:\foo\." -> "C:\foo", while "C:\." -> "C:\"
        //    if (numSigChars == 0)
        //    {
        //        if (numDots > 0)
        //        {
        //            // Look for ".[space]*" or "..[space]*"
        //            int start = lastSigChar + 1;
        //            if (aPath[start] != '.')
        //            {
        //                throw new ArgumentException("Illegal path.");
        //            }

        //            // Only allow "[dot]+[space]*", and normalize the legal ones to "." or ".."
        //            if (numDots >= 2)
        //            {
        //                // Reject "C:..."
        //                if (startedWithVolumeSeparator && numDots > 2)
        //                {
        //                    throw new ArgumentException("Illegal path.");
        //                }

        //                if (aPath[start + 1] == '.')
        //                {
        //                    // Search for a space in the middle of the dots and throw
        //                    for (int i = start + 2; i < start + numDots; i++)
        //                    {
        //                        if (aPath[i] != '.')
        //                        {
        //                            throw new ArgumentException("Illegal path.");
        //                        }
        //                    }

        //                    numDots = 2;
        //                }
        //                else
        //                {
        //                    if (numDots > 1)
        //                    {
        //                        throw new ArgumentException("Illegal path.");
        //                    }
        //                    numDots = 1;
        //                }
        //            }

        //            if (numDots == 2)
        //            {
        //                newBuffer = string.Concat(newBuffer, ".");
        //                index++;
        //                FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //            }

        //            newBuffer = string.Concat(newBuffer, ".");
        //            index++;
        //            FatHelpers.Debug("-- Path.NormalizePath : newBuffer = " + newBuffer + " --");
        //        }
        //    } // if (numSigChars == 0)

        //    // If we ended up eating all the characters, bail out.
        //    if (newBuffer.Length == 0)
        //    {
        //        throw new ArgumentException("Illegal path.");
        //    }

        //    string returnVal = newBuffer.ToString();
        //    FatHelpers.Debug("-- Path.NormalizePath : returnVal = " + returnVal + " --");
        //    if (string.Equals(returnVal, aPath, StringComparison.Ordinal))
        //    {
        //        returnVal = aPath;
        //    }
        //    return returnVal;

        //}

        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            if (aPath == null)
            {
                FatHelpers.Debug("-- Path.NormalizePath : aPath is null --");
                throw new ArgumentNullException("aPath");
            }

            string result = aPath;
            if (IsRelative(result))
            {
                result = string.Concat(Directory.GetCurrentDirectory(), Path.DirectorySeparatorChar.ToString(), result);
                FatHelpers.Debug("-- Path.NormalizePath : aPath is relative, aPath = " + aPath + ", result = " + result + " --");
            }

            if (IsDirectorySeparator(result[result.Length - 1]))
            {
                FatHelpers.Debug("Found directory seprator");
                result = result.Remove(result.Length - 1);
            }

            FatHelpers.Debug("-- Path.NormalizePath : aPath = " + aPath + ", result = " + result + " --");
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug("System.SR, System.Private.CoreLib")]
    public class SRImpl
    {
        public static string InternalGetResourceString(string aKey)
        {
            if (aKey == "ArgumentNull_Generic")
            {
                return "Value cannot be null.";
            }
            if (aKey == "ArgumentNull_WithParamName")
            {
                return "Parameter '{0}' cannot be null.";
            }
            if (aKey == "ArgumentNull_FileName")
            {
                return "File name cannot be null.";
            }
            if (aKey == "ArgumentNull_Path")
            {
                return "Path cannot be null.";
            }
            if (aKey == "ArgumentNull_Stream")
            {
                return "Stream cannot be null.";
            }
            if (aKey == "ArgumentNull_String")
            {
                return "String reference not set to an instance of a String.";
            }
            if (aKey == "Arg_ParamName_Name")
            {
                return "Parameter name: {0}";
            }
            if (aKey == "ArgumentOutOfRange_Index")
            {
                return "Argument {0} out of range.";
            }
            if (aKey == "ArgumentOutOfRange_NegativeCount")
            {
                return "Argument {0} out of range. It must not be negative.";
            }
            if (aKey == "Arg_EnumIllegalVal")
            {
                return "Argument {0} contains an illegal enum value.";
            }
            if (aKey == "Argument_EmptyFileName")
            {
                return "Empty file name is not legal.";
            }
            if (aKey == "Argument_EmptyPath")
            {
                return "Empty path name is not legal.";
            }
            if (aKey == "Argument_EmptyName")
            {
                return "Empty name is not legal.";
            }
            if (aKey == "Arg_NullReferenceException")
            {
                return "Object reference not set to an instance of an object.";
            }
            if (aKey == "Arg_OverflowException")
            {
                return "Arithmetic operation resulted in an overflow.";
            }
            if (aKey == "Arg_PathIllegal")
            {
                return "The path is not of a legal form.";
            }
            if (aKey == "Arg_ArgumentException")
            {
                return "Value does not fall within the expected range.";
            }
            if (aKey == "Arg_ArgumentOutOfRangeException")
            {
                return "Specified argument was out of the range of valid values.";
            }
            if (aKey == "Arg_DirectoryNotFoundException")
            {
                return "Attempted to access a path that is not on the disk.";
            }
            if (aKey == "Arg_DriveNotFoundException")
            {
                return "Attempted to access a drive that is not available.";
            }
            if (aKey == "Arg_InvalidFileName")
            {
                return "Specified file name was invalid.";
            }
            if (aKey == "Arg_InvalidFileExtension")
            {
                return "Specified file extension was not a valid extension.";
            }
            if (aKey == "Argument_PathEmpty")
            {
                return "Path cannot be the empty string or all whitespace.";
            }
            if (aKey == "Argument_PathFormatNotSupported")
            {
                return "The given path's format is not supported.";
            }
            if (aKey == "Argument_InvalidSubPath")
                return "The given path {0} is {1}";

            //Console.Write("Getting resource: '");
            //Console.Write(aResource);
            //Console.WriteLine("'");
            //Console.ReadLine();
            return aKey;
        }
    }
}

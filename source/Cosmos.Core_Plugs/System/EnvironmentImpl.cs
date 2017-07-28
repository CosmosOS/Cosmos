using System;
using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Plugs.System {
    [Plug(Target = typeof(Environment))]
    public static class EnvironmentImpl {
        //        [PlugMethod(Signature = "System_Environment_OSName__System_Environment_get_OSInfo__")]
        //        public static int get_OSName() { return 0x82; }

        public static string GetResourceFromDefault(string aResource) {
            if (aResource == "ArgumentNull_Generic") {
                return "Value cannot be null.";
            }
            if (aResource == "ArgumentNull_WithParamName") {
                return "Parameter '{0}' cannot be null.";
            }
            if (aResource == "ArgumentNull_FileName") {
                return "File name cannot be null.";
            }
            if (aResource == "ArgumentNull_Path") {
                return "Path cannot be null.";
            }
            if (aResource == "ArgumentNull_Stream") {
                return "Stream cannot be null.";
            }
            if (aResource == "ArgumentNull_String") {
                return "String reference not set to an instance of a String.";
            }
            if (aResource == "Arg_ParamName_Name") {
                return "Parameter name: {0}";
            }
            if (aResource == "ArgumentOutOfRange_Index") {
                return "Argument {0} out of range.";
            }
            if (aResource == "ArgumentOutOfRange_NegativeCount") {
                return "Argument {0} out of range. It must not be negative.";
            }
            if (aResource == "Arg_EnumIllegalVal") {
                return "Argument {0} contains an illegal enum value.";
            }
            if (aResource == "Argument_EmptyFileName") {
                return "Empty file name is not legal.";
            }
            if (aResource == "Argument_EmptyPath") {
                return "Empty path name is not legal.";
            }
            if (aResource == "Argument_EmptyName") {
                return "Empty name is not legal.";
            }
            if (aResource == "Arg_NullReferenceException") {
                return "Object reference not set to an instance of an object.";
            }
            if (aResource == "Arg_OverflowException") {
                return "Arithmetic operation resulted in an overflow.";
            }
            if (aResource == "Arg_PathIllegal") {
                return "The path is not of a legal form.";
            }
            if (aResource == "Arg_ArgumentException") {
                return "Value does not fall within the expected range.";
            }
            if (aResource == "Arg_ArgumentOutOfRangeException") {
                return "Specified argument was out of the range of valid values.";
            }
            if (aResource == "Arg_DirectoryNotFoundException") {
                return "Attempted to access a path that is not on the disk.";
            }
            if (aResource == "Arg_DriveNotFoundException") {
                return "Attempted to access a drive that is not available.";
            }
            if (aResource == "Arg_InvalidFileName") {
                return "Specified file name was invalid.";
            }
            if (aResource == "Arg_InvalidFileExtension") {
                return "Specified file extension was not a valid extension.";
            }
            if (aResource == "Argument_PathEmpty") {
                return "Path cannot be the empty string or all whitespace.";
            }
            if (aResource == "Argument_PathFormatNotSupported") {
                return "The given path's format is not supported.";
            }

            //Console.Write("Getting resource: '");
            //Console.Write(aResource);
            //Console.WriteLine("'");
            //Console.ReadLine();
            return aResource;
        }

        //        public static string GetResourceString(string key,
        //                                               params object[] values)
        //        {
        //            return string.Format(GetResourceString(key), values);
        //        }

        public static string GetResourceString(string aResource) {
            return GetResourceFromDefault(aResource);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Tasks
{
    public static class VSCompliantLogger
    {
        public enum MessageType
        {
            Warning,
            Error
        }

        public static string Log(string source, MessageType type, string code, string message)
        {
            return string.Format("{0} : {1} {2} : {3}", source, type.ToString().ToLower(), code, message);
        }

        public static string Log(string source, string subcategory, MessageType type, string code, string message)
        {
            return string.Format("{0} : {1} {2} {3} : {4}", source, subcategory, type.ToString().ToLower(), code, message);
        }

        public static string Log(string source, string subcategory, MessageType type, string code)
        {
            return string.Format("{0} : {1} {2} {3}", source, subcategory, type.ToString().ToLower(), code);
        }

        public static string Log(string source, MessageType type, string code)
        {
            return string.Format("{0} : {1} {2}", source, type.ToString().ToLower(), code);
        }
    }
}

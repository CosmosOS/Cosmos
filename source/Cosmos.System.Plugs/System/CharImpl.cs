using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(char))]
    public static class CharImpl
    {
        public static void Cctor()
        {
            //
        }

        public static string ToString(ref char aThis)
        {
            char[] xResult = new char[1];
            xResult[0] = aThis;
            return new string(xResult);
        }

        public static char ToUpper(char aThis)
        {
            // todo: properly implement Char.ToUpper()
            return aThis;
        }

        public static bool IsWhiteSpace(char aChar)
        {
            return aChar == ' ' || aChar == '\t';
        }
    }
}
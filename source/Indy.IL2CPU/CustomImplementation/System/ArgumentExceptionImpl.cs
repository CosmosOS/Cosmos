using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
namespace Indy.IL2CPU.CustomImplementation
{

    [Plug(Target = typeof(ArgumentException))]
    public static class ArgumentExceptionImpl
    {
        public static String get_Message()
        {
            return "";
        }


    }
}

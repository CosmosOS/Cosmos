using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target=typeof(Environment))]
    public static class EnvironmentImpl
    {
        [PlugMethod(Signature = "System_Environment_OSName__System_Environment_get_OSInfo__")]
        public static int get_OSName() { return 0x82; }

        public static string GetResourceFromDefault(string aResource)
        {
            if (aResource == "ArgumentNull_Generic")
            {
                return "Parameter was null!";
            }
            if (aResource == "Arg_ParamName_Name")
            {
                return "Parameter name: {0}";
            }
            if (aResource == "ArgumentOutOfRange_Index")
            {
                return "Argument {0} out of range!";
            }
            //Console.Write("Getting resource: '");
            //Console.Write(aResource);
            //Console.WriteLine("'");
            //Console.ReadLine();
            return aResource;
        }

        public static string GetResourceString(string key,
                                               params object[] values)
        {
            return key;
        }

        public static string GetResourceString(string aResource)
        {
            return GetResourceFromDefault(aResource);
        }

    }

}

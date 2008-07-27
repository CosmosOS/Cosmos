using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
    [Plug(Target = typeof(Environment))]
    public static class EnvironmentImpl {
        public static string GetResourceFromDefault(string aResource) {
            if (aResource == "ArgumentNull_Generic") {
                return "Parameter was null!";
            }
            if (aResource == "Arg_ParamName_Name") {
                return "Parameter name: {0}";
            }
            Console.Write("Getting resource: '");
            Console.Write(aResource);
            Console.WriteLine("'");
            return aResource;
        }

        public static string GetResourceString(string key,
                                               params object[] values) {
            return key;
        }

        public static string GetResourceString(string aResource) {
            return GetResourceFromDefault(aResource);
        }
    }
}
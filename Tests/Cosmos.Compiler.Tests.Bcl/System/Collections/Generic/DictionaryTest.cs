using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;


namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Generic
{
    public static class DictionaryTest
    {
        public static void Execute()
        {
            Dictionary<String, Object> commands = new Dictionary<String, Object>()
            {
                {"echo", "ECHO"},
                {"reboot", "REBOOT" },
                {"shutdown", "SHUTDOWN"}
            };

            Assert.IsTrue(commands.ContainsKey("echo"), "Dictionary ContainsKey does not work");
        }
    }
}

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
                {"shutdown", "SHUTDOWN"},
                {"integer", 500}
            };

            Assert.IsTrue(commands.ContainsKey("echo"), "Dictionary ContainsKey does not work1");
            Assert.IsFalse(commands.ContainsKey("musterror"), "Dictionary ContainsKey does not work 2");
            


            //String test
            Assert.IsTrue((string)commands["echo"] == "ECHO", "Dictionary string not work");
            commands["echo"] = "notEcho";
            Assert.IsTrue((string)commands["echo"] == "notEcho", "Dictionary string been reset not working");
            
            
            //Integer test
            Assert.IsTrue((int)commands["integer"] == 500, "Dictionary integer not working");
            commands["integer"] = 321;
            Assert.IsTrue((int)commands["integer"] == 321, "Dictionary integer been reset not working");


        }
    }
}

using System;

namespace Cosmos.TestRunner.Core
{
    public class OutputHandlerFullConsole: OutputHandlerFullTextBase
    {
        protected override void Log(string message)
        {
            Console.Write(DateTime.Now.ToString("hh:mm:ss.ffffff "));
            Console.Write(new string(' ', mLogLevel * 2));
            Console.WriteLine(message);
        }
    }
}

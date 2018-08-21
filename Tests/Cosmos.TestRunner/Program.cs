using System;
using System.IO;

using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var xLogPath = Path.Combine(
                Path.GetDirectoryName(typeof(Program).Assembly.Location), "WorkingDirectory", "TestRunnerLog.xml");

            if (args.Length == 1)
            {
                xLogPath = args[0];
            }

            var xEngineConfiguration = new DefaultEngineConfiguration();
            var xOutputHandler = new OutputHandlerFullConsole();

            var xEngine = new FullEngine(xEngineConfiguration);
            xEngine.SetOutputHandler(xOutputHandler);

            var xResult = xEngine.Execute();

            try
            {
                xResult.SaveXmlToFile(xLogPath);
                Console.WriteLine("Log written to '{0}'.", xLogPath);
            }
            catch (Exception e)
            {
                xOutputHandler.UnhandledException(e);
                Console.ReadKey(true);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }
    }
}

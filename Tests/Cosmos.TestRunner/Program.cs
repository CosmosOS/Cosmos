using System;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.Console
{
    using Console = global::System.Console;

    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var xEngine = new Engine(new DefaultEngineConfiguration());

            var xOutputXml = new OutputHandlerXml();
            xEngine.OutputHandler = new MultiplexingOutputHandler(
                xOutputXml,
                new OutputHandlerFullConsole());

            xEngine.Execute();

            Console.WriteLine("Do you want to save test run details?");
            Console.Write("Type yes, or nothing to just exit: ");
            var xResult = Console.ReadLine();
            if (xResult != null && xResult.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Path to file: ");
                xResult = Console.ReadLine();

                try
                {
                    xOutputXml.SaveToFile(xResult);
                    Console.WriteLine("Succesfully saved output to file " + xResult);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }
                Console.ReadLine();
                //var xSaveDialog = new SaveFileDialog();
                //xSaveDialog.Filter = "XML documents|*.xml";
                //if (xSaveDialog.ShowDialog() != DialogResult.OK)
                //{
                //    return;
                //}

                //xOutputXml.SaveToFile(xSaveDialog.FileName);
            }
        }
    }
}

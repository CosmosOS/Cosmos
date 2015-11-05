using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cosmos.TestRunner.Core;
using Microsoft.Win32;

namespace Cosmos.TestRunner.Console
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var xEngine = new Engine();

            DefaultEngineConfiguration.Apply(xEngine);

            var xOutputXml = new OutputHandlerXml();
            xEngine.OutputHandler = new MultiplexingOutputHandler(
                xOutputXml,
                new OutputHandlerFullConsole());

            xEngine.Execute();

            global::System.Console.WriteLine("Do you want to save test run details?");
            global::System.Console.Write("Type yes, or nothing to just exit: ");
            var xResult = global::System.Console.ReadLine();
            if (xResult != null && xResult.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                var xSaveDialog = new SaveFileDialog();
                xSaveDialog.Filter = "XML documents|*.xml";
                if (xSaveDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                xOutputXml.SaveToFile(xSaveDialog.FileName);
            }
        }
    }
}

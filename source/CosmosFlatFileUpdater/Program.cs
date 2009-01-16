using System;
using System.IO;
using System.Text;

namespace CosmosFlatFileUpdater {
    internal class Program {
        private static void Main() {
            var xBaseCosmosSourceDir = new DirectoryInfo(Path.GetDirectoryName(typeof(Program).Assembly.Location)).Parent.Parent.Parent.FullName;
            var xLines = File.ReadAllLines(Path.Combine(xBaseCosmosSourceDir,@"Cosmos.sln"));
            if((File.GetAttributes(Path.Combine(xBaseCosmosSourceDir,"Cosmos.Flat.sln")) & FileAttributes.ReadOnly) != 0) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please make Cosmos.Flat.sln read-write! Press a key to close");
                Console.ReadKey();
                return;
            }
            using (var xOutput = new StreamWriter(Path.Combine(xBaseCosmosSourceDir,"Cosmos.Flat.New.sln"), false, Encoding.UTF8)) {
                var xLinesToSkip = 0;
                var xSkipTo = "";
                foreach (var xLine in xLines) {
                    if (xLinesToSkip > 0) {
                        xLinesToSkip--;
                        continue;
                    }
                    if (!String.IsNullOrEmpty(xSkipTo)) {
                        if (xLine.Trim() == xSkipTo) {
                            xSkipTo = null;
                        }
                        continue;
                    }
                    if (xLine.StartsWith("Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\")")) {
                        xLinesToSkip = 1;
                        continue;
                    }
                    if (xLine.Trim() == "GlobalSection(TeamFoundationVersionControl) = preSolution") {
                        xSkipTo = "EndGlobalSection";
                        continue;
                    }
                    if (xLine.Trim() == "GlobalSection(NestedProjects) = preSolution") {
                        xSkipTo = "EndGlobalSection";
                        continue;
                    }
                    if (xLine.Trim() == "# Visual Studio 2008") {
                        xOutput.WriteLine("# Visual C# Express 2008");
                        continue;
                    }
                    xOutput.WriteLine(xLine);
                }
                Console.WriteLine("Done updating Cosmos.Flat.sln! Press a key to close");
                Console.ReadKey();
            }
        }
    }
}
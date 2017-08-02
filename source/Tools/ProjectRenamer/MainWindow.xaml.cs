using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IO = System.IO;

namespace ProjectRenamer {
    public partial class MainWindow : Window {
        string mCosmosDir;
        string mSourceDir;

        string mOld;
        string mNew;

        List<string> mSlnList = new List<string>();

        public MainWindow() {
            InitializeComponent();

            //tboxRenameOldName.Text = "Cosmos.Core.Plugs";
            //tboxRenameNewName.Text = "Cosmos.Core_Plugs";

            mCosmosDir = IO.Path.GetFullPath(IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\.."));
            tblkCosmosDir.Text = tblkCosmosDir.Text + mCosmosDir;
            //
            mSourceDir = IO.Path.Combine(mCosmosDir, "source");
            tblkSourceDir.Text = tblkSourceDir.Text + mSourceDir;

            // TODO - Find all SLNs instead?
            // If so, have to exlude possibly Tools at least.
            AddSLN(@"Build");
            AddSLN(@"Builder");
            AddSLN(@"Compiler");
            AddSLN(@"Cosmos");
            AddSLN(@"IDE");
            AddSLN(@"Kernel");
            AddSLN(@"Test");
            AddSLN(@"XSharp");
            AddSLN(@"source\Tools\Tools");
        }

        void AddSLN(string aBaseName) {
            mSlnList.Add(IO.Path.Combine(mCosmosDir, aBaseName + ".sln"));
        }

        private void tboxRenameNewName_GotFocus(object sender, RoutedEventArgs e) {
            if (tboxRenameNewName.Text.Trim() == "") {
                tboxRenameNewName.Text = tboxRenameOldName.Text.Trim();
            }
        }

        string SlnProjectName(string aBase) {
            string xResult = "\") = \"" + aBase + "\", ";
            return xResult;
        }
        string SlnProjectPath(string aBase) {
            // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Cosmos.Core_Plugs", "source\Cosmos.Core_Plugs\Cosmos.Core_Plugs.csproj", "{1132E689-18B0-4D87-94E8-934D4802C540}"
            string xResult = ", \"source\\" + aBase + @"\" + aBase + ".csproj\", ";
            return xResult;
        }

        List<string> GetFiles(string aDir, string aWildCard) {
            string xArchive = IO.Path.Combine(aDir, @"source\archive\");
            var xFiles = IO.Directory.GetFiles(aDir, aWildCard, IO.SearchOption.AllDirectories);

            var xResult = new List<string>();
            foreach (var x in xFiles) {
                if (!x.StartsWith(xArchive, StringComparison.InvariantCultureIgnoreCase)) {
                    xResult.Add(x);
                }
            }
            return xResult;
        }

        void FixCsprojs() {
            Log("Fix references in .csproj files");

            // Change to mSourceDir after we move tests
            var xProjs = GetFiles(mCosmosDir, "*.csproj");
            foreach (var xProj in xProjs) {
                string x = IO.File.ReadAllText(xProj);
                string y = x.Replace(mOld, mNew);
                if (x != y) {
                    Log("  " + IO.Path.GetFileName(xProj));
                    IO.File.WriteAllText(xProj, y);
                }
            }

            Log();
        }

        void FixCs() {
            Log("Fix namespaces in .cs files");

            // Change to mSourceDir after we move tests
            var xProjs = GetFiles(mCosmosDir, "*.cs");
            foreach (var xProj in xProjs) {
                if (IO.Path.GetDirectoryName(xProj).EndsWith(@"\ProjectRenamer")) {
                    continue;
                }

                string x = IO.File.ReadAllText(xProj);
                string y = x.Replace(mOld, mNew);
                if (x != y) {
                    Log("  " + IO.Path.GetFileName(xProj));
                    IO.File.WriteAllText(xProj, y);
                }
            }

            Log();
        }

        void FixIss() {
            Log("Fix in .iss files");

            var xProjs = GetFiles(mSourceDir, "*.iss");
            foreach (var xProj in xProjs) {
                string x = IO.File.ReadAllText(xProj);
                string y = x.Replace(mOld, mNew);
                if (x != y) {
                    Log("  " + IO.Path.GetFileName(xProj));
                    IO.File.WriteAllText(xProj, y);
                }
            }

            Log();
        }

        void FixCosmos() {
            Log("Fix in .Cosmos files");

            // Change to mSourceDir after we move tests
            var xProjs = GetFiles(mCosmosDir, "*.Cosmos");
            foreach (var xProj in xProjs) {
                string x = IO.File.ReadAllText(xProj);
                string y = x.Replace(mOld, mNew);
                if (x != y) {
                    Log("  " + IO.Path.GetFileName(xProj));
                    IO.File.WriteAllText(xProj, y);
                }
            }

            Log();
        }

        void RenameProj() {
            Log("Renaming project");

            Log("  Renaming directory");
            string xProjDir = IO.Path.Combine(mSourceDir, mOld);
            if (!IO.Directory.Exists(xProjDir)) {
                MessageBox.Show("Cannot locate directory: " + xProjDir);
            }
            string xNewProjDir = IO.Path.Combine(mSourceDir, mNew);
            IO.Directory.Move(xProjDir, xNewProjDir);

            // Rename project file
            Log("  Project: " + mNew);
            string xProjPath = IO.Path.Combine(xNewProjDir, mOld + ".csproj");
            string xProjPathNew = IO.Path.Combine(xNewProjDir, mNew + ".csproj");
            IO.File.Move(xProjPath, xProjPathNew);

            Log();
        }

        void ModifySLNs() {
            Log("Modify project names in each SLN.");

            foreach (var xSLN in mSlnList) {
                string xSlnPath = IO.Path.Combine(mCosmosDir, xSLN);
                var xLines = IO.File.ReadAllLines(xSlnPath);
                bool xSlnChanged = false;

                for (int i = 0; i < xLines.Length; i++) {
                    // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Cosmos.Core_Plugs", "source\Cosmos.Core_Plugs\Cosmos.Core_Plugs.csproj", "{1132E689-18B0-4D87-94E8-934D4802C540}"
                    string xLine = xLines[i];
                    if (xLine.StartsWith("Project(") && xLine.Contains(mOld)) {
                        xLine = xLine.Replace(SlnProjectName(mOld), SlnProjectName(mNew));
                        xLine = xLine.Replace(SlnProjectPath(mOld), SlnProjectPath(mNew));
                    }
                    xSlnChanged = xSlnChanged || (xLine != xLines[i]);
                    if (xSlnChanged) {
                        xLines[i] = xLine;
                    }
                }

                // Avoid changing timestamp if no actual changes.
                if (xSlnChanged) {
                    Log("  " + xSLN);
                    IO.File.WriteAllLines(xSlnPath, xLines);
                }
            }

            Log();
        }

        private void butnRename_Click(object sender, RoutedEventArgs e) {
            LogClear();

            mOld = tboxRenameOldName.Text.Trim();
            mNew = tboxRenameNewName.Text.Trim();
            if (mOld == "" || mNew == "") {
                MessageBox.Show("Old and new cannot be empty.");
                return;
            }

            RenameProj();
            FixCsprojs(); // After RenameProj()
            FixCs();
            FixIss();
            FixCosmos();

            ModifySLNs();

            MessageBox.Show("Done.");
        }

        void LogClear() {
            tboxLog.Clear();
        }
        void Log(string aMsg = "") {
            tboxLog.Text = tboxLog.Text + aMsg + "\r\n";
            tboxLog.SelectionStart = tboxLog.Text.Length;
        }
    }
}

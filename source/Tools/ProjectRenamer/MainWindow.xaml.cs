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
        List<string> mSlnList = new List<string>();

        public MainWindow() {
            InitializeComponent();

            tboxRenameOldName.Text = "Cosmos.Core.Plugs";
            tboxRenameNewName.Text = "Cosmos.Core_Plugs";

            mCosmosDir = IO.Path.GetFullPath(IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\.."));
            tblkCosmosDir.Text = tblkCosmosDir.Text + mCosmosDir;
            //
            mSourceDir = IO.Path.Combine(mCosmosDir, "source");
            tblkSourceDir.Text = tblkSourceDir.Text + mSourceDir;

            AddSLN(@"Build");
            AddSLN(@"Builder");
            AddSLN(@"Compiler");
            AddSLN(@"Cosmos");
            AddSLN(@"IDE");
            AddSLN(@"Kernel");
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
            // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Cosmos.Core.Plugs", "source\Cosmos.Core.Plugs\Cosmos.Core.Plugs.csproj", "{1132E689-18B0-4D87-94E8-934D4802C540}"
            string xResult = ", \"source\\" + aBase + @"\" + aBase + ".csproj\", ";
            return xResult;
        }
        private void butnRename_Click(object sender, RoutedEventArgs e) {
            // In future may do more of a line parse, but for now its a bit hacky because it works
            // and this is not a core tool, but simply needs to "work".

            string xOld = tboxRenameOldName.Text.Trim();
            string xNew = tboxRenameNewName.Text.Trim();

            if (xOld == "" || xNew == "") {
                MessageBox.Show("Old and new cannot be empty.");
                return;
            }

            // Rename project directory
            string xProjDir = IO.Path.Combine(mSourceDir, xOld);
            if (!IO.Directory.Exists(xProjDir)) {
                MessageBox.Show("Cannot locate directory: " + xProjDir);
            }
            string xNewProjDir = IO.Path.Combine(mSourceDir, xNew);
            IO.Directory.Move(xProjDir, xNewProjDir);

            // Rename project file
            string xProjFile = IO.Path.Combine(xNewProjDir, xOld + ".csproj");
            string xNewProjFile = IO.Path.Combine(xNewProjDir, xNew + ".csproj");
            IO.File.Move(xProjFile, xNewProjFile);

            // TODO Fix namespaces in cs files
            // Default NS in csproj files too

            // Modify project names in each SLN
            foreach (var xSLN in mSlnList) {
                string xSlnPath = IO.Path.Combine(mCosmosDir, xSLN);
                var xLines = IO.File.ReadAllLines(xSlnPath);
                bool xSlnChanged = false;

                for (int i = 0; i < xLines.Length; i++) {
                    // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Cosmos.Core.Plugs", "source\Cosmos.Core.Plugs\Cosmos.Core.Plugs.csproj", "{1132E689-18B0-4D87-94E8-934D4802C540}"
                    string xLine = xLines[i];
                    if (xLine.StartsWith("Project(") && xLine.Contains(xOld)) {
                        xLine = xLine.Replace(SlnProjectName(xOld), SlnProjectName(xNew));
                        xLine = xLine.Replace(SlnProjectPath(xOld), SlnProjectPath(xNew));
                    }
                    xSlnChanged = xSlnChanged || (xLine != xLines[i]);
                    if (xSlnChanged) {
                        xLines[i] = xLine;
                    }
                }

                // Avoid changing timestamp if no actual changes.
                if (xSlnChanged) {
                    IO.File.WriteAllLines(xSlnPath, xLines);
                }
            }

            MessageBox.Show("Done.");
        }
    }
}

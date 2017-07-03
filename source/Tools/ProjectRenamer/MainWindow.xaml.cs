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
        List<string> mSlnList = new List<string>();

        public MainWindow() {
            InitializeComponent();

            tboxRenameOldName.Text = "Cosmos.Core.Plugs";
            tboxRenameNewName.Text = "Cosmos.Core_Plugs";

            mCosmosDir = IO.Path.GetFullPath(IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\.."));
            tblkCosmosDir.Text = tblkCosmosDir.Text + mCosmosDir;

            AddSLN(@"Build.sln");
            AddSLN(@"Builder.sln");
            AddSLN(@"Compiler.sln");
            AddSLN(@"Cosmos.sln");
            AddSLN(@"IDE.sln");
            AddSLN(@"Kernel.sln");
            AddSLN(@"XSharp.sln");
            AddSLN(@"source\Tools\Tools.sln");
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
            return "\") = \"" + aBase + "\", ";
        }
        string SlnProjectPath(string aBase) {
            // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Cosmos.Core.Plugs", "source\Cosmos.Core.Plugs\Cosmos.Core.Plugs.csproj", "{1132E689-18B0-4D87-94E8-934D4802C540}"
            return ", \"source\\" + aBase + ".csproj\", ";
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

            foreach (var xSLN in mSlnList) {
                string xSlnPath = IO.Path.Combine(mCosmosDir, xSLN);
                var xLines = IO.File.ReadAllLines(xSlnPath);
                bool xChanged = false;

                for(int i = 0; i < xLines.Length; i++) {
                    // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Cosmos.Core.Plugs", "source\Cosmos.Core.Plugs\Cosmos.Core.Plugs.csproj", "{1132E689-18B0-4D87-94E8-934D4802C540}"
                    string x = xLines[i];
                    if (x.StartsWith("Project(")) {
                        x = x.Replace(SlnProjectName(xOld), SlnProjectName(xNew));
                        x = x.Replace(SlnProjectPath(xOld), SlnProjectPath(xNew));
                    }
                    xChanged = (x != xLines[i]) || xChanged;
                }

                // Avoid changing timestamp if no actual changes.
                if (xChanged) {
                    IO.File.WriteAllLines(xSlnPath, xLines);
                }
            }
        }
    }
}

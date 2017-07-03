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

        private void butnRename_Click(object sender, RoutedEventArgs e) {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Build.StandAlone {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void butnBuild_Click(object sender, EventArgs e) {
            var xIL2CPU = new Cosmos.Build.MSBuild.IL2CPU();

        }
    }
}

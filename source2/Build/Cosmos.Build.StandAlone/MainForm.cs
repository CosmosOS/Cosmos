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
            xIL2CPU.EmitDebugSymbols = true;
            xIL2CPU.OutputFilename = @"m:\temp\Cosmos.asm";
            //<Framework>MicrosoftNET</Framework>
            xIL2CPU.UseNAsm = true;
            //<UseInternalAssembler>False</UseInternalAssembler>
            //<DebugMode>Source</DebugMode>
            //<TraceMode>
            //</TraceMode>
            //<TraceAssemblies>Cosmos</TraceAssemblies>
            xIL2CPU.Execute();
        }
    }
}

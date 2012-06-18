using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace XSharpCompilerTester {
  public partial class MainForm : Form {
    public MainForm() {
      InitializeComponent();
      // TODO convert to app path + relative
      // D:\source\Cosmos\source2\Tests\XSharpCompilerTester\bin\Debug
      // D:\source\Cosmos\source2\Users\Matthijs\MatthijsPlayground
    }

    protected void Test(string aFilename) {
      string xInputString = File.ReadAllText(@"D:\source\Cosmos\source2\Compiler\Cosmos.Compiler.DebugStub\" + aFilename);
      tboxInput.Text = xInputString;
      using (var xInput = new StringReader(xInputString)) {
        using (var xOutput = new StringWriter()) {
          var xGenerator = new Cosmos.Compiler.XSharp.Generator();
          xGenerator.Execute("DefaultNamespace", "InputFileName", xInput, xOutput);

          textOutput.Text = xOutput.ToString();
        }
      }
    }

    protected void textInput_TextChanged(object sender, EventArgs e) {
      timerConvert.Enabled = false;
      timerConvert.Enabled = true;
    }

    protected void timerConvert_Tick(object sender, EventArgs e) {
      timerConvert.Enabled = false;
      Test("TracerEntry.xs");
      Test("Serial.xs");
      Test("Screen.xs");
    }
  }
}

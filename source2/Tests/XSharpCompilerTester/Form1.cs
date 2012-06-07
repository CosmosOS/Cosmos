using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace XSharpCompilerTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textInput_TextChanged(object sender, EventArgs e)
        {
            timerConvert.Enabled = false;
            timerConvert.Enabled = true;

        }

        private void timerConvert_Tick(object sender, EventArgs e)
        {
            timerConvert.Enabled = false;
            using (var xInput = new StringReader(textInput.Text))
            {
                using (var xOut = new StringWriter())
                {
                  Cosmos.Compiler.XSharp.Generator.Execute(xInput, "InputFileName", xOut, "Default.Namespace");
                    textOutput.Text = xOut.ToString();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PCIHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var file = File.OpenText("PCIVendors.txt");// Source:  http://www.pcidatabase.com/vendors.php?sort=id
            var sb = new StringBuilder("");
            while (!file.EndOfStream)
            {
                string s=file.ReadLine();
                sb.AppendLine("mVendors.Add("+s.Substring(0, 6) + ",\"" + s.Substring(7, s.Length - 8) + "\");");                
            }
            Debug.Write(sb);
            //Used for Cosmos.Hardware.PCIBus.DeviceIDs
            Clipboard.SetText(sb.ToString());
        }
    }
}

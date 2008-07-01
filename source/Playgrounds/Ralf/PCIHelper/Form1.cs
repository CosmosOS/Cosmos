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

        private void button2_Click(object sender, EventArgs e)
        {
            var file = File.OpenText("pci.csv");// Source:  http://www.pcidatabase.com
            var sb = new StringBuilder("");
            char[] sep = new char[] {';'};
            Dictionary<string, string> devices = new Dictionary<string, string>();
            while (!file.EndOfStream)
            {

                string s = file.ReadLine().Replace("\",\"", "\";\""); //Commas are inside ...
                var tabs = s.Split(sep, 10, StringSplitOptions.None);
                if (!devices.ContainsKey(tabs[0] + tabs[1]))
                {
                    string description="";
                    devices.Add(tabs[0] + tabs[1], tabs[2]);
                    if (tabs[3].Length > 3)
                    {
                        description = tabs[3];
                    }
                    else
                    {
                        description = tabs[4];
                        
                    }
                    if (description.Length > 3)
                    {
                        if (CheckTabForHexValue(tabs[0]) && CheckTabForHexValue(tabs[1]))
                        {
                            if ((description.StartsWith("\"")) && (description.EndsWith("\"")))
                            {
                                sb.AppendLine(string.Format("//mDevices.Add({0}{1},@{2});",
                                                            tabs[0].Substring(1, tabs[0].Length - 2),
                                                            tabs[1].Substring(3, tabs[1].Length - 4), description));
                            }
                            else
                            {
                                Debug.WriteLine("Wrong entry:" + s);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Wrong entry:" + s);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Wrong entry:" + s);
                    }
                }
                else
                {
                    Debug.WriteLine("Double entry:" + s);
                }
            }
            Clipboard.SetText(sb.ToString());
        }

        private bool CheckTabForHexValue(string tab)
        {
            if (tab.Length != 8)
                return false;
            if (!tab.StartsWith("\"0x"))
                return false;
            if (!tab.EndsWith("\""))
                return false;
            for (int i = 3; i < 7; i++)
            {
                switch (tab[i])
                {
                    case '0':
                        break;
                    case '1':
                        break;
                    case '2':
                        break;
                    case '3':
                        break;
                    case '4':
                        break;
                    case '5':
                        break;
                    case '6':
                        break;
                    case '7':
                        break;
                    case '8':
                        break;
                    case '9':
                        break;
                    case 'A':
                        break;
                    case 'B':
                        break;
                    case 'C':
                        break;
                    case 'D':
                        break;
                    case 'E':
                        break;
                    case 'F':
                        break;
                    default:
                        return false;
                }               
            }
            return true;            
        }
    }
}

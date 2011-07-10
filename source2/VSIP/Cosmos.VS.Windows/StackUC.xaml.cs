using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cosmos.Cosmos_VS_Windows
{
    public partial class StackUC : UserControl
    {
        public StackUC()
        {
            InitializeComponent();

            tboxSourceFrame.Text = "";
            tboxSourceStack.Text = "";
        }

        public void UpdateFrame(byte[] aData)
        {
            string xData = BitConverter.ToString(aData);
            xData = xData.Trim();
            xData = xData.Replace("-", "");

            var xSB = new StringBuilder();
            for (int i = 0; i < aData.Length; i += 8) {
              xSB.Insert(0, "[EBP + " + (i / 8) + "] 0x" + xData.Substring(i, 8) + "\n");
            }
            xSB.Insert(0, "Arguments\n");
            tboxSourceFrame.Text = xSB.ToString();
        }

        public void UpdateStack(byte[] aData)
        {
            string xData = BitConverter.ToString(aData);
            xData = xData.Trim();
            xData = xData.Replace("-", "");

            var xSB = new StringBuilder();
            xSB.AppendLine("Stack Contents");
            for (int i = 0; i < xData.Length; i += 8) {
              xSB.AppendLine("[EBP - " + ((i / 8) + 4) + "] 0x" + xData.Substring(i, 8));
            }
            tboxSourceStack.Text = xSB.ToString();
        }
    }
}
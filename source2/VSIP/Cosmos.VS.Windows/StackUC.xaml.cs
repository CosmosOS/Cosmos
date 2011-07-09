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
    /// <summary>
    /// Interaction logic for StackUC.xaml
    /// </summary>
    public partial class StackUC : UserControl
    {
        public StackUC()
        {
            InitializeComponent();
        }

        public void Update(byte[] aData)
        {
            int xOffset = 124;
            string xData = BitConverter.ToString(aData);
            xData = xData.Trim();
            xData = xData.Replace("-", "");
            if (xData.Length == 512)
            {
                for (int i = 0; i < 512; i += 8)
                {
                    string xTemp = xData.Substring(i, 8);
                    tboxSource.Text += ("EBP + " + xOffset.ToString() + " : " + xTemp + "\n");
                    xOffset -= 4;
                }
            }
            else tboxSource.Text = "Error loading the frame.";
        }
    }
}
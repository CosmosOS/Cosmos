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
using Cosmos.Compiler.Debug;
using System.Windows.Threading;
using Cosmos.VS.Debug;
using System.Threading;

namespace Cosmos.Cosmos_VS_Windows
{
    public partial class AssemblyUC : UserControl
    {
        public AssemblyUC()
        {
            InitializeComponent();
        }

        public void Update(byte[] aData)
        {
            string xData = Encoding.ASCII.GetString(aData);
            xData = xData.Replace("\t", "  ");
            tboxSource.Text = xData;
        }
    }
}
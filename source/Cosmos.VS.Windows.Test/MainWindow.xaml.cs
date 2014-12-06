using System.Windows;

namespace Cosmos.VS.Windows.Test
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string xPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..");

            // Uncomment code in AssemblyUC.xml.cs to genearate a new bin test file in method DoUpdate.
            // Make sure to comment back out before running this, else there will be conflicts
            byte[] xData = System.IO.File.ReadAllBytes(System.IO.Path.Combine(xPath, "SourceTest.bin"));
            ucAssembly.Update(null, xData);
        }
    }
}
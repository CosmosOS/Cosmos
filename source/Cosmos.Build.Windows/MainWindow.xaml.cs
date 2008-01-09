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

namespace Cosmos.Build.Windows {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window {
        public Window1() {
            InitializeComponent();
        }

        //open key as read only
        //
        //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework
        //sdkInstallRootv2.0
        //C:\Program Files\Microsoft.NET\SDK\v2.0 64bit\
        //
        //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\.NETFramework\v2.0
    }
}

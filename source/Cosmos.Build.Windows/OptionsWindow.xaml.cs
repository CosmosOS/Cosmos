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
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace Cosmos.Build.Windows {
    public partial class OptionsWindow : Window {

        [DllImport("user32.dll")]
        public static extern int ShowWindow(int Handle, int showState);
        [DllImport("kernel32.dll")]
        public static extern int GetConsoleWindow();
        
        protected Block mOptionsBlockPrefix;
        protected Builder mBuilder = new Builder();

        public static void Display() {
            int xConsoleWindow = GetConsoleWindow();
            ShowWindow(xConsoleWindow, 0);

            var xOptionsWindow = new OptionsWindow();
            if (xOptionsWindow.ShowDialog().Value) {
                ShowWindow(xConsoleWindow, 1);
                // Build Window is a hack to catch console out. We need
                // to change the build process to use proper event notifications
                // rather than just string output, and console at that.
                //
                // Doesnt currently work because we block the main thread
                // Need to change to be threaded, or use code from XAMLPoint
                //var xBuildWindow = new BuildWindow();
                //xBuildWindow.Show();

                xOptionsWindow.DoBuild();

                var xDebugWindow = new DebugWindow();
                xDebugWindow.ShowDialog();
                //Console.WriteLine("Press enter to continue.");
                //Console.ReadLine();
            }
        }

        public OptionsWindow() {
            InitializeComponent();

            Loaded += delegate(object sender, RoutedEventArgs e) {
                this.Activate();
            };

            butnBuild.Click += new RoutedEventHandler(butnBuild_Click);
            butnCancel.Click += new RoutedEventHandler(butnCancel_Click);

            rdioQEMU.Checked += new RoutedEventHandler(rdioTarget_Checked);
            rdioQEMU.Unchecked += new RoutedEventHandler(rdioTarget_Unchecked);
            rdioVMWare.Checked += new RoutedEventHandler(rdioVMWare_Checked);
            rdioVMWare.Unchecked += new RoutedEventHandler(rdioVMWare_Unchecked);
            rdioVPC.Checked += new RoutedEventHandler(rdioVPC_Checked);
            rdioVPC.Unchecked += new RoutedEventHandler(rdioVPC_Unchecked);
            rdioISO.Checked += new RoutedEventHandler(rdioISO_Checked);
            rdioISO.Unchecked += new RoutedEventHandler(rdioISO_Unchecked);
            rdioPXE.Checked += new RoutedEventHandler(rdioPXE_Checked);
            rdioPXE.Unchecked += new RoutedEventHandler(rdioPXE_Unchecked);
            rdioUSB.Checked += new RoutedEventHandler(rdioUSB_Checked);
            rdioUSB.Unchecked += new RoutedEventHandler(rdioUSB_Unchecked);

            spanBuildPath.Inlines.Add(mBuilder.BuildPath);
            spanISOPath.Inlines.Add(mBuilder.BuildPath + "Cosmos.iso");

            mOptionsBlockPrefix = paraQEMUOptions.PreviousBlock;
            //RootDoc.Blocks.Remove(paraQEMUOptions);
            RootDoc.Blocks.Remove(paraVMWareOptions);
            RootDoc.Blocks.Remove(paraVPCOptions);
            RootDoc.Blocks.Remove(paraISOOptions);
            RootDoc.Blocks.Remove(paraPXEOptions);
            RootDoc.Blocks.Remove(paraUSBOptions);

            var xDrives = System.IO.Directory.GetLogicalDrives();
            foreach (string xDrive in xDrives) {
                var xType = new System.IO.DriveInfo(xDrive);
                if (xType.IsReady) {
                    if ((xType.DriveType == System.IO.DriveType.Removable) && xType.DriveFormat.StartsWith("FAT")) {
                        cmboUSBDevice.Items.Add(xDrive);
                    }
                }
            }

            LoadSettingsFromRegistry();
        }

        void rdioUSB_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraUSBOptions);
        }
        void rdioUSB_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraUSBOptions);
        }

        void rdioISO_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraISOOptions);
        }
        void rdioISO_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraISOOptions);
        }

        void rdioVPC_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraVPCOptions);
        }
        void rdioVPC_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraVPCOptions);
        }

        void rdioVMWare_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraVMWareOptions);
        }
        void rdioVMWare_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraVMWareOptions);
        }

        void rdioPXE_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraPXEOptions);
        }
        void rdioPXE_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraPXEOptions);
        }

        void rdioTarget_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraQEMUOptions);
        }
        void rdioTarget_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraQEMUOptions);
        }

        void butnBuild_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        protected void DoBuild() {
            SaveSettingsToRegistry();

            if (!buildCheckBox.IsChecked.Value) {
                Console.WriteLine("Compiling...");
                mBuilder.Compile();
            }

            if (rdioQEMU.IsChecked.Value) {
                mBuilder.MakeQEMU(chckQEMUUseHD.IsChecked.Value, chckQEMUUseGDB.IsChecked.Value);
            } else if (rdioVMWare.IsChecked.Value) {
                mBuilder.MakeVMWare();
            } else if (rdioVPC.IsChecked.Value) {
                mBuilder.MakeVPC();
            } else if (rdioISO.IsChecked.Value) {
                mBuilder.MakeISO();
            } else if (rdioPXE.IsChecked.Value) {
                mBuilder.MakePXE();
            } else if (rdioUSB.IsChecked.Value) {
                mBuilder.MakeUSB(cmboUSBDevice.Text[0]);
            }
        }

        void butnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        void SaveSettingsToRegistry() {
            string xValue = "QEMU";
            if (rdioVMWare.IsChecked.Value) {
                xValue = "VMWare";
            } else if (rdioVPC.IsChecked.Value) {
                xValue = "VPC";
            } else if (rdioISO.IsChecked.Value) {
                xValue = "ISO";
            } else if (rdioPXE.IsChecked.Value) {
                xValue = "PXE";
            } else if (rdioUSB.IsChecked.Value) {
                xValue = "USB";
            }
            BuildRegistry.Write("Build Type", xValue);
            
            BuildRegistry.Write("Use GDB", chckQEMUUseGDB.IsChecked.Value.ToString());
            BuildRegistry.Write("Create HD Image", chckQEMUUseHD.IsChecked.Value.ToString());
            BuildRegistry.Write("Skip IL", buildCheckBox.IsChecked.Value.ToString());
            if (cmboUSBDevice.SelectedItem != null) {
                BuildRegistry.Write("USB Device", cmboUSBDevice.Text);
            }
        }

        void LoadSettingsFromRegistry() {
            string xBuildType = BuildRegistry.Read("Build Type");
            switch (xBuildType) {
                case "QEMU":
                    rdioQEMU.IsChecked = true;
                    break;
                case "VMWare":
                    rdioVMWare.IsChecked = true;
                    break;
                case "VPC":
                    rdioVPC.IsChecked = true;
                    break;
                case "ISO":
                    rdioISO.IsChecked = true;
                    break;
                case "PXE":
                    rdioPXE.IsChecked = true;
                    break;
                case "USB":
                    rdioUSB.IsChecked = true;
                    break;
            }

            bool useGDB;
            bool.TryParse(BuildRegistry.Read("Use GDB"), out useGDB);
            chckQEMUUseGDB.IsChecked = useGDB;

            bool createHDimg;
            bool.TryParse(BuildRegistry.Read("Create HD Image"), out createHDimg);
            chckQEMUUseHD.IsChecked = createHDimg;

            bool skipIL;
            bool.TryParse(BuildRegistry.Read("Skip IL"), out skipIL);
            buildCheckBox.IsChecked = skipIL;

            string xUSBDevice = BuildRegistry.Read("USB Device");
            cmboUSBDevice.SelectedIndex = cmboUSBDevice.Items.IndexOf(xUSBDevice);
        }

    }
}

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
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
	public partial class OptionsWindow: Window {

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

				//Debug Window is only displayed if Qemu + Debug checked, or if other VM + Debugport selected
				bool xIsQemu = xOptionsWindow.rdioQEMU.IsChecked.Value;
				bool xUseQemuDebug = xOptionsWindow.chckQEMUUseDebug.IsChecked.Value;
				int xDebugPort = xOptionsWindow.cmboDebugPort.SelectedIndex;
				if ((xIsQemu & xUseQemuDebug) | (!xIsQemu & (xDebugPort > 0))) {
					var xDebugWindow = new DebugWindow();
					xDebugWindow.ShowDialog();
				}
			}
		}

		protected void AddSection(params Paragraph[] aParagraphs) {
			foreach (var xPara in aParagraphs) {
				RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, xPara);
			}
		}

		protected void TargetChanged(object aSender, RoutedEventArgs e) {
			RootDoc.Blocks.Remove(paraDebugOptions);
			RootDoc.Blocks.Remove(paraQEMUOptions);
			RootDoc.Blocks.Remove(paraVMWareOptions);
			RootDoc.Blocks.Remove(paraVPCOptions);
			RootDoc.Blocks.Remove(paraISOOptions);
			RootDoc.Blocks.Remove(paraPXEOptions);
			RootDoc.Blocks.Remove(paraUSBOptions);

			if (aSender == rdioUSB) {
				AddSection(paraDebugOptions);
				AddSection(paraUSBOptions);
			} else if (aSender == rdioISO) {
				AddSection(paraDebugOptions);
				AddSection(paraISOOptions);
			} else if (aSender == rdioVPC) {
				AddSection(paraDebugOptions);
				AddSection(paraVPCOptions);
			} else if (aSender == rdioVMWare) {
				AddSection(paraDebugOptions);
				AddSection(paraVMWareOptions);
			} else if (aSender == rdioPXE) {
				AddSection(paraDebugOptions);
				AddSection(paraPXEOptions);
			} else if (aSender == rdioQEMU) {
				AddSection(paraDebugOptions);
				AddSection(paraQEMUOptions);
			}
		}

		public OptionsWindow() {
			InitializeComponent();
			mOptionsBlockPrefix = paraGeneralOptions; // paraQEMUOptions.PreviousBlock;

			Loaded += delegate(object sender, RoutedEventArgs e) {
				this.Activate();
			};

			butnBuild.Click += new RoutedEventHandler(butnBuild_Click);
			butnCancel.Click += new RoutedEventHandler(butnCancel_Click);

			rdioQEMU.Checked += new RoutedEventHandler(TargetChanged);
			rdioVMWare.Checked += new RoutedEventHandler(TargetChanged);
			rdioVPC.Checked += new RoutedEventHandler(TargetChanged);
			rdioISO.Checked += new RoutedEventHandler(TargetChanged);
			rdioPXE.Checked += new RoutedEventHandler(TargetChanged);
			rdioUSB.Checked += new RoutedEventHandler(TargetChanged);

			chckQEMUUseDebug.Checked += new RoutedEventHandler(chckQEMUUseDebug_Checked);
			chckQEMUUseDebug.Unchecked += new RoutedEventHandler(chckQEMUUseDebug_Unchecked);

			spanBuildPath.Inlines.Add(mBuilder.BuildPath);
			spanISOPath.Inlines.Add(mBuilder.BuildPath + "Cosmos.iso");

			var xDrives = System.IO.Directory.GetLogicalDrives();
			foreach (string xDrive in xDrives) {
				var xType = new System.IO.DriveInfo(xDrive);
				if (xType.IsReady) {
					if ((xType.DriveType == System.IO.DriveType.Removable) && xType.DriveFormat.StartsWith("FAT")) {
						cmboUSBDevice.Items.Add(xDrive);
					}
				}
			}

			cmboDebugPort.Items.Add("Disabled");
			// MtW: for now, leave COM1 out, as COM1 is used by the Cosmos kernel to output debug messages
			//cmboDebugPort.Items.Add("COM1");
			cmboDebugPort.SelectedIndex = cmboDebugPort.Items.Add("COM2");
			cmboDebugPort.Items.Add("COM3");
			cmboDebugPort.Items.Add("COM4");
			cmboDebugPort.Items.Add("Ethernet 1");
			cmboDebugPort.Items.Add("Ethernet 2");
			cmboDebugPort.Items.Add("Ethernet 3");
			cmboDebugPort.Items.Add("Ethernet 4");

			cmboDebugMode.Items.Add("IL");
			cmboDebugMode.SelectedIndex = cmboDebugMode.Items.Add("Source");

			LoadSettingsFromRegistry();
		}

		void chckQEMUUseDebug_Unchecked(object sender, RoutedEventArgs e) {
			chckQEMUSerialWait.IsChecked = false;
			chckQEMUSerialWait.IsEnabled = true;
		}

		void chckQEMUUseDebug_Checked(object sender, RoutedEventArgs e) {
			chckQEMUSerialWait.IsChecked = true;
			chckQEMUSerialWait.IsEnabled = false;
		}

		void butnBuild_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		protected void DoBuild() {
            SaveSettingsToRegistry();

            if (chckCompileIL.IsChecked.Value) {
                Console.WriteLine("Compiling...");
				byte xComport = (byte)cmboDebugPort.SelectedIndex;
				if (xComport > 3) {
					throw new Exception("Debug port not supported yet!");
				}
				DebugModeEnum xDebugMode = DebugModeEnum.None;
				if (cmboDebugMode.SelectedIndex == 0) {
					xDebugMode = DebugModeEnum.IL;
				} else {
					if (cmboDebugMode.SelectedIndex == 1) {
						xDebugMode = DebugModeEnum.Source;
					} else {
						throw new Exception("Selected debug mode not supported!");
					}
				}
				var xBuildWindow = new BuildWindow();
				mBuilder.DebugLog += xBuildWindow.DoDebugMessage;
				xBuildWindow.Show();
                mBuilder.Compile(xDebugMode, xComport);
				mBuilder.DebugLog -= xBuildWindow.DoDebugMessage;
				xBuildWindow.Close();
            }

            if (rdioQEMU.IsChecked.Value) {
                mBuilder.MakeQEMU(chckQEMUUseHD.IsChecked.Value, chckQEMUUseGDB.IsChecked.Value, chckQEMUUseDebug.IsChecked.Value, chckQEMUUseDebug.IsChecked.Value);
            } else if (rdioVMWare.IsChecked.Value) {
                string vmwareversion = string.Empty;
                
                if (rdVMWareServer.IsChecked.Value) 
                    vmwareversion = rdVMWareServer.Content.ToString();
                else if (rdVMWareWorkstation.IsChecked.Value)
                    vmwareversion = rdVMWareWorkstation.Content.ToString();

                mBuilder.MakeVMWare(vmwareversion);
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

		protected const string mRegKey = @"Software\Cosmos\User Kit";
		protected void SaveSettingsToRegistry() {
			using (var xKey = Registry.CurrentUser.CreateSubKey(mRegKey)) {
				string xTarget = "QEMU";
				if (rdioVMWare.IsChecked.Value) {
					xTarget = "VMWare";
				} else if (rdioVPC.IsChecked.Value) {
					xTarget = "VPC";
				} else if (rdioISO.IsChecked.Value) {
					xTarget = "ISO";
				} else if (rdioPXE.IsChecked.Value) {
					xTarget = "PXE";
				} else if (rdioUSB.IsChecked.Value) {
					xTarget = "USB";
				}
				xKey.SetValue("Target", xTarget);

				// General
				xKey.SetValue("Compile IL", chckCompileIL.IsChecked.Value, RegistryValueKind.DWord);
				xKey.SetValue("Debug Port", cmboDebugPort.Text);

				// QEMU
				xKey.SetValue("Use GDB", chckQEMUUseGDB.IsChecked.Value, RegistryValueKind.DWord);
				xKey.SetValue("Use Debugger", chckQEMUUseDebug.IsChecked.Value, RegistryValueKind.DWord);
				xKey.SetValue("Create HD Image", chckQEMUUseHD.IsChecked.Value, RegistryValueKind.DWord);
				xKey.SetValue("Wait for Serial TCP", chckQEMUSerialWait.IsChecked.Value, RegistryValueKind.DWord);

				// VMWare
				string xVMWareVersion = string.Empty;
				if (rdVMWareServer.IsChecked.Value)
					xVMWareVersion = "VMWare Server";
				else if (rdVMWareWorkstation.IsChecked.Value)
					xVMWareVersion = "VMWare Workstation";
				xKey.SetValue("VMWare Version", xVMWareVersion);


				// USB
				if (cmboUSBDevice.SelectedItem != null) {
					xKey.SetValue("USB Device", cmboUSBDevice.Text);
				}
			}
		}

		void LoadSettingsFromRegistry() {
			using (var xKey = Registry.CurrentUser.CreateSubKey(mRegKey)) {
				string xBuildType = (string)xKey.GetValue("Target", "QEMU");
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

				// General
				chckCompileIL.IsChecked = ((int)xKey.GetValue("Compile IL", 1) != 0);
				cmboDebugPort.SelectedIndex = cmboDebugPort.Items.IndexOf(xKey.GetValue("Debug Port", ""));
				if (cmboDebugPort.SelectedIndex == -1) {
					cmboDebugPort.SelectedIndex = 0;
				}

				// QEMU
				chckQEMUUseGDB.IsChecked = ((int)xKey.GetValue("Use GDB", 0) != 0);
				chckQEMUUseDebug.IsChecked = ((int)xKey.GetValue("Use Debugger", 1) != 0);
				chckQEMUUseHD.IsChecked = ((int)xKey.GetValue("Create HD Image", 0) != 0);
				chckQEMUSerialWait.IsChecked = ((int)xKey.GetValue("Wait for Serial TCP", 0) != 0);

				// VMWare
				string xVMWareVersion = (string)xKey.GetValue("VMWare Version", "VMWare Server");
				switch (xVMWareVersion) {
					case "VMWare Server":
						rdVMWareServer.IsChecked = true;
						break;
					case "VMWare Workstation":
						rdVMWareWorkstation.IsChecked = true;
						break;
				}

				// USB
				cmboUSBDevice.SelectedIndex = cmboUSBDevice.Items.IndexOf(xKey.GetValue("USB Device", ""));
			}
		}

	}
}

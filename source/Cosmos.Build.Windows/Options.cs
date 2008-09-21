using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
 
    public class Options {
        public const string RegKey = @"Software\Cosmos\User Kit";

        public void Load() {
            using (var xKey = Registry.CurrentUser.OpenSubKey(Options.RegKey, false)) {
                object xValue;
                if (xKey == null) {
                    CompileIL = true;
                    Target = "QEMU";
                    ShowOptions = true;
                    NetworkCard = Builder.QemuNetworkCard.rtl8139.ToString();
                    AudioCard = Builder.QemuAudioCard.es1370.ToString();
                    return; 
                }
                //TODO: Use attributes or just name and reflection to save/load
                xValue = xKey.GetValue("Debug Trace Assemblies");
                if (xValue != null) {
                    TraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), (string)xValue, true);
                } else {
                    TraceAssemblies = TraceAssemblies.Cosmos;
                }
                Target = (string)xKey.GetValue("Target");
                DebugPort = (string)xKey.GetValue("Debug Port");
                DebugMode = (string)xKey.GetValue("Debug Mode");
                UseGDB = Boolean.Parse((string)xKey.GetValue("UseGDB", "false"));
                CreateHDImage = Boolean.Parse((string)xKey.GetValue("Use HD Image", "false"));
                UseNetworkTAP = Boolean.Parse((string)xKey.GetValue("Use TAP", "false"));
                NetworkCard = (string)xKey.GetValue("NetworkCard", Builder.QemuNetworkCard.rtl8139.ToString());
                AudioCard = (string)xKey.GetValue("AudioCard", Builder.QemuAudioCard.es1370.ToString());
                VMWareEdition = (string)xKey.GetValue("VMWare Edition");
                USBDevice = (string)xKey.GetValue("USB Device");
                ShowOptions = Boolean.Parse((string)xKey.GetValue("Show Options", "true"));
                ShowConsoleWindow = Boolean.Parse((string)xKey.GetValue("Show Console", "false"));
                CompileIL = Boolean.Parse((string)xKey.GetValue("CompileIL", "true"));
            }
        }

        public void Save() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(Options.RegKey)) {
                xKey.SetValue("Debug Trace Assemblies", TraceAssemblies.ToString());
                xKey.SetValue("Target", Target);
                xKey.SetValue("Debug Port", DebugPort);
                xKey.SetValue("Debug Mode", DebugMode);
                xKey.SetValue("UseGDB", UseGDB.ToString());
                xKey.SetValue("Use HD Image", CreateHDImage.ToString());
                xKey.SetValue("Use TAP", UseNetworkTAP.ToString());
                xKey.SetValue("NetworkCard", NetworkCard);
                xKey.SetValue("AudioCard", AudioCard);
                xKey.SetValue("VMWare Edition", VMWareEdition ?? "");
                xKey.SetValue("USB Device", USBDevice??"");
                xKey.SetValue("Show Options", ShowOptions.ToString());
                xKey.SetValue("Show Console", ShowConsoleWindow.ToString());
                xKey.SetValue("CompileIL", CompileIL.ToString());
                xKey.Flush();
            }
        }
        
        public TraceAssemblies TraceAssemblies { get; set; }
        public string Target { get; set; }
        public string DebugPort { get; set; }
        public string DebugMode { get; set; }
        public bool UseGDB { get; set; }
        public bool CreateHDImage { get; set; }
        public bool UseNetworkTAP { get; set; }
        public string NetworkCard { get; set; }
        public string AudioCard { get; set; }
        public string VMWareEdition { get; set; }
        public string USBDevice { get; set; }
        public bool ShowOptions { get; set; }
        public bool ShowConsoleWindow { get; set; }
        public bool CompileIL { get; set; }
    }
}

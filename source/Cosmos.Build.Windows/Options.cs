using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Compiler.Builder {
 
    public class Options {
        protected const string RegKey = @"Software\Cosmos\User Kit";
        
        protected object ReadEnum(RegistryKey aKey, string aName, object aDefault) {
            object xValue = aKey.GetValue(aName);
            if (xValue != null) {
                return Enum.Parse(aDefault.GetType(), (string)xValue, true);
            } else {
                return aDefault;
            }
        }
        
        public void Load() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(Options.RegKey)) {
                //TODO: Use attributes or just name and reflection to save/load
                TraceAssemblies = (TraceAssemblies)ReadEnum(xKey, "Debug Trace Assemblies", TraceAssemblies.Cosmos);
                DebugMode = (DebugMode)ReadEnum(xKey, "Debug Mode", DebugMode.Source);
                //TODO: Use attributes or just name and reflection to save/load
                TraceAssemblies = (TraceAssemblies)ReadEnum(xKey, "Debug Trace Assemblies", TraceAssemblies.Cosmos);
                DebugMode = (DebugMode)ReadEnum(xKey, "Debug Mode", DebugMode.Source);

                //TODO: all strings need converted to enums that are enums...
                Target = (string)xKey.GetValue("Target");
                DebugPort = (string)xKey.GetValue("Debug Port");
                UseGDB = Boolean.Parse((string)xKey.GetValue("UseGDB", "false"));
                CreateHDImage = Boolean.Parse((string)xKey.GetValue("Use HD Image", "false"));
                UseNetworkTAP = Boolean.Parse((string)xKey.GetValue("Use TAP", "false"));
                NetworkCard = (string)xKey.GetValue("NetworkCard", Builder.QemuNetworkCard.rtl8139.ToString());
                AudioCard = (string)xKey.GetValue("AudioCard", Builder.QemuAudioCard.es1370.ToString());
                VMWareEdition = (string)xKey.GetValue("VMWare Edition");
                USBDevice = (string)xKey.GetValue("USB Device");
                ShowOptions = Boolean.Parse((string)xKey.GetValue("Show Options", "true"));
                CompileIL = Boolean.Parse((string)xKey.GetValue("CompileIL", "true"));
            }
        }

        public void Save() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(Options.RegKey)) {
                xKey.SetValue("Debug Trace Assemblies", TraceAssemblies);
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
                xKey.SetValue("CompileIL", CompileIL.ToString());
                xKey.Flush();
            }
        }
        
        public TraceAssemblies TraceAssemblies { get; set; }
        public string Target { get; set; }
        public string DebugPort { get; set; }
        public DebugMode DebugMode { get; set; }
        public bool UseGDB { get; set; }
        public bool CreateHDImage { get; set; }
        public bool UseNetworkTAP { get; set; }
        public string NetworkCard { get; set; }
        public string AudioCard { get; set; }
        public string VMWareEdition { get; set; }
        public string USBDevice { get; set; }
        public bool ShowOptions { get; set; }
        public bool CompileIL { get; set; }
    }
}

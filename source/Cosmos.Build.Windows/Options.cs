using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Compiler.Builder {
 
    public class Options {
        protected const string RegKey = @"Software\Cosmos\User Kit";
        public Options() {
            CompileIL = true;
            Target = "QEMU";
            NetworkCard = Builder.QemuNetworkCard.rtl8139.ToString();
            AudioCard = Builder.QemuAudioCard.sb16.ToString();
        }

        protected TEnum ReadEnum<TEnum>(XmlDocument aDoc, string aName, TEnum aDefault) {

            string xValue = ReadValue(aDoc,
                                      aName,
                                      null);
            if (xValue != null) {
                return (TEnum)Enum.Parse(typeof(TEnum), xValue, true);
            } else {
                return aDefault;
            }
        }

        protected string ReadValue(XmlDocument aDoc, string aName, string aDefault) {
            var xElem = aDoc.SelectSingleNode("/settings/" + aName);
            if(xElem!=null) {
                return xElem.InnerText.Trim();
            }
            return aDefault;
        }

        protected static string ConfigDirName {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX) {
                    return ".cosmos";
                } else {
                    return "Cosmos";
                }
            }
        }

        public void Load() {
            var xDoc = new XmlDocument();
            var xFileName = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                      ConfigDirName),
                                         "BuilderConfig.xml");
            if (!File.Exists(xFileName)) {
                return;
            }
            xDoc.Load(xFileName);
            TraceAssemblies = ReadEnum(xDoc,
                                       "DebugTraceAssemblies",
                                       Indy.IL2CPU.TraceAssemblies.Cosmos);
            DebugMode = ReadEnum(xDoc,
                                 "DebugMode",
                                 DebugMode.Source);
            Target = ReadValue(xDoc,
                               "Target",
                               "");
            DebugPort = ReadValue(xDoc,
                                  "DebugPort",
                                  "");
            UseGDB = Boolean.Parse(ReadValue(xDoc,
                                             "UseGDB",
                                             "false"));
            CreateHDImage = Boolean.Parse(ReadValue(xDoc,
                                                    "UseHDImage",
                                                    "False"));
            UseNetworkTAP = Boolean.Parse(ReadValue(xDoc,
                                                    "UseTAP",
                                                    "False"));
            // todo: make NetworkCard, AudioCard properties strongly typed
            NetworkCard = ReadValue(xDoc,
                                    "NetworkCard",
                                    Builder.QemuNetworkCard.rtl8139.ToString());
            AudioCard = ReadValue(xDoc,
                                  "AudioCard",
                                  Builder.QemuAudioCard.es1370.ToString());
            VMWareEdition = ReadValue(xDoc,
                                      "VMWareEdition",
                                      "");
            USBDevice = ReadValue(xDoc,
                                  "USBDevice",
                                  "");
            ShowOptions = Boolean.Parse(ReadValue(xDoc,
                                                  "ShowOptions",
                                                  "true"));
            CompileIL = Boolean.Parse(ReadValue(xDoc,
                                                "CompileIL",
                                                "true"));
        }

        public void Save() {
            var xFileName = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                     ConfigDirName),
                                        "BuilderConfig.xml");
            if (File.Exists(xFileName)) {
                File.Delete(xFileName);
            } else {
                if (!Directory.Exists(Path.GetDirectoryName(xFileName))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(xFileName));
                }
            }
            using (var xWriter = XmlWriter.Create(xFileName)) {
                xWriter.WriteStartDocument();
                xWriter.WriteStartElement("settings");
                Action<string, string> xWriteValue = delegate(string aKey,
                                                              string aValue) {
                                                         xWriter.WriteStartElement(aKey);
                                                         xWriter.WriteValue(aValue??"");
                                                         xWriter.WriteEndElement();
                                                     };
                xWriteValue("DebugTraceAssemblies",
                            TraceAssemblies.ToString());
                xWriteValue("Target",
                            Target);
                xWriteValue("DebugPort",
                            DebugPort);
                xWriteValue("DebugMode",
                            DebugMode.ToString());
                xWriteValue("UseGDB",
                            UseGDB.ToString());
                xWriteValue("UseHDImage",
                            CreateHDImage.ToString());
                xWriteValue("UseTAP",
                            UseNetworkTAP.ToString());
                xWriteValue("NetworkCard",
                            NetworkCard);
                xWriteValue("AudioCard",
                            AudioCard);
                xWriteValue("VMWareEdition",
                            VMWareEdition);
                xWriteValue("USBDevice",
                            USBDevice);
                xWriteValue("ShowOptions",
                            ShowOptions.ToString());
                xWriteValue("CompileIL",
                            CompileIL.ToString());
                xWriter.WriteEndDocument();
                xWriter.Flush();
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
        public string BuildPath {
            get;
            set;
        }
    }
}

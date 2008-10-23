using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using Indy.IL2CPU;
using System.Collections;

namespace Cosmos.Compiler.Builder {
 
    public class Options {
        public readonly static Hashtable QEmuAudioCard;
        public readonly static Hashtable QEmuNetworkCard;
        public readonly static Hashtable QEmuDebugComType;
        //public Hashtable VmWareDebugComType = new Hashtable();
        protected const string RegKey = @"Software\Cosmos\User Kit";
        static Options() {
            QEmuAudioCard=new Hashtable();
            QEmuNetworkCard=new Hashtable();
            QEmuDebugComType = new Hashtable();
            //public enum QemuAudioCard { pcspk, sb16, es1370, adlib }
            //public enum QemuNetworkCard { ne2k_pci, rtl8139, pcnet }
            QEmuAudioCard.Add("PC Speaker", "pcspk");
            //QEmuAudioCard.Add("Ensoniq ES1370 PCI", "es1370");
            //QEmuAudioCard.Add("SoundBlaster 16", "sb16");
            //QEmuAudioCard.Add("Adlib", "adlib");
            QEmuNetworkCard.Add("Realtek RTL8139", "rtl8139");
            //QEmuAudioCard.Add("ISA NE2000","ne2k_pci"
            //QEmuAudioCard.Add("PCnet","pcnet");
            QEmuDebugComType.Add("Named pipe: Cosmos Debugger as client, QEmu as server", "-serial pipe:CosmosDebug");
            QEmuDebugComType.Add("Named pipe: Cosmos Debugger as server, QEmu as client", "-serial pipe_client:CosmosDebug");
            QEmuDebugComType.Add("TCP: Cosmos Debugger as client, QEmu as server on port 4444", "-serial tcp:127.0.0.1:4444,server");
            QEmuDebugComType.Add("TCP: Cosmos Debugger as server on port 4444, QEmu as client", "-serial tcp:127.0.0.1:4444");
            //DebugComType.Add("UDP: Cosmos Debugger as server on port 4444, QEmu as client", "-serial null");
            //DebugComType.Add("UDP: Cosmos Debugger as client on port 4444, QEmu as server", "-serial null");
            //DebugComType.Add("Telnet: Cosmos Debugger as server on port 4444, QEmu as client", "-serial null");
            //DebugComType.Add("Telnet: Cosmos Debugger as client, QEmu as server on port 4444", "-serial null");
            //DebugComType.Add("COM: Cosmos Debugger as server, QEmu as client", "-serial null");
            //DebugComType.Add("COM: Cosmos Debugger as client, QEmu as server", "-serial null");

            CompileIL = true;
            Target = "QEMU";
            ShowOptions = true;
        }

        protected static TEnum ReadEnum<TEnum>(XmlDocument aDoc, string aName, TEnum aDefault) {

            string xValue = ReadValue(aDoc,
                                      aName,
                                      null);
            if (xValue != null) {
                return (TEnum)Enum.Parse(typeof(TEnum), xValue, true);
            } else {
                return aDefault;
            }
        }

        protected static string ReadValue(XmlDocument aDoc, string aName, string aDefault) {
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

        public static void Load() {
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
                                    "Realtek RTL8139");
            AudioCard = ReadValue(xDoc,
                                  "AudioCard",
                                  "PC Speaker");
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
            DebugComMode = ReadValue(xDoc,"DebugComMode","TCP: Cosmos Debugger as server on port 4444, QEmu as client");
        }

        public static void Save() {
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
                xWriteValue("DebugComMode", DebugComMode.ToString());
                xWriter.WriteEndDocument();
                xWriter.Flush();
            }
        }
        
        public static TraceAssemblies TraceAssemblies { get; set; }
        public static string Target { get; set; }
        public static string DebugPort { get; set; }
        public static DebugMode DebugMode { get; set; }
        public static string DebugComMode { get; set; }
        public static bool UseGDB { get; set; }
        public static bool CreateHDImage { get; set; }
        public static bool UseNetworkTAP { get; set; }
        public static string NetworkCard { get; set; }
        public static string AudioCard { get; set; }
        public static string VMWareEdition { get; set; }
        public static string USBDevice { get; set; }
        public static bool ShowOptions { get; set; }
        public static bool CompileIL { get; set; }
        public static string BuildPath {
            get;
            set;
        }
    }
}

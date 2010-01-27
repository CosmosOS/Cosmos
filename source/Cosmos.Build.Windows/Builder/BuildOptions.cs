using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using System.Collections;
using Cosmos.IL2CPU;
using Cosmos.Build.Common;

namespace Cosmos.Compiler.Builder
{
    public enum dotNETFrameworkImplementationEnum
    {
        Microsoft = 0,
        ProjectMono
    }

    public class BuildOptions
    {
        public readonly  Hashtable QEmuAudioCard;
        public readonly  Hashtable QEmuNetworkCard;
        public readonly  Hashtable QEmuDebugComType;
        //public Hashtable VmWareDebugComType = new Hashtable();
        protected const string REG_KEY = @"Software\Cosmos\User Kit";

         public BuildOptions() //TODO make private at the moment options is storing config data as well as config options. 
        {
            QEmuAudioCard = new Hashtable();
            QEmuNetworkCard = new Hashtable();
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

        protected  TEnum ReadEnum<TEnum>(XmlDocument aDoc, string aName, TEnum aDefault)
        {

            string xValue = ReadValue(aDoc,
                                      aName,
                                      null);
            if (xValue != null)
            {
                return (TEnum)Enum.Parse(typeof(TEnum), xValue, true);
            }
            else
            {
                return aDefault;
            }
        }

        protected  string ReadValue(XmlDocument aDoc, string aName, string aDefault)
        {
            var xElem = aDoc.SelectSingleNode("/settings/" + aName);
            if (xElem != null)
            {
                return xElem.InnerText.Trim();
            }
            return aDefault;
        }

        protected  string ConfigDirName
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    return ".cosmos";
                }
                else
                {
                    return "Cosmos";
                }
            }
        }

        //called by test runner
        public static BuildOptions Load()
        {
            BuildOptions options = new BuildOptions();
            options.LoadData();
            return options;
        }
        /// <summary>
        /// Retrieves settings from %AppData%\Cosmos\BuilderConfig.xml
        /// </summary>
        void LoadData()
        {
            var xDoc = new XmlDocument();
            var xFileName = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                      ConfigDirName),
                                         "BuilderConfig.xml");
            if (!File.Exists(xFileName))
            {
                return;
            }
            xDoc.Load(xFileName);
            BuildPath = ReadValue(xDoc, "BuildPath", "");
            TraceAssemblies = ReadEnum(xDoc,
                                       "DebugTraceAssemblies",
                                       TraceAssemblies.Cosmos);
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
            dotNETFrameworkImplementation = ReadEnum(xDoc,
                                                     "dotNETFrameworkImplementation",
                                                     dotNETFrameworkImplementationEnum.Microsoft);
            DebugComMode = ReadValue(xDoc, "DebugComMode", "TCP: Cosmos Debugger as server on port 4444, QEmu as client");
        }

        public  void Save()
        {
            var xFileName = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                     ConfigDirName),
                                        "BuilderConfig.xml");
            if (File.Exists(xFileName))
            {
                File.Delete(xFileName);
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(xFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(xFileName));
                }
            }
            using (var xWriter = XmlWriter.Create(xFileName))
            {
                xWriter.WriteStartDocument();
                xWriter.WriteStartElement("settings");
                Action<string, string> xWriteValue = delegate(string aKey,
                                                              string aValue)
                {
                    xWriter.WriteStartElement(aKey);
                    xWriter.WriteValue(aValue ?? "");
                    xWriter.WriteEndElement();
                };
                xWriteValue("BuildPath", BuildPath);
                xWriteValue("DebugTraceAssemblies", TraceAssemblies.ToString());
                xWriteValue("Target", Target);
                xWriteValue("DebugPort", DebugPort);
                xWriteValue("DebugMode", DebugMode.ToString());
                xWriteValue("UseGDB", UseGDB.ToString());
                xWriteValue("UseHDImage", CreateHDImage.ToString());
                xWriteValue("UseTAP", UseNetworkTAP.ToString());
                xWriteValue("NetworkCard", NetworkCard);
                xWriteValue("AudioCard", AudioCard);
                xWriteValue("VMWareEdition", VMWareEdition);
                xWriteValue("USBDevice", USBDevice);
                xWriteValue("ShowOptions", ShowOptions.ToString());
                xWriteValue("CompileIL", CompileIL.ToString());
                xWriteValue("dotNETFrameworkImplementation", dotNETFrameworkImplementation.ToString());
                xWriteValue("DebugComMode", DebugComMode);
                xWriter.WriteEndDocument();
                xWriter.Flush();
            }
        }

        public  TraceAssemblies TraceAssemblies { get; set; }
        public  string Target { get; set; } //TODO make enumeration
        public  string DebugPort { get; set; }
        public byte DebugPortId { get; set; }
        public  DebugMode DebugMode { get; set; }
        public  string DebugComMode { get; set; }
        public  bool UseGDB { get; set; }
        public  bool CreateHDImage { get; set; }
        public  bool UseNetworkTAP { get; set; }
        public  string NetworkCard { get; set; }
        public  string AudioCard { get; set; }
        public  string VMWareEdition { get; set; }
        public  string USBDevice { get; set; }
        public  bool ShowOptions { get; set; }
        public  bool CompileIL { get; set; }
        public bool UseInternalAssembler { get; set; }
        public  string BuildPath
        {
            get;
            set;
        }

        public  dotNETFrameworkImplementationEnum dotNETFrameworkImplementation
        {
            get;
            set;
        }
    }
}

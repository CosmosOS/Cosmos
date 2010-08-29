using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public class Settings {
        static protected string mFilename = "";
        static public string Filename {
            get { return mFilename; }
        }

        static protected bool mAutoConnect = false;
        static public bool AutoConnect {
            get { return mAutoConnect; }
            set { mAutoConnect = value; }
        }

        static protected string mOutputPath;
        static public string OutputPath {
            get { return mOutputPath; }
        }

        static protected string mObjFile;
        static public string ObjFile {
            get { return mObjFile; }
        }

        static protected string mAsmFile;
        static public string AsmFile {
            get { return mAsmFile; }
        }

        static public SettingsDS DS = new SettingsDS();

        static public void Save() {
            Windows.SavePositions();
            Windows.mBreakpointsForm.SaveSettings();
            // Its often checked into TFS, so if its readonly, dont save it.
            if ((File.GetAttributes(Filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                MessageBox.Show("File is read only. Cannot save.");
            } else {
                DS.WriteXml(Filename, System.Data.XmlWriteMode.IgnoreSchema);
            }
        }

        static public void Load(string aFilename) {
            mFilename = aFilename;
            if (File.Exists(Filename)) {
                //TODO: Change this and other general settings to read from the General datatable
                mOutputPath = Path.Combine(Path.GetDirectoryName(Filename), @"bin\debug\");
                mObjFile = Path.GetFileNameWithoutExtension(Filename) + ".obj";
                mAsmFile = Path.GetFileNameWithoutExtension(Filename) + ".asm";
                
                DS.ReadXml(Filename, System.Data.XmlReadMode.IgnoreSchema);
            }
        }

        static public void InitWindows() {
            Windows.RestorePositions();
            Windows.mBreakpointsForm.LoadSession();
        }
    }

}

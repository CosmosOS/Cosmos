using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public class Settings {
        static protected string mFilenameOfBreakPointXml = "";
        static public string Filename {
            get { return mFilenameOfBreakPointXml; }
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
            if (File.Exists(Filename) && (File.GetAttributes(Filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                MessageBox.Show("File is read only. Cannot save.");
            } else {
                DS.WriteXml(Filename, System.Data.XmlWriteMode.IgnoreSchema);
            }
        }

        static public bool Load(string aFilenameOfBreakPointXml) {
            mFilenameOfBreakPointXml = aFilenameOfBreakPointXml;
			try {
				//TODO: Change this and other general settings to read from the General datatable
				mOutputPath = Path.Combine(Path.GetDirectoryName(Filename), @"bin\debug\");
				mObjFile = Path.GetFileNameWithoutExtension(Filename) + ".obj";
				mAsmFile = Path.GetFileNameWithoutExtension(Filename) + ".asm";
				if (File.Exists(Filename))
					DS.ReadXml(Filename, System.Data.XmlReadMode.IgnoreSchema);
				return true;
			}
			catch (Exception e) {
				mOutputPath = null;
				MessageBox.Show(string.Format(
						"Exception on loading of settings file \"{0}\" :\n{1}\n\nStacktrace:\n{2}",
						Filename, e.Message, e.StackTrace),
					"Breakpoint Settings", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
			return false;
        }

		static public bool LoadOnFly(string aFilenameOfBreakPointXml)
		{
			mFilenameOfBreakPointXml = aFilenameOfBreakPointXml;
			try
			{
				//TODO: Change this and other general settings to read from the General datatable
				mOutputPath = Path.GetDirectoryName(Filename);
				mObjFile = Path.GetFileNameWithoutExtension(Filename) + ".obj";
				mAsmFile = Path.GetFileNameWithoutExtension(Filename) + ".asm";
				// for save function needed
				if (mOutputPath.ToLower().EndsWith("\\bin\\debug"))
				{
					// create path for cgdb like VS it does
					int len = "\\bin\\debug".Length;
					mFilenameOfBreakPointXml = Path.Combine(mOutputPath.Substring(0, mOutputPath.Length - len), Path.GetFileNameWithoutExtension(Filename) + ".cgdb");
				}
				else
					mFilenameOfBreakPointXml = Path.Combine(mOutputPath, Path.GetFileNameWithoutExtension(Filename) + ".cgdb");

				if (false == File.Exists(Path.Combine(mOutputPath, mObjFile))
					|| false == File.Exists(Path.Combine(mOutputPath, mAsmFile))
					|| false == File.Exists(Path.Combine(mOutputPath, Path.GetFileNameWithoutExtension(Filename) + ".mdf")))
				{
					mOutputPath = null;
					return false;
				}
				return true;
			}
			catch (Exception)
			{
				mOutputPath = null;
			}
			return false;
		}

        static public void InitWindows() {
            Windows.RestorePositions();
            Windows.mBreakpointsForm.LoadSession();
        }
    }
}
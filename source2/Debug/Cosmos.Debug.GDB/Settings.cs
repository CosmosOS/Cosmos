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
            set { mFilename = value; }
        }

        static protected bool mAutoConnect = false;
        static public bool AutoConnect {
            get { return mAutoConnect; }
            set { mAutoConnect = value; }
        }

        static public SettingsDS DS = new SettingsDS();

        static public void Save() {
            Windows.SavePositions();
            // Its often checked into TFS, so if its ro, just dont save it.
            if ((File.GetAttributes(Filename) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) {
                DS.WriteXml(Filename, System.Data.XmlWriteMode.IgnoreSchema);
            }
        }

        static public void Load() {
            if (File.Exists(Filename)) {
                DS.ReadXml(Filename, System.Data.XmlReadMode.IgnoreSchema);

                Windows.RestorePositions();
                Windows.mBreakpointsForm.LoadSession();
            }
        }
    }

}

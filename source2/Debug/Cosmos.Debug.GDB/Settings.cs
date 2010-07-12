using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public class Settings {
        //TODO: Not supposed to be in app dir, but unless we release this as a standalone project
        //it doesnt matter. If we do that we have to create project types anyways.
        static protected string ConfigPathname = Application.ExecutablePath + ".Settings";

        static public SettingsDS DS = new SettingsDS();

        static public void Save() {
            Windows.SavePositions();
            DS.WriteXml(ConfigPathname, System.Data.XmlWriteMode.IgnoreSchema);
        }

        static public void Load() {
            if (File.Exists(ConfigPathname)) {
                DS.ReadXml(ConfigPathname, System.Data.XmlReadMode.IgnoreSchema);
            }
        }
    }

}

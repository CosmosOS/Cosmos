using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace PlugsInspector
{
    public class AssembliesPreloader
    {
        public static void LoadAllAssemblies()
        {
            AllFilesInApplicationFolder().ForEach(fi => Assembly.LoadFrom(fi.FullName)); 
        }

        public static List<FileInfo> AllDllFilesIn(string path, bool recursively = false)
        {
            return new DirectoryInfo(path)
                      .EnumerateFiles("*.dll", recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
        }
        public static List<FileInfo> AllFilesInApplicationFolder()
        {
            return AllDllFilesIn(Path.GetDirectoryName(Application.ExecutablePath), true);
        }
    }  
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using Cosmos.Build.Windows.Config.MSCorCfg;

namespace Cosmos.Build.Windows.Config.Tasks
{
    class CleanGacTask : Task
    {
        private ArrayList arr = new ArrayList();

        #region ITask Members

        public override void Execute()
        {
            string path = Tools.Dir("GAC");

            Fusion.ReadCache(arr, (uint)Fusion.CacheType.GAC);

            string[] dlls = Directory.GetFiles(path, "*.dll");
            int i = 0;

            foreach (string dll in dlls)
            {
                float p = 100 * (i++) / dlls.Length;
                OnStatus(p, dll);

                Assembly a = Assembly.LoadFile(dll);
                AssemblyName an = new AssemblyName(a.FullName);
                RemoveMatching(arr, an.Name, Convert.ToBase64String(an.GetPublicKeyToken()));
            }
        }

        private void RemoveMatching(ArrayList arr, string name, string sn)
        {
            ArrayList toremove = new ArrayList();

            foreach (object info in arr)
            {
                AssemInfo asi = new AssemInfo(info);
                if (asi.Name == name)
                    toremove.Add(info);
            }

            foreach (object info2 in toremove)
            {
                AssemInfo asi = new AssemInfo(info2);
                arr.Remove(info2);
                Fusion.GacUninstall(asi.Name);
            }
        }

        #endregion

        public override string Name
        {
            get { return "Clean GAC"; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Windows.Config.MSCorCfg;
using System.IO;
using System.Collections;

namespace Cosmos.Build.Windows.Config.Tasks
{
    class InstallGacTask : Task
    {
        private ArrayList arr = new ArrayList();

        public override string Name
        {
            get { return "Install To GAC"; }
        }

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
                Fusion.GacInstall(dll);
            }
        }
    }
}

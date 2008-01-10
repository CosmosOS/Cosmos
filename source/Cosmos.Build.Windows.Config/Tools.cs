using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Build.Windows.Config
{
    public static class Tools
    {
        public static string Path { get; private set; }

        static Tools()
        {
            Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Path = Dir("..");
        }

        public static string Dir(params string[] path)
        {
            string res = Path;
            foreach (string p in path)
                res = System.IO.Path.Combine(res, p);
            return res;
        }
    }
}

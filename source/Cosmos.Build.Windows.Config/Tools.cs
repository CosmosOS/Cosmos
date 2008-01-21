using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Build.Windows.Config
{
    public static class Tools
    {
        public static string CosmosPath { get; private set; }

        static Tools()
        {
            CosmosPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string CosmosDir(params string[] path)
        {
            string res = CosmosPath;
            foreach (string p in path)
                res = System.IO.Path.Combine(res, p);
            return res;
        }

		public static string VSPath {
			get;
			set;
		}

		public static string VCSPath {
			get;
			set;
		}

		public static string VSTemplatePath {
			get;
			set;
		}

		public static string VCSTemplatePath {
			get;
			set;
		}
    }
}

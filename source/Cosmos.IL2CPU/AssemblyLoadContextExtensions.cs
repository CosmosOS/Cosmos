using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Cosmos.IL2CPU {
    public static class AssemblyLoadContextExtensions {
        private static Dictionary<string, Assembly> mLoadedAssemblies = new Dictionary<string, Assembly>();

        public static List<Assembly> GetLoadedAssemblies(this AssemblyLoadContext aContext) {
            return mLoadedAssemblies.Select(xAssembly => xAssembly.Value).ToList();
        }

        public static Assembly LoadFromAssemblyCacheOrPath(this AssemblyLoadContext aContext, string aFullPath) {
            string xAssemblyShortName = Path.GetFileNameWithoutExtension(aFullPath);
            if (mLoadedAssemblies.ContainsKey(xAssemblyShortName)) {
                return mLoadedAssemblies[xAssemblyShortName];
            }

            Assembly xAssembly = null;
            var xAssemblyName = AssemblyLoadContext.GetAssemblyName(aFullPath);
            if (xAssemblyName != null) {
                xAssembly = aContext.LoadFromAssemblyName(xAssemblyName);
            }
            if (xAssembly == null) {
                xAssembly = aContext.LoadFromAssemblyPath(aFullPath);
            }
            mLoadedAssemblies.Add(xAssemblyShortName, xAssembly);
            return xAssembly;
        }
    }
}

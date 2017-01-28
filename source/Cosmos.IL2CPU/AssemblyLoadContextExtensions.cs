using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Cosmos.IL2CPU
{
    public static class AssemblyLoadContextExtensions
    {
        private static Dictionary<string, Assembly> mLoadedAssemblies = new Dictionary<string, Assembly>();

        public static List<Assembly> GetLoadedAssemblies(this AssemblyLoadContext aContext)
        {
            return mLoadedAssemblies.Select(xAssembly => xAssembly.Value).ToList();
        }

        public static Assembly LoadFromAssemblyCacheOrPath(this AssemblyLoadContext aContext, string aFullPath)
        {
            string xAssemblyName = Path.GetFileNameWithoutExtension(aFullPath);
            if (mLoadedAssemblies.ContainsKey(xAssemblyName))
            {
                return mLoadedAssemblies[xAssemblyName];
            }

            var xAssembly = aContext.LoadFromAssemblyPath(aFullPath);
            mLoadedAssemblies.Add(xAssemblyName, xAssembly);
            return xAssembly;
        }
    }
}

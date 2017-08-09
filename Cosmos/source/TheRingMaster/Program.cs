using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Cosmos.Build.Common;

namespace TheRingMaster
{
    public class Program
    {
        static Dictionary<Assembly, string> RingCache = new Dictionary<Assembly, string>();
        static string KernelDir;

        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("ARGS:");

                foreach (var xArg in args)
                {
                    Console.WriteLine(xArg);
                }

                Console.WriteLine("Usage: theringmaster <path-to-kernel>");
                return;
            }

            var xKernelAssemblyPath = args[0];

            if (!File.Exists(xKernelAssemblyPath))
            {
                throw new FileNotFoundException("Kernel Assembly not found! Path: '" + xKernelAssemblyPath + "'");
            }

            KernelDir = Path.GetDirectoryName(xKernelAssemblyPath);
            AssemblyLoadContext.Default.Resolving += Default_Resolving;

            var xKernelAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(xKernelAssemblyPath);
            CheckRings(xKernelAssembly, "Application");

            void CheckRings(Assembly aAssembly, string aRing, string aSourceAssemblyName = null)
            {
                RingCache.TryGetValue(aAssembly, out var xRing);

                if (xRing == null)
                {
                    var xManifestName = aAssembly.GetManifestResourceNames()
                                                 .Where(n => n == aAssembly.GetName().Name + ".Cosmos.cfg")
                                                 .SingleOrDefault();

                    if (xManifestName != null)
                    {
                        Dictionary<string, string> xCfg;

                        using (var xManifestStream = aAssembly.GetManifestResourceStream(xManifestName))
                        {
                            xCfg = ParseCfg(xManifestStream);
                        }

                        if (xCfg == null)
                        {
                            throw new Exception("Invalid Cosmos configuration! Resource name: " + xManifestName);
                        }

                        xCfg.TryGetValue("Ring", out xRing);

                        if (!new string[] { "CPU", "Platform", "HAL", "System", "Application", /*"Plug",*/ "Debug" }.Contains(xRing))
                        {
                            throw new Exception("Unknown ring! Ring: " + xRing);
                        }
                    }

                    if (xRing == null)
                    {
                        xRing = "External";
                    }
                }

                RingCache.Add(aAssembly, xRing);

                // Check Rings

                bool xValid = false;

                // Same rings
                // OR
                // External ring, can be referenced by any ring
                // OR
                // One of the assemblies is Debug
                if (aRing == xRing || xRing == "External" || aRing == "Debug" || xRing == "Debug")
                {
                    xValid = true;
                }

                if (!xValid)
                {
                    switch (aRing)
                    {
                        case "Application" when xRing == "System":
                        case "System" when xRing == "HAL":
                        case "Platform" when xRing == "CPU":
                        case "Platform" when xRing == "HAL":
                        //case "Plug" when xRing == "System":
                            return;
                    }
                }

                if (!xValid)
                {
                    var xExceptionMessage = "Invalid rings! Source assembly: " + (aSourceAssemblyName ?? "(no assembly)") +
                                            ": Ring " + aRing + "; Referenced assembly: " + aAssembly.GetName().Name +
                                            ": Ring " + xRing;

                    throw new Exception(xExceptionMessage);
                }

                foreach (var xReference in aAssembly.GetReferencedAssemblies())
                {
                    try
                    {
                        CheckRings(AssemblyLoadContext.Default.LoadFromAssemblyName(xReference), xRing, aAssembly.GetName().Name);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
            }
        }

        private static Assembly Default_Resolving(AssemblyLoadContext aContext, AssemblyName aAssemblyName)
        {
            Assembly xAssembly = null;

            if (ResolveAssemblyForDir(KernelDir, out xAssembly))
            {
                return xAssembly;
            }

            if (ResolveAssemblyForDir(CosmosPaths.Kernel, out xAssembly))
            {
                return xAssembly;
            }

            return xAssembly;

            bool ResolveAssemblyForDir(string aDir, out Assembly aAssembly)
            {
                aAssembly = null;

                var xFiles = Directory.GetFiles(aDir, aAssemblyName.Name + ".*", SearchOption.TopDirectoryOnly);
                
                if (xFiles.Any(f => Path.GetExtension(f) == ".dll"))
                {
                    aAssembly = aContext.LoadFromAssemblyPath(xFiles.Where(f => Path.GetExtension(f) == ".dll").Single());
                }

                if (xFiles.Any(f => Path.GetExtension(f) == ".exe"))
                {
                    aAssembly = aContext.LoadFromAssemblyPath(xFiles.Where(f => Path.GetExtension(f) == ".exe").Single());
                }

                return aAssembly != null;
            }
        }

        static Dictionary<string, string> ParseCfg(Stream aStream)
        {
            var xCfg = new Dictionary<string, string>();

            using (var xReader = new StreamReader(aStream))
            {
                while (xReader.Peek() >= 0)
                {
                    var xLine = xReader.ReadLine();

                    if (!xLine.Contains(':') || xLine.Count(c => c == ':') > 1)
                    {
                        return null;
                    }

                    var xProperty = xLine.Split(':');
                    xCfg.Add(xProperty[0].Trim(), xProperty[1].Trim());
                }
            }

            return xCfg;
        }
    }
}

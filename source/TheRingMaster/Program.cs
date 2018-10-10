using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using IL2CPU.API.Attribs;

namespace TheRingMaster
{
    public class Program
    {
        enum Ring
        {
            External = 0,
            CPU = 10,
            Platform = 20,
            HAL = 30,
            System = 40,
            Application = 50,
            Plug = 91,
            CpuPlug = 92,
            Debug
        }

        static Dictionary<Assembly, Ring> RingCache = new Dictionary<Assembly, Ring>();
        static string KernelDir;

        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
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
            CheckRings(xKernelAssembly, Ring.Application);

            void CheckRings(Assembly aAssembly, Ring aRing, string aSourceAssemblyName = null)
            {
                bool xDebugAllowed = false;

                if (!RingCache.TryGetValue(aAssembly, out var xRing))
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

                        xCfg.TryGetValue("Ring", out var xRingName);

                        if (!Enum.TryParse(xRingName, true, out xRing))
                        {
                            throw new Exception("Unknown ring! Ring: " + xRingName);
                        }

                        xCfg.TryGetValue("DebugRing", out var xDebugRing);
                        xDebugAllowed = xDebugRing.ToLower() == "allowed";
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
                // OR
                // Debug ring allowed
                if (aRing == xRing || xRing == Ring.External || aRing == Ring.Debug || xRing == Ring.Debug || xDebugAllowed)
                {
                    xValid = true;
                }

                if (!xValid)
                {
                    switch (aRing)
                    {
                        case Ring.Application when xRing == Ring.System:
                        case Ring.System when xRing == Ring.HAL:
                        case Ring.Platform when xRing == Ring.CPU:
                        case Ring.Platform when xRing == Ring.HAL:
                            return;
                    }
                }

                if (!xValid)
                {
                    var xExceptionMessage = "Invalid rings! Source assembly: " + (aSourceAssemblyName ?? "(no assembly)") +
                                            ", Ring: " + aRing + "; Referenced assembly: " + aAssembly.GetName().Name +
                                            ", Ring: " + xRing;

                    throw new Exception(xExceptionMessage);
                }

                if (xRing != Ring.CPU && xRing != Ring.CpuPlug)
                {
                    foreach (var xModule in aAssembly.Modules)
                    {
                        // TODO: Check unsafe code
                    }
                }

                foreach (var xType in aAssembly.GetTypes())
                {
                    if (xRing != Ring.Plug)
                    {
                        if (xType.GetCustomAttribute<Plug>() != null)
                        {
                            throw new Exception("Plugs are only allowed in the Plugs ring! Assembly: " + aAssembly.GetName().Name);
                        }
                    }
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

            //if (ResolveAssemblyForDir(CosmosPaths.Kernel, out xAssembly))
            //{
            //    return xAssembly;
            //}

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

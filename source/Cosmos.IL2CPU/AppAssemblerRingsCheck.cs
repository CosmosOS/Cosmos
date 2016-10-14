using System;
using System.Linq;
using System.Reflection;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU
{
    public static class AppAssemblerRingsCheck
    {
        private static bool IsAssemblySkippedDuringRingCheck(Assembly assembly)
        {
            string xName = assembly.GetName().Name;

            if (assembly.GlobalAssemblyCache ||
                (xName == "Cosmos.Debug.Kernel") ||
                (xName == "Cosmos.Debug.Kernel.Plugs") ||
                (xName == "Cosmos.IL2CPU") ||
                (xName == "Cosmos.Common") ||
                (xName == "Cosmos.TestRunner.TestController"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method validates rings. Assemblies are specific for a given ring and are able to be dependent on assemblies in
        /// the same ring or one ring "up" (ie, User can reference System, etc), but not other way around.
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="entryAssembly"></param>
        public static void Execute(ILScanner scanner, Assembly entryAssembly)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            RingsWriteLine("Start check");

            // verify the entry assembly is in the User ring.
            var xRing = GetRingFromAssembly(entryAssembly);
            if (xRing != Ring.User)
            {
                throw new Exception($"Assembly '{entryAssembly.GetName().Name}' contains your kernel class, which means it should be in the ring {Ring.User}!");
            }

            foreach (var xAssembly in scanner.mUsedAssemblies)
            {
                if (IsAssemblySkippedDuringRingCheck(xAssembly))
                {
                    continue;
                }

                RingsWriteLine("Assembly '{0}'", xAssembly.GetName().Name);
                xRing = GetRingFromAssembly(xAssembly);
                var xRingInt = (int)xRing;

                RingsWriteLine("\t\tRing = {0}", xRing);
                foreach (var xAsmDepRef in xAssembly.GetReferencedAssemblies())
                {
                    var xAsmDep = scanner.mUsedAssemblies.FirstOrDefault(i => i.GetName().Name == xAsmDepRef.Name);
                    if (xAsmDep == null || IsAssemblySkippedDuringRingCheck(xAsmDep))
                    {
                        continue;
                    }
                    RingsWriteLine("\tDependency '{0}'", xAsmDepRef.Name);
                    var xDepRing = GetRingFromAssembly(xAsmDep);
                    RingsWriteLine("\t\tRing = {0}", xDepRing);

                    var xDepRingInt = (int)xDepRing;

                    if (xDepRingInt == xRingInt)
                    {
                        // assembly and its dependency are in the same ring.
                        continue;
                    }
                    if (xDepRingInt > xRingInt)
                    {
                        throw new Exception($"Assembly '{xAssembly.GetName().Name}' is in ring {xRing}({xRingInt}). It references assembly '{xAsmDepRef.Name}' which is in ring {xDepRing}({xDepRingInt}), but this is not allowed!");
                    }

                    var xRingDiff = xRingInt - xDepRingInt;
                    if (xRingDiff == 1)
                    {
                        // 1 level up is allowed
                        continue;
                    }
                    throw new Exception($"Assembly '{xAssembly.GetName().Name}' is in ring {xRing}({xRingInt}). It references assembly '{xAsmDepRef.Name}' which is in ring {xDepRing}({xDepRingInt}), but this is not allowed!");
                }

                // now do per-ring checks:
                switch (xRing)
                {
                    case Ring.User:
                        ValidateUserAssembly(xAssembly);
                        break;
                    case Ring.Core:
                        ValidateCoreAssembly(xAssembly);
                        break;
                    case Ring.HAL:
                        ValidateHALAssembly(xAssembly);
                        break;
                    case Ring.System:
                        ValidateSystemAssembly(xAssembly);
                        break;
                    default:
                        throw new NotImplementedException($"Ring {xRing} not implemented");
                }
            }
        }

        private static bool HasAssemblyPlugs(Assembly assembly)
        {
            foreach (var xTypes in assembly.GetTypes())
            {
                if (xTypes.IsSubclassOf(typeof(AssemblerMethod)))
                {
                    return true;
                }
            }
            return false;
        }

        private static void ValidateCoreAssembly(Assembly assembly)
        {
            // any checks to do?
        }

        private static void ValidateHALAssembly(Assembly assembly)
        {
            if (HasAssemblyPlugs(assembly))
            {
                throw new Exception($"HAL assembly '{assembly.GetName().Name}' uses Assembly plugs, which are not allowed!");
            }
        }

        private static void ValidateSystemAssembly(Assembly assembly)
        {
            if (HasAssemblyPlugs(assembly))
            {
                throw new Exception(String.Format("System assembly '{0}' uses Assembly plugs, which are not allowed!", assembly.GetName().Name));
            }
        }

        private static void ValidateUserAssembly(Assembly assembly)
        {
            if (HasAssemblyPlugs(assembly))
            {
                throw new Exception($"User assembly '{assembly.GetName().Name}' uses Assembly plugs, which are not allowed!");
            }
        }

        private static Ring GetRingFromAssembly(Assembly assembly)
        {
            var xRingAttrib = assembly.GetCustomAttributes<RingAttribute>().SingleOrDefault();
            if (xRingAttrib == null)
            {
                return Ring.User;
            }

            return xRingAttrib.Ring;
        }

        private static void RingsWriteLine(string line, params object[] args)
        {
            Console.WriteLine("Rings: " + String.Format(line.Replace("\t", "    "), args));
        }
    }
}

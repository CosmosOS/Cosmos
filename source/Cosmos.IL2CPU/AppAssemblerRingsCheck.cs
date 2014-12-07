using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU
{
    public static class AppAssemblerRingsCheck
    {
        private static bool IsAssemblySkippedDuringRingCheck(Assembly assembly)
        {
            if (assembly.GlobalAssemblyCache)
            {
                return true;
            }

            // Cosmos.Debug.Kernel.Debugger is a "all-rings" assembly
            if (assembly == typeof(Cosmos.Debug.Kernel.Debugger).Assembly)
            {
                return true;
            }

            // Cosmos.Debug.Kernel.Debugger.Plugs is a "all-rings" assembly
            if (assembly.GetName().Name == typeof(Cosmos.Debug.Kernel.Debugger).Assembly.GetName().Name + ".Plugs")
            {
                return true;
            }

            if (assembly == typeof(AppAssemblerRingsCheck).Assembly)
            {
                return true;
            }

            // Cosmos.Common is a all-rings assembly
            if (assembly == typeof(RingAttribute).Assembly)
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
        public static void Execute(ILScanner scanner)
        {
            RingsWriteLine("Start check");

            foreach (var xAssembly in scanner.mUsedAssemblies)
            {
                if (IsAssemblySkippedDuringRingCheck(xAssembly))
                {
                    continue;
                }

                RingsWriteLine("Assembly '{0}'", xAssembly.GetName().Name);
                var xRing = GetRingFromAssembly(xAssembly);
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
                        throw new Exception(string.Format("Assembly '{0}' is in ring {1}({2}). It references assembly '{3}' which is in ring {4}({5}), but this is not allowed!", xAssembly.GetName().Name, xRing, xRingInt, xAsmDepRef.Name, xDepRing, xDepRingInt));
                    }

                    var xRingDiff = xRingInt - xDepRingInt;
                    if (xRingDiff == 1)
                    {
                        // 1 level up is allowed
                        continue;
                    }
                    throw new Exception(string.Format("Assembly '{0}' is in ring {1}({2}). It references assembly '{3}' which is in ring {4}({5}), but this is not allowed!", xAssembly.GetName().Name, xRing, xRingInt, xAsmDepRef.Name, xDepRing, xDepRingInt));
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
                        throw new NotImplementedException(String.Format("Ring {0} not implemented", xRing));
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
                throw new Exception(String.Format("HAL assembly '{0}' uses Assembly plugs, which are not allowed!", assembly.GetName().Name));
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
                throw new Exception(String.Format("User assembly '{0}' uses Assembly plugs, which are not allowed!", assembly.GetName().Name));
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
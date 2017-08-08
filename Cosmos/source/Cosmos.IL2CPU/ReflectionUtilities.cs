using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.IL2CPU
{
    public static class ReflectionUtilities
    {
        public static Type GetType(string aAssembly,
                                   string aType)
        {
            if (String.IsNullOrEmpty(aAssembly) || aAssembly == typeof(ReflectionUtilities).GetTypeInfo().Assembly.GetName().Name || aAssembly == typeof(ReflectionUtilities).GetTypeInfo().Assembly.FullName)
            {
                aAssembly = typeof(ReflectionUtilities).GetTypeInfo().Assembly.FullName;
            }
            var xAssemblyDef = Assembly.Load(new AssemblyName(aAssembly));
            return GetType(xAssemblyDef,
                           aType);
        }

        public static Type GetType(Assembly aAssembly,
                                   string aType)
        {
            string xActualTypeName = aType;
            if (xActualTypeName.Contains("<") && xActualTypeName.Contains(">"))
            {
                xActualTypeName = xActualTypeName.Substring(0,
                                                            xActualTypeName.IndexOf("<"));
            }
            Type xResult = aAssembly.GetType(aType,
                                             false);
            if (xResult != null)
            {
                return xResult;
            }
            throw new Exception("Type '" + aType + "' not found in assembly '" + aAssembly + "'!");
        }
        public static MethodBase GetMethodBase(Type aType,
                                               string aMethod,
                                               params string[] aParamTypes)
        {
            foreach (MethodBase xMethod in aType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (xMethod.Name != aMethod)
                {
                    continue;
                }
                ParameterInfo[] xParams = xMethod.GetParameters();
                if (xParams.Length != aParamTypes.Length)
                {
                    continue;
                }
                bool errorFound = false;
                for (int i = 0; i < xParams.Length; i++)
                {
                    if (xParams[i].ParameterType.FullName != aParamTypes[i])
                    {
                        errorFound = true;
                        break;
                    }
                }
                if (!errorFound)
                {
                    return xMethod;
                }
            }
            foreach (MethodBase xMethod in aType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (xMethod.Name != aMethod)
                {
                    continue;
                }
                ParameterInfo[] xParams = xMethod.GetParameters();
                if (xParams.Length != aParamTypes.Length)
                {
                    continue;
                }
                bool errorFound = false;
                for (int i = 0; i < xParams.Length; i++)
                {
                    if (xParams[i].ParameterType.FullName != aParamTypes[i])
                    {
                        errorFound = true;
                        break;
                    }
                }
                if (!errorFound)
                {
                    return xMethod;
                }
            }
            throw new Exception($"Method '{aMethod}' not found on type '{aType.FullName}'!");
        }
    }
}

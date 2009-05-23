using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Indy.IL2CPU.IL;

namespace Indy.IL2CPU.Compiler
{
    public static class Scanner
    {
        public static IEnumerable<Assembly> GetAllAssemblies(Assembly aAssembly, List<Assembly> aIgnoreList)
        {
            var xResult = new List<Assembly>();
            xResult.Add(aAssembly);
            for (int i = 0;i < xResult.Count; i++)
            {
                var xCur = xResult[i];
                foreach (var xRef in xCur.GetReferencedAssemblies())
                {
                    var xAsm = Assembly.Load(xRef);
                    if (!xResult.Contains(xAsm) && !aIgnoreList.Contains(xAsm))
                    {
                        xResult.Add(xAsm);
                    }
                }
            }
            return xResult;
        }

        public static void GetAllGenericNames(Assembly aAssembly, bool aIsEntryAsm, IList<string> aGenericTypes, IList<string> aGenericMethods)
        {
            // for entry assemblies, skip the Main method, as it contains the builder stuff
            bool xShouldSkipMain = aIsEntryAsm;
            Action<Type> xCheckType = null;
            xCheckType = new Action<Type>(delegate(Type aType)
             {
                 if (aType.IsGenericType && !aType.IsGenericTypeDefinition)
                 {
                     if (!aGenericTypes.Contains(aType.AssemblyQualifiedName))
                     {
                         aGenericTypes.Add(aType.AssemblyQualifiedName);
                     }
                     foreach (var xArg in aType.GetGenericArguments())
                     {
                         xCheckType(xArg);
                     }
                 }
             });
            foreach (var xType in aAssembly.GetTypes())
            {
                if (xType.BaseType != null)
                {
                    xCheckType(xType.BaseType);
                }
                foreach (var xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (xShouldSkipMain && xMethod == aAssembly.EntryPoint)
                    {
                        continue;
                    }
                    xCheckType(xMethod.ReturnType);
                    foreach (var xParam in xMethod.GetParameters())
                    {
                        xCheckType(xParam.ParameterType);
                    }
                    try
                    {
                        if (xMethod.GetMethodBody() == null)
                        {
                            continue;
                        }
                    }
                    catch (System.Security.VerificationException VE)
                    {
                        // apparently, ms uses some scary code for the .net framework..
                        continue;
                    }
                    catch (Exception E)
                    {
                        throw;
                    }

                    var xReader = new ILReader(xMethod);
                    while (xReader.Read())
                    {
                        switch (xReader.OpCode)
                        {
                            case OpCodeEnum.Call:
                            case OpCodeEnum.Callvirt:
                            case OpCodeEnum.Newobj:
                                xCheckType(xReader.OperandValueMethod.DeclaringType);
                                if (xReader.OperandValueMethod.IsGenericMethod && !xReader.OperandValueMethod.IsGenericMethodDefinition)
                                {
                                    var xName = xReader.OperandValueMethod.GetFullName();
                                    if (!aGenericMethods.Contains(xName))
                                    {
                                        aGenericMethods.Add(xName);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
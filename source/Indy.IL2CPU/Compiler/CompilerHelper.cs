using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Diagnostics;
using Indy.IL2CPU.IL;

namespace Indy.IL2CPU.Compiler
{
    public class CompilerHelper
    {
        public class CacheState: IComparable<CacheState>, IEquatable<CacheState> {
            public List<string> UsedGenericMethods = new List<string>();
            public List<string> UsedGenericTypes = new List<string>();

            internal void Load(string aFile)
            {
                if (!File.Exists(aFile))
                {
                    return;
                }
                var xDoc = new XmlDocument();
                xDoc.Load(aFile);
                var xElemMethods = xDoc.SelectSingleNode("/CacheState/GenericMethods");
                foreach (XmlNode xMethodNode in xElemMethods.ChildNodes)
                {
                    UsedGenericMethods.Add(xMethodNode.InnerText);
                }
                var xElemTypes = xDoc.SelectSingleNode("/CacheState/GenericTypes");
                foreach (XmlNode xTypeNode in xElemTypes.ChildNodes)
                {
                    UsedGenericTypes.Add(xTypeNode.InnerText);
                }
                UsedGenericTypes.Sort();
                UsedGenericMethods.Sort();
            }

            internal void Save(string aFile)
            {
                using (var xOut = XmlWriter.Create(aFile))
                {
                    xOut.WriteStartDocument();
                    xOut.WriteStartElement("CacheState");
                    {
                        xOut.WriteStartElement("GenericMethods");
                        {
                            foreach (var xMethod in UsedGenericMethods)
                            {
                                xOut.WriteStartElement("Method");
                                {
                                    xOut.WriteCData(xMethod);
                                }
                                xOut.WriteEndElement();
                            }
                        } xOut.WriteEndElement();
                        xOut.WriteStartElement("GenericTypes");
                        {
                            foreach (var xType in UsedGenericTypes)
                            {
                                xOut.WriteStartElement("Type");
                                {
                                    xOut.WriteCData(xType);
                                }
                                xOut.WriteEndElement();
                            }
                        } xOut.WriteEndElement();
                    }
                    xOut.WriteEndElement();
                    xOut.WriteEndDocument();
                }
            }

            #region IComparable<CacheState> Members

            public int CompareTo(CacheState other)
            {
                var xResult = this.UsedGenericMethods.Count.CompareTo(other.UsedGenericMethods.Count);
                if (xResult != 0)
                {
                    return xResult;
                }
                xResult = this.UsedGenericTypes.Count.CompareTo(other.UsedGenericTypes.Count);
                if (xResult != 0)
                {
                    return xResult;
                }
                for (int i = 0; i < UsedGenericMethods.Count; i++)
                {
                    xResult = UsedGenericMethods[i].CompareTo(other.UsedGenericMethods[i]);
                    if (xResult != 0)
                    {
                        return xResult;
                    }
                }
                for (int i = 0; i < UsedGenericTypes.Count; i++)
                {
                    xResult = UsedGenericTypes[i].CompareTo(other.UsedGenericTypes[i]);
                    if (xResult != 0)
                    {
                        return xResult;
                    }
                }
                return 0;
            }

            #endregion

            #region IEquatable<CacheState> Members

            public bool Equals(CacheState other)
            {
                return CompareTo(other) == 0;
            }

            #endregion
        }
        public List<Assembly> SkipList
        {
            get;
            set;
        }

        public event Func<Assembly, string> GetCacheStateFile;
        public event Func<Assembly, string> GetChecksumFile;
        public event Func<Assembly, bool, Assembler.Assembler> GetAssembler;
        public event Action<Assembly, Assembler.Assembler> SaveAssembler;
        public event Func<OpCodeMap> GetOpCodeMap;
        public event Action<LogSeverityEnum, string> DebugLog;

        public List<string> Plugs = new List<string>();

        public CompilerHelper()
        {
            SkipList = new List<Assembly>();
        }

        private Assembly mEntryAssembly;
        private IEnumerable<Assembly> mAllAssemblies;

        public void CompileExe(Assembly aAssembly)
        {
            if (aAssembly == null)
            {
                throw new ArgumentNullException("aAssembly");
            }
            mEntryAssembly=aAssembly;
            mAllAssemblies = Scanner.GetAllAssemblies(aAssembly, SkipList);
            var xAssembliesToScanForGenerics = GetAssembliesToScanForGenerics(mAllAssemblies);
            LoadCacheStates(mAllAssemblies);

            bool xNeedsFullRecompile = false;
            // now scan for generics usage
            foreach (var xAsm in xAssembliesToScanForGenerics)
            {
                var xTest = this.ScanAssembly(xAsm);
                if(!xTest.Equals(mCacheStates[xAsm])){
                    mCacheStates[xAsm] = xTest;
                    xNeedsFullRecompile =true;
                }
            }
            if (xNeedsFullRecompile)
            {
                foreach(var xAsm in mAllAssemblies)
                {
                 if(!mGenericTypeInstancesToGenerate.ContainsKey(xAsm))
                 {
                     ScanAssembly(xAsm);
                 }
                }
                DoFullRecompile();
            }
            else
            {
                SmartRecompile();
            }
        }

        private void CompileAssembly(Assembly aAssembly)
        {
            if(aAssembly==null)
            {
                throw new ArgumentNullException("aAssembly");
            }
            Console.Write(new String('-', Console.BufferWidth));
            Console.WriteLine("Starting compilation of assembly: " + aAssembly.FullName);
            var xCompiler = new AssemblyCompiler();
            xCompiler.DebugLog = delegate(LogSeverityEnum aSeverity, string aMessage) { this.DebugLog(aSeverity, aMessage); };
            xCompiler.IsEntrypointAssembly = aAssembly == mEntryAssembly;
            using (var xAssembler = GetAssembler(aAssembly, xCompiler.IsEntrypointAssembly))
            {
                xCompiler.Assembler = xAssembler;
                xCompiler.Assembly = aAssembly;
                if (xCompiler.IsEntrypointAssembly)
                {
                    foreach (var xRef in mAllAssemblies)
                    {
                        xCompiler.AssemblyReferences.Add(xRef.FullName);
                    }
                }
                xCompiler.OpCodeMap = GetOpCodeMap();
                xCompiler.Plugs.AddRange(Plugs);
                if(!mGenericTypeInstancesToGenerate.ContainsKey(aAssembly))
                {
                    Console.Write("");
                }
                xCompiler.Types.AddRange(mGenericTypeInstancesToGenerate[aAssembly]);
                xCompiler.Methods.AddRange(mGenericMethodInstancesToGenerate[aAssembly]);
                try
                {
                    xCompiler.Execute();

                    // update cache state files
                    mCacheStates[aAssembly].Save(GetCacheStateFile(aAssembly));
                    var xLastTime = File.GetLastWriteTimeUtc(aAssembly.Location).ToBinary();
                    var xChecksumFileName = GetChecksumFile(aAssembly);
                    File.WriteAllText(xChecksumFileName, xLastTime.ToString());
                }
                finally
                {
                    // todo: at the end, we dont want this, but for now to do debugging, leave it in finally
                    SaveAssembler(aAssembly, xCompiler.Assembler);
                }
            }
            GC.Collect();
        }





        //HACK fix up Generics 
        private List<RuntimeMethodHandle> Convert(IEnumerable<MethodBase> source)
        {
            List<RuntimeMethodHandle> result = new List<RuntimeMethodHandle>();

            foreach (var method in source)
                result.Add(method.MethodHandle);

            return result; 
        }


        private void DoFullRecompile()
        {
            int xTest = 0;
            foreach (var xAsm in mAllAssemblies)
            {
                CompileAssembly(xAsm);
                xTest++;
                GC.Collect();
                //if (xTest == 3)
                //{
                //    return;
                //}
            }
        }

        private void SmartRecompile()
        {
            foreach (var xAsm in GetAssembliesToScanForGenerics(mAllAssemblies))
            {
                CompileAssembly(xAsm);
            }
        }

       

        private void LoadCacheStates(IEnumerable<Assembly> xAllAssemblies)
        {
            mCacheStates.Clear();
            foreach (var xAsm in xAllAssemblies)
            {
                var xState = new CacheState();
                xState.Load(GetCacheStateFile(xAsm));
                mCacheStates.Add(xAsm, xState);
            }
        }

        private Dictionary<Assembly, CacheState> mCacheStates = new Dictionary<Assembly, CacheState>();
        private Dictionary<Assembly, List<Type>> mGenericTypeInstancesToGenerate = new Dictionary<Assembly, List<Type>>();
        private Dictionary<Assembly, List<MethodBase>> mGenericMethodInstancesToGenerate = new Dictionary<Assembly, List<MethodBase>>();
        
        private CacheState ScanAssembly(Assembly aAsm)
        {
            var xResult = new CacheState();
            bool xShouldSkipMain = aAsm == mEntryAssembly;
            if (!mGenericTypeInstancesToGenerate.ContainsKey(aAsm))
            {
                mGenericTypeInstancesToGenerate.Add(aAsm, new List<Type>());
            }
            var xDeclaringType_TypeInstances = mGenericTypeInstancesToGenerate[aAsm];
            if (!mGenericMethodInstancesToGenerate.ContainsKey(aAsm))
            {
                mGenericMethodInstancesToGenerate.Add(aAsm, new List<MethodBase>());
            }
            var xDeclaringType_MethodInstances = mGenericMethodInstancesToGenerate[aAsm];

            Action<Type> xCheckType = null;
            xCheckType = new Action<Type>(delegate(Type aType)
            {
                if (aType.IsGenericType && !aType.IsGenericTypeDefinition)
                {
                    // add to the list of the current assembly
                    if (!xResult.UsedGenericTypes.Contains(aType.AssemblyQualifiedName))
                    {
                        xResult.UsedGenericTypes.Add(aType.AssemblyQualifiedName);
                    }
                    // add to the list of the declaring assembly, so it can generate it later on
                    if (!xDeclaringType_TypeInstances.Contains(aType))
                    {
                        xDeclaringType_TypeInstances.Add(aType);
                    }

                    foreach (var xArg in aType.GetGenericArguments())
                    {
                        xCheckType(xArg);
                    }
                }
            });
            foreach (var xType in aAsm.GetTypes())
            {
                if (xType.BaseType != null)
                {
                    xCheckType(xType.BaseType);
                }
                foreach (var xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (xShouldSkipMain && xMethod == aAsm.EntryPoint)
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
                                    if (!xResult.UsedGenericMethods.Contains(xName))
                                    {
                                        xResult.UsedGenericMethods.Add(xName);
                                    }
                                    // add to the list of the declaring assembly, so it can generate it later on
                                    if (!xDeclaringType_MethodInstances.Contains(xReader.OperandValueMethod))
                                    {
                                        xDeclaringType_MethodInstances.Add(xReader.OperandValueMethod);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            return xResult;
        }

        private List<Assembly> GetAssembliesToScanForGenerics(IEnumerable<Assembly> xAllAssemblies)
        {
            var xAssembliesToScanForGenerics = new List<Assembly>();
            foreach (var xAsm in xAllAssemblies)
            {
                var xLastTime = File.GetLastWriteTimeUtc(xAsm.Location).ToBinary();
                var xChecksumFileName = GetChecksumFile(xAsm);
                if (File.Exists(xChecksumFileName))
                {
                    var xChecksumStr = File.ReadAllText(xChecksumFileName);
                    long xChecksum;
                    if (Int64.TryParse(xChecksumStr, out xChecksum))
                    {
                        if (xChecksum == xLastTime)
                        {
                            // todo: remove or move to other logging
                            Console.WriteLine("Assembly '{0}' doesn't need a recompile", xAsm.GetName().Name);
                            continue;
                        }
                    }
                }
                xAssembliesToScanForGenerics.Add(xAsm);
            }
            return xAssembliesToScanForGenerics;
        }
    }
}
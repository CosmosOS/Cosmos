#define VERBOSE_DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.IL;
using Indy.IL2CPU.Plugs;
using Microsoft.Samples.Debugging.CorSymbolStore;

namespace Indy.IL2CPU.Compiler
{
    public partial class CompilerHelper
    {
        public CompilerHelper()
        {
            SkipList=new List<Assembly>();
        }
        public List<Assembly> SkipList
        {
            get;
            set;
        }

        /// <summary>
        /// first parameter tells which assembly this assembler is for. second param tells whether it's the entrypoint assembly
        /// </summary>
        public event Func<Assembly, bool, Assembler.Assembler> GetAssembler;
        public event Action<Assembly, Assembler.Assembler> SaveAssembler;
        public event Func<OpCodeMap> GetOpCodeMap;
        public event Action<LogSeverityEnum, string> DebugLog;

        private Dictionary<Assembly, AssemblyCompilationInfo> mAssemblyInfos =
            new Dictionary<Assembly, AssemblyCompilationInfo>();

        // does based on instances is unique?
        private List<MethodBase> mAllMethods = new List<MethodBase>();
        private int mNextMethodToScan = 0;
        private List<FieldInfo> mAllStaticFields= new List<FieldInfo>();

        public List<string> Plugs = new List<string>();
        public DebugMode DebugMode;
        public TraceAssemblies TraceAssemblies;

        private Assembly mEntryPointAssembly;
        private OpCodeMap mCurrentMap;
        public void CompileExe(Assembly aAssembly)
        {
            mEntryPointAssembly = aAssembly;
            Initialize();
            // add standard methods (mainly the entry point)
            AddStandardMethods();
            ScanMethods();
            foreach(var xAsmInfo in mAssemblyInfos.Values)
            {
                CompileAssembly(xAsmInfo);
            }
        }

        private void CompileAssembly(AssemblyCompilationInfo info)
        {
            using(var xCurrentAssembler = GetAssembler(info.Assembly, info.Assembly == mEntryPointAssembly))
            {
                mCurrentAssemblyCompilationInfo = info;
                try
                {
                    // emit the methods. 
                    foreach (var xMethod in info.Methods.Values)
                    {
                        CompileMethod(xMethod, xCurrentAssembler);
                    }

                    // emit the fields
                    foreach (var xField in info.StaticFields.Values)
                    {
                        CompileStaticField(xField, xCurrentAssembler);
                    }

                    // emit the method and type id data members
                    foreach (var xIdMember in info.IDLabels)
                    {
                        xCurrentAssembler.DataMembers.Add(new DataMember(xIdMember, new int[1]));
                    }

                    // emit externals
                    foreach (var xExternal in info.Externals)
                    {
                        new ExternalLabel(xExternal);
                    }

                    // todo: implement string init method, vmt init methods, cctor calling method, etc
                    SaveAssembler(info.Assembly, xCurrentAssembler);
                }finally
                {
                    mCurrentAssemblyCompilationInfo = null;
                }
            }
        }

        private void CompileStaticField(FieldInfo aField, Assembler.Assembler aAssembler)
        {
            //ProgressChanged.Invoke(String.Format("Processing static field: {0}", xCurrentField.GetFullName()));
            string xFieldName = aField.GetFullName();
            xFieldName = DataMember.GetStaticFieldName(aField);
            if (aAssembler.DataMembers.Count(x => x.Name == xFieldName) == 0)
            {
                var xItemList = (from item in aField.GetCustomAttributes(false)
                                 where item.GetType().FullName == "ManifestResourceStreamAttribute"
                                 select item).ToList();

                object xItem = null;
                if (xItemList.Count > 0)
                    xItem = xItemList[0];
                string xManifestResourceName = null;
                if (xItem != null)
                {
                    var xItemType = xItem.GetType();
                    xManifestResourceName = (string)xItemType.GetField("ResourceName").GetValue(xItem);
                }
                if (xManifestResourceName != null)
                {
                    //RegisterType(xCurrentField.FieldType);
                    //string xFileName = Path.Combine(mOutputDir,
                    //                                (xCurrentField.DeclaringType.Assembly.FullName + "__" + xManifestResourceName).Replace(",",
                    //                                                                                                                       "_") + ".res");
                    //using (var xStream = xCurrentField.DeclaringType.Assembly.GetManifestResourceStream(xManifestResourceName)) {
                    //    if (xStream == null) {
                    //        throw new Exception("Resource '" + xManifestResourceName + "' not found!");
                    //    }
                    //    using (var xTarget = File.Create(xFileName)) {
                    //        // todo: abstract this array code out.
                    //        xTarget.Write(BitConverter.GetBytes(Engine.RegisterType(Engine.GetType("mscorlib",
                    //                                                                               "System.Array"))),
                    //                      0,
                    //                      4);
                    //        xTarget.Write(BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray),
                    //                      0,
                    //                      4);
                    //        xTarget.Write(BitConverter.GetBytes((int)xStream.Length), 0, 4);
                    //        xTarget.Write(BitConverter.GetBytes((int)1), 0, 4);
                    //        var xBuff = new byte[128];
                    //        while (xStream.Position < xStream.Length) {
                    //            int xBytesRead = xStream.Read(xBuff, 0, 128);
                    //            xTarget.Write(xBuff, 0, xBytesRead);
                    //        }
                    //    }
                    //}
                    //mAssembler.DataMembers.Add(new DataMember("___" + xFieldName + "___Contents",
                    //                                          "incbin",
                    //                                          "\"" + xFileName + "\""));
                    //mAssembler.DataMembers.Add(new DataMember(xFieldName,
                    //                                          "dd",
                    //                                          "___" + xFieldName + "___Contents"));
                    throw new NotImplementedException();
                }
                else
                {
                    uint xTheSize;
                    //string theType = "db";
                    Type xFieldTypeDef = aField.FieldType;
                    if (!xFieldTypeDef.IsClass || xFieldTypeDef.IsValueType)
                    {
                        xTheSize = SizeOfType(aField.FieldType);
                    }
                    else
                    {
                        xTheSize = 4;
                    }
                    var xData = new byte[xTheSize];
                    try
                    {
                        object xValue = aField.GetValue(null);
                        if (xValue != null)
                        {
                            try
                            {
                                xData = new byte[xTheSize];
                                if (xValue.GetType().IsValueType)
                                {
                                    for (int x = 0; x < xTheSize; x++)
                                    {
                                        xData[x] = Marshal.ReadByte(xValue,
                                                                    x);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch
                    {
                    }
                    aAssembler.DataMembers.Add(new DataMember(xFieldName, xData));
                }
            }
        }

        private void CompileMethod(MethodBase xCurrentMethod, Assembler.Assembler assembler)
        {
            try
            {
                if (xCurrentMethod.IsAbstract)
                {
                    return;
                }
                string xMethodName = Label.GetFullName(xCurrentMethod);
                TypeInformation xTypeInfo = null;
                if (!xCurrentMethod.IsStatic)
                {
                    xTypeInfo = GetTypeInfo(xCurrentMethod.DeclaringType);
                }
                var xMethodScanInfo = new SortedList<string, object>(StringComparer.InvariantCultureIgnoreCase);
                MethodInformation xMethodInfo = GetMethodInfo(xCurrentMethod, xCurrentMethod
                                                              , xMethodName, xTypeInfo, DebugMode != DebugMode.None,
                                                              xMethodScanInfo);

                Op xOp = GetOpFromType(mCurrentMap.MethodHeaderOp, null, xMethodInfo);
                xOp.Assembler = assembler;
#if VERBOSE_DEBUG
                    string comment = "(No Type Info available)";
                    if (xMethodInfo.TypeInfo != null)
                    {
                        comment = "Type Info:\r\n \r\n" + xMethodInfo.TypeInfo;
                    }
                    foreach (string s in comment.Trim().Split(new string[] { "\r\n" }
                     , StringSplitOptions.RemoveEmptyEntries))
                    {
                        new Comment(s);
                    }
                    comment = xMethodInfo.ToString();
                    foreach (string s in comment.Trim().Split(new string[] { "\r\n" }
                     , StringSplitOptions.RemoveEmptyEntries))
                    {
                        new Comment(s);
                    }
#endif
                xOp.Assemble();
                MethodBase xCustomImplementation = GetCustomMethodImplementation(xMethodName);
                bool xIsCustomImplementation = (xCustomImplementation != null);
                // what to do if a method doesn't have a body?
                bool xContentProduced = false;
                if (xIsCustomImplementation)
                {
                    // this is for the support for having extra fields on types, and being able to use
                    // them in custom implementation methods
                    CustomMethodImplementationProxyOp xProxyOp
                        = (CustomMethodImplementationProxyOp) GetOpFromType(
                                                                  mCurrentMap.CustomMethodImplementationProxyOp, null,
                                                                  xMethodInfo);
                    xProxyOp.Assembler = assembler;
                    xProxyOp.ProxiedMethod = xCustomImplementation;
                    xProxyOp.Assemble();
                    xContentProduced = true;
                }
                if (!xContentProduced)
                {
                    Type xOpType = mCurrentMap.GetOpForCustomMethodImplementation(xMethodName);
                    if (xOpType != null)
                    {
                        Op xMethodOp = GetOpFromType(xOpType, null, xMethodInfo);
                        if (xMethodOp != null)
                        {
                            xMethodOp.Assembler = assembler;
                            xMethodOp.Assemble();
                            xContentProduced = true;
                        }
                    }
                }
                if (!xContentProduced)
                {
                    if (mCurrentMap.HasCustomAssembleImplementation(xMethodInfo))
                    {
                        mCurrentMap.DoCustomAssembleImplementation(assembler, xMethodInfo);
                        // No plugs, we need to compile the IL from the method
                    }
                    else
                    {
                        MethodBody xBody = xCurrentMethod.GetMethodBody();
                        // todo: add better detection of implementation state
                        if (xBody != null)
                        {
                            assembler.StackContents.Clear();
                            var xReader = new ILReader(xCurrentMethod);
                            #region let instructions prepare themselves
                            while(xReader.Read())
                            {
                                mCurrentMap.ScanILCode(xReader, xMethodInfo, xMethodScanInfo);
                            }
                            #endregion
                            xReader.Restart();
                            var xInstructionInfos = new List<DebugSymbolsAssemblyTypeMethodInstruction>();

                            // Section currently is dead code. Working on matching it up 
                            // with contents from inside the read
                            int[] xCodeOffsets = null;
                            if (DebugMode == DebugMode.Source)
                            {
                                var xSymbolReader = GetSymbolReaderForAssembly(xCurrentMethod.DeclaringType.Assembly);
                                if (xSymbolReader != null)
                                {
                                    var xSmbMethod =
                                        xSymbolReader.GetMethod(new SymbolToken(xCurrentMethod.MetadataToken));
                                    // This gets the Sequence Points.
                                    // Sequence Points are spots that identify what the compiler/debugger says is a spot
                                    // that a breakpoint can occur one. Essentially, an atomic source line in C#
                                    if (xSmbMethod != null)
                                    {
                                        xCodeOffsets = new int[xSmbMethod.SequencePointCount];
                                        var xCodeDocuments = new ISymbolDocument[xSmbMethod.SequencePointCount];
                                        var xCodeLines = new int[xSmbMethod.SequencePointCount];
                                        var xCodeColumns = new int[xSmbMethod.SequencePointCount];
                                        var xCodeEndLines = new int[xSmbMethod.SequencePointCount];
                                        var xCodeEndColumns = new int[xSmbMethod.SequencePointCount];
                                        xSmbMethod.GetSequencePoints(xCodeOffsets, xCodeDocuments
                                                                     , xCodeLines, xCodeColumns, xCodeEndLines,
                                                                     xCodeEndColumns);
                                    }
                                }
                            }

                            // Scan each IL op in the method
                            while (xReader.Read())
                            {
                                ExceptionHandlingClause xCurrentHandler = null;

                                #region Exception handling support code

                                // todo: add support for nested handlers using a stack or so..
                                foreach (ExceptionHandlingClause xHandler in xBody.ExceptionHandlingClauses)
                                {
                                    if (xHandler.TryOffset > 0)
                                    {
                                        if (xHandler.TryOffset <= xReader.NextPosition &&
                                            (xHandler.TryLength + xHandler.TryOffset) > xReader.NextPosition)
                                        {
                                            if (xCurrentHandler == null)
                                            {
                                                xCurrentHandler = xHandler;
                                                continue;
                                            }
                                            else if (xHandler.TryOffset > xCurrentHandler.TryOffset &&
                                                     (xHandler.TryLength + xHandler.TryOffset) <
                                                     (xCurrentHandler.TryLength + xCurrentHandler.TryOffset))
                                            {
                                                // only replace if the current found handler is narrower
                                                xCurrentHandler = xHandler;
                                                continue;
                                            }
                                        }
                                    }
                                    if (xHandler.HandlerOffset > 0)
                                    {
                                        if (xHandler.HandlerOffset <= xReader.NextPosition &&
                                            (xHandler.HandlerOffset + xHandler.HandlerLength) > xReader.NextPosition)
                                        {
                                            if (xCurrentHandler == null)
                                            {
                                                xCurrentHandler = xHandler;
                                                continue;
                                            }
                                            else if (xHandler.HandlerOffset > xCurrentHandler.HandlerOffset &&
                                                     (xHandler.HandlerOffset + xHandler.HandlerLength) <
                                                     (xCurrentHandler.HandlerOffset + xCurrentHandler.HandlerLength))
                                            {
                                                // only replace if the current found handler is narrower
                                                xCurrentHandler = xHandler;
                                                continue;
                                            }
                                        }
                                    }
                                    if ((xHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0)
                                    {
                                        if (xHandler.FilterOffset > 0)
                                        {
                                            if (xHandler.FilterOffset <= xReader.NextPosition)
                                            {
                                                if (xCurrentHandler == null)
                                                {
                                                    xCurrentHandler = xHandler;
                                                    continue;
                                                }
                                                else if (xHandler.FilterOffset > xCurrentHandler.FilterOffset)
                                                {
                                                    // only replace if the current found handler is narrower
                                                    xCurrentHandler = xHandler;
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion

                                xMethodInfo.CurrentHandler = xCurrentHandler;
                                xOp = GetOpFromType(mCurrentMap.GetOpForOpCode(xReader.OpCode), xReader, xMethodInfo);

                                xOp.Assembler = assembler;
                                new Comment("StackItems = " + assembler.StackContents.Count);
                                foreach (var xStackContent in assembler.StackContents)
                                {
                                    new Comment("    " + xStackContent.Size);
                                }

                                // Create label for current point
                                string xLabel = Op.GetInstructionLabel(xReader);
                                if (xLabel.StartsWith("."))
                                {
                                    xLabel = DataMember.FilterStringForIncorrectChars(
                                        Label.LastFullLabel + "__DOT__" + xLabel.Substring(1));
                                }

                                // Possibly emit Tracer call
                                EmitTracer(xOp, xCurrentMethod.DeclaringType.Namespace, (int) xReader.Position,
                                           xCodeOffsets, xLabel, assembler);

                                if (mSymbols != null)
                                {
                                    var xMLSymbol = new MLDebugSymbol();
                                    xMLSymbol.LabelName = xLabel;
                                    int xStackSize = (from item in assembler.StackContents
                                                      let xSize = (item.Size%4 == 0)
                                                                      ? item.Size
                                                                      : (item.Size + (4 - (item.Size%4)))
                                                      select xSize).Sum();
                                    xMLSymbol.StackDifference = xMethodInfo.LocalsSize + xStackSize;
                                    try
                                    {
                                        xMLSymbol.AssemblyFile = xCurrentMethod.DeclaringType.Assembly.Location;
                                    }
                                    catch (NotSupportedException)
                                    {
                                        xMLSymbol.AssemblyFile = "DYNAMIC: " +
                                                                 xCurrentMethod.DeclaringType.Assembly.FullName;
                                    }
                                    xMLSymbol.MethodToken = xCurrentMethod.MetadataToken;
                                    xMLSymbol.TypeToken = xCurrentMethod.DeclaringType.MetadataToken;
                                    xMLSymbol.ILOffset = (int) xReader.Position;
                                    mSymbols.Add(xMLSymbol);
                                }
                                xOp.Assemble();
                                //if (xInstructionInfo != null) {
                                //    int xNewStack = (from item in mAssembler.StackContents
                                //                     let xSize = (item.Size % 4 == 0) ? item.Size : (item.Size + (4 - (item.Size % 4)))
                                //                     select xSize).Sum();
                                //    xInstructionInfo.StackResult = xNewStack - xCurrentStack;
                                //    xInstructionInfo.StackResultSpecified = true;
                                //    xInstructionInfos.Add(xInstructionInfo);
                                //}
                            }
                            if (mSymbols != null)
                            {
                                MLDebugSymbol[] xSymbols;
                                xSymbols = mSymbols.ToArray();
                            }
                        }
                        else
                        {
                            if ((xCurrentMethod.Attributes & MethodAttributes.PinvokeImpl) != 0)
                            {
                                LogMessage(LogSeverityEnum.Error,
                                           "Method '{0}' not generated!",
                                           xCurrentMethod.GetFullName());
                                new Comment("Method not being generated yet, as it's handled by a PInvoke");
                            }
                            else
                            {
                                LogMessage(LogSeverityEnum.Error,
                                           "Method '{0}' not generated!",
                                           xCurrentMethod.GetFullName());
                                new Comment("Method not being generated yet, as it's handled by an iCall");
                            }
                        }
                    }
                }
                xOp = GetOpFromType(mCurrentMap.MethodFooterOp, null, xMethodInfo);
                xOp.Assembler = assembler;
                xOp.Assemble();
                assembler.StackContents.Clear();
            }
            catch (Exception e)
            {
                LogMessage(LogSeverityEnum.Error, xCurrentMethod.GetFullName());
                LogMessage(LogSeverityEnum.Warning, e.ToString());
                throw;
            }
        }

        private 
            
            
            void Initialize()
        {
            mCurrentMap = GetOpCodeMap();
            InitializePlugs(Plugs);
            mCurrentMap.SetServiceProvider(this);
            mCurrentMap.Initialize(new[] { mEntryPointAssembly });
        }

        private void CheckAssemblyForPlugAssemblies(Assembly aAssembly)
        {
            //If in the GAC, then ignore assembly
            if (aAssembly.GlobalAssemblyCache)
            {
                return;
            }

            // todo: implement this again
            // todo: try to get rid of the try..catch. find a way to detect dynamic assemblies.
            try
            {
                //Search for related .config file
                //string configFile = String.Intern(aAssembly.Location + ".cosmos-config");
                //if (System.IO.File.Exists(configFile))
                //{
                //    //Load and parse all PlugAssemblies referred to in the .config file
                //    foreach (Assembly xAssembly in GetAssembliesFromConfigFile(configFile))
                //    {
                //        LoadPlugAssembly(xAssembly);
                //    }
                //}
            }
            catch
            {
            }
        }


        private void InitializePlugs(IEnumerable<string> aPlugs)
        {
            if (mPlugMethods != null)
            {
                throw new Exception("PlugMethods list already initialized!");
            }
            if (mPlugFields != null)
            {
                throw new Exception("PlugFields list already initialized!");
            }

            mPlugMethods = new SortedList<string, MethodBase>();
            mPlugFields = new SortedList<Type, Dictionary<string, PlugFieldAttribute>>(new TypeComparer());

            //AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies())
            {
                CheckAssemblyForPlugAssemblies(xAsm);
            }
            List<Assembly> xPlugs = new List<Assembly>();
            var xComparer = new AssemblyEqualityComparer();

            foreach (string s in aPlugs)
            {
                Assembly a = Assembly.LoadFrom(s);
                a.GetTypes();
                if (!xPlugs.Contains(a,
                                     xComparer))
                {
                    xPlugs.Add(a);
                }
            }

            foreach (var item in GetPlugAssemblies())
            {
                if (!xPlugs.Contains(item,
                                     xComparer))
                {
                    xPlugs.Add(item);
                }
            }

            foreach (Assembly xAssemblyDef in xPlugs)
            {
                LoadPlugAssembly(xAssemblyDef);
            }
        }

        private IList<Assembly> GetPlugAssemblies()
        {
            var xResult = mCurrentMap.GetPlugAssemblies();
            xResult.Add(typeof(CompilerHelper).Assembly);
            return xResult;
        }

        public readonly bool RunningOnMono;

        private void LoadPlugAssembly(Assembly aAssemblyDef)
        {
            foreach (var xType in (from item in aAssemblyDef.GetTypes()
                                   let xCustomAttribs = item.GetCustomAttributes(typeof(PlugAttribute),
                                                                                 false)
                                   where xCustomAttribs != null && xCustomAttribs.Length > 0
                                   select new KeyValuePair<Type, PlugAttribute>(item,
                                                                                (PlugAttribute)xCustomAttribs[0])))
            {
                PlugAttribute xPlugAttrib = xType.Value;
                if (xPlugAttrib.IsMonoOnly && !RunningOnMono)
                {
                    continue;
                }
                if (xPlugAttrib.IsMicrosoftdotNETOnly && RunningOnMono)
                {
                    continue;
                }
                Type xTypeRef = xPlugAttrib.Target;
                if (xTypeRef == null)
                {
                    xTypeRef = Type.GetType(xPlugAttrib.TargetName,
                                            true);
                }

                PlugFieldAttribute[] xTypePlugFields = xType.Key.GetCustomAttributes(typeof(PlugFieldAttribute),
                                                                                     false) as PlugFieldAttribute[];
                if (xTypePlugFields != null && xTypePlugFields.Length > 0)
                {
                    Dictionary<string, PlugFieldAttribute> xPlugFields;
                    if (mPlugFields.ContainsKey(xTypeRef))
                    {
                        xPlugFields = mPlugFields[xTypeRef];
                    }
                    else
                    {
                        mPlugFields.Add(xTypeRef,
                                        xPlugFields = new Dictionary<string, PlugFieldAttribute>());
                    }
                    foreach (var xPlugField in xTypePlugFields)
                    {
                        if (xPlugAttrib.IsMonoOnly && !RunningOnMono)
                        {
                            continue;
                        }
                        if (xPlugAttrib.IsMicrosoftdotNETOnly && RunningOnMono)
                        {
                            continue;
                        }
                        if (!xPlugFields.ContainsKey(xPlugField.FieldId))
                        {
                            xPlugFields.Add(xPlugField.FieldId,
                                            xPlugField);
                        }
                    }
                }

                foreach (MethodBase xMethod in xType.Key.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    PlugMethodAttribute xPlugMethodAttrib = xMethod.GetCustomAttributes(typeof(PlugMethodAttribute),
                                                                                        true).Cast<PlugMethodAttribute>().FirstOrDefault();
                    string xSignature = String.Empty;
                    if (xPlugMethodAttrib != null)
                    {
                        xSignature = xPlugMethodAttrib.Signature;
                        if (!xPlugMethodAttrib.Enabled)
                        {
                            continue;
                        }
                        if (xPlugAttrib.IsMonoOnly && !RunningOnMono)
                        {
                            continue;
                        }
                        if (xPlugAttrib.IsMicrosoftdotNETOnly && RunningOnMono)
                        {
                            continue;
                        }
                        if (!String.IsNullOrEmpty(xSignature))
                        {
                            if (!mPlugMethods.ContainsKey(xSignature))
                            {
                                mPlugMethods.Add(xSignature,
                                                 xMethod);
                            }
                            continue;
                        }
                    }
                    foreach (MethodBase xOrigMethodDef in xTypeRef.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        string xStrippedSignature = GetStrippedMethodBaseFullName(xMethod,
                                                                                  xOrigMethodDef);
                        string xOrigStrippedSignature = GetStrippedMethodBaseFullName(xOrigMethodDef,
                                                                                      null);
                        if (xOrigStrippedSignature == xStrippedSignature)
                        {
                            if (!mPlugMethods.ContainsKey(MethodInfoLabelGenerator.GenerateLabelName(xOrigMethodDef)))
                            {
                                mPlugMethods.Add(MethodInfoLabelGenerator.GenerateLabelName(xOrigMethodDef),
                                                 xMethod);
                            }
                        }
                    }
                    foreach (MethodBase xOrigMethodDef in xTypeRef.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        string xStrippedSignature = GetStrippedMethodBaseFullName(xMethod,
                                                                                  xOrigMethodDef);
                        string xOrigStrippedSignature = GetStrippedMethodBaseFullName(xOrigMethodDef,
                                                                                      null);
                        if (xOrigStrippedSignature == xStrippedSignature)
                        {
                            if (mPlugMethods.ContainsKey(MethodInfoLabelGenerator.GenerateLabelName(xOrigMethodDef)))
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            mPlugMethods.Add(MethodInfoLabelGenerator.GenerateLabelName(xOrigMethodDef),
                                             xMethod);
                        }
                    }
                }
            }
        }

        private void ScanMethods()
        {
            int xOldCount = 0;
            do
            {
                xOldCount = mAllMethods.Count;
                ScanMethods_OnePass();
            } while (xOldCount != mAllMethods.Count);
            
        }


        private IDictionary<string, MethodBase> mPlugMethods;
        private IDictionary<Type, Dictionary<string, PlugFieldAttribute>> mPlugFields;
        private AssemblyCompilationInfo mCurrentAssemblyCompilationInfo = null;

        /// <summary>
        ///  contains the code for scanning for new methods, but just one pass.
        /// </summary>
        private void ScanMethods_OnePass()
        {
            var xEmptyDict = new Dictionary<string, object>();
            for (int i = mNextMethodToScan; i < mAllMethods.Count; i++)
            {
                var xCurrentMethod = mAllMethods[i];
                if (xCurrentMethod.IsAbstract)
                {
                    continue;
                }
                if(!mAssemblyInfos.TryGetValue(xCurrentMethod.DeclaringType.Assembly, out mCurrentAssemblyCompilationInfo))
                {
                    mCurrentAssemblyCompilationInfo = new AssemblyCompilationInfo
                                                          {Assembly = xCurrentMethod.DeclaringType.Assembly};
                    mAssemblyInfos.Add(mCurrentAssemblyCompilationInfo.Assembly, mCurrentAssemblyCompilationInfo);
                }
                try
                {
                    string xMethodName = Label.GetFullName(xCurrentMethod);
                    TypeInformation xTypeInfo = null;
                    if (!xCurrentMethod.IsStatic)
                    {
                        xTypeInfo = GetTypeInfo(xCurrentMethod.DeclaringType);
                    }
                    MethodInformation xMethodInfo;
                    // using (mMethodsLocker.AcquireReaderLock())
                    {
                        xEmptyDict.Clear();
                        xMethodInfo = GetMethodInfo(xCurrentMethod,
                                                    xCurrentMethod,
                                                    xMethodName,
                                                    xTypeInfo,
                                                    false, // debug mode
                                                    xEmptyDict);
                    }
                    MethodBase xCustomImplementation = GetCustomMethodImplementation(xMethodName);
                    if (xCustomImplementation != null)
                    {
                        try
                        {
                            AddMethod(xCustomImplementation);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Method " + xCurrentMethod.GetFullName() + " has called " + e.Message +
                                                "! Probably it needs to be plugged");
                        }
                        //   using (mMethodsLocker.AcquireReaderLock())
                        //{
                        //    mMethods[xCurrentMethod].Implementation = xCustomImplementation;
                        //}
                        continue;
                    }
                    Type xOpType = mCurrentMap.GetOpForCustomMethodImplementation(xMethodName);
                    if (xOpType != null)
                    {
                        Op xMethodOp = GetOpFromType(xOpType, null, xMethodInfo);
                        if (xMethodOp != null)
                        {
                            continue;
                        }
                    }
                    if (mCurrentMap.HasCustomAssembleImplementation(xMethodInfo))
                    {
                        mCurrentMap.ScanCustomAssembleImplementation(xMethodInfo);
                        continue;
                    }

                    //xCurrentMethod.GetMethodImplementationFlags() == MethodImplAttributes.
                    MethodBody xBody = xCurrentMethod.GetMethodBody();
                    // todo: add better detection of implementation state
                    if (xBody != null)
                    {
                        ILReader xReader = new ILReader(xCurrentMethod);
                        var xInstructionInfos = new List<DebugSymbolsAssemblyTypeMethodInstruction>();
                        while (xReader.Read())
                        {
                            SortedList<string, object> xInfo = new SortedList<string, object>();
                            // using (mMethodsLocker.AcquireReaderLock())
                            //{
                            //    xInfo = mMethods[xCurrentMethod].Info;
                            //}
                            mCurrentMap.ScanILCode(xReader, xMethodInfo, xInfo);
                            switch (xReader.OpCode)
                            {
                                case OpCodeEnum.Call:
                                case OpCodeEnum.Callvirt:
                                case OpCodeEnum.Newobj:
                                case OpCodeEnum.Ldftn:
                                    AddMethod(xReader.OperandValueMethod);
                                    break;
                                case OpCodeEnum.Initobj:
                                case OpCodeEnum.Ldelema:
                                    //Add(xReader.OperandValueType);
                                    break;
                                case OpCodeEnum.Stsfld:
                                case OpCodeEnum.Ldsfld:
                                case OpCodeEnum.Ldsflda:
                                    AddStaticField(xReader.OperandValueField);
                                    break;
                                case OpCodeEnum.Ldtoken:
                                    if (xReader.OperandValueType != null)
                                    {
                                        //RegisterType(xReader.OperandValueType);
                                        break;
                                    }
                                    if (xReader.OperandValueField != null)
                                    {
                                        AddStaticField(xReader.OperandValueField);
                                        break;
                                    }
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    mCurrentAssemblyCompilationInfo = null;
                }
            }
            mNextMethodToScan = mAllMethods.Count;
        }

        private Op GetOpFromType(Type aType, ILReader aReader, MethodInformation aMethodInfo)
        {
            var xResult = (Op)Activator.CreateInstance(aType, aReader, aMethodInfo);
            xResult.SetServiceProvider(this);
            return xResult;
        }

        private MethodBase GetCustomMethodImplementation(string aMethodName)
        {
            if (mPlugMethods.ContainsKey(aMethodName))
            {
                return mPlugMethods[aMethodName];
            }
            return null;
        }

        private void AddMethod(MethodBase aMethod)
        {
            if ((from item in mAllMethods
                 where item.GetFullName() == aMethod.GetFullName()
                 select item).Any())
            {
                return;
            }
            mAllMethods.Add(aMethod);
            AssemblyCompilationInfo xAsmCompileInfo;
            if (!mAssemblyInfos.TryGetValue(aMethod.DeclaringType.Assembly, out xAsmCompileInfo))
            {
                mAssemblyInfos.Add(aMethod.DeclaringType.Assembly,
                                   xAsmCompileInfo=new AssemblyCompilationInfo {Assembly = aMethod.DeclaringType.Assembly});
            }
            xAsmCompileInfo.AddMethod(aMethod);
        }


        
        private void AddStaticField(FieldInfo aField)
        {
            if (mAllStaticFields.Contains(aField))
            {
                return;
            }
            mAllStaticFields.Add(aField);
            AssemblyCompilationInfo xAsmCompileInfo;
            if (!mAssemblyInfos.TryGetValue(aField.DeclaringType.Assembly, out xAsmCompileInfo))
            {
                mAssemblyInfos.Add(aField.DeclaringType.Assembly,
                                   xAsmCompileInfo = new AssemblyCompilationInfo { Assembly = aField.DeclaringType.Assembly });
            }
            xAsmCompileInfo.AddStaticField(aField);
        }

        private void AddStandardMethods()
        {
            AddMethod(mEntryPointAssembly.EntryPoint.DeclaringType.GetMethod("Init", new Type[0]));
            AddMethod(RuntimeEngineRefs.InitializeApplicationRef);
            AddMethod(RuntimeEngineRefs.FinalizeApplicationRef);
            AddMethod(typeof(Assembler.Assembler).GetMethod("PrintException"));
            AddMethod(VTablesImplRefs.LoadTypeTableRef);
            AddMethod(VTablesImplRefs.SetMethodInfoRef);
            AddMethod(VTablesImplRefs.IsInstanceRef);
            AddMethod(VTablesImplRefs.SetTypeInfoRef);
            AddMethod(VTablesImplRefs.GetMethodAddressForTypeRef);
            AddMethod(GCImplementationRefs.IncRefCountRef);
            AddMethod(GCImplementationRefs.DecRefCountRef);
            AddMethod(GCImplementationRefs.AllocNewObjectRef);
        
        }

        private static string GetStrippedMethodBaseFullName(MethodBase aMethod,
                                                    MethodBase aRefMethod)
        {
            StringBuilder xBuilder = new StringBuilder(256);
            string[] xParts = aMethod.ToString().Split(' ');
            string[] xParts2 = xParts.Skip(1).ToArray();
            MethodInfo xMethodInfo = aMethod as MethodInfo;
            if (xMethodInfo != null)
            {
                xBuilder.Append(xMethodInfo.ReturnType.FullName);
            }
            else
            {
                if (aMethod is ConstructorInfo)
                {
                    xBuilder.Append(typeof(void).FullName);
                }
                else
                {
                    xBuilder.Append(xParts[0]);
                }
            }
            xBuilder.Append("  ");
            xBuilder.Append(".");
            xBuilder.Append(aMethod.Name);
            xBuilder.Append("(");
            ParameterInfo[] xParams = aMethod.GetParameters();
            bool xParamAdded = false;
            for (int i = 0; i < xParams.Length; i++)
            {
                if (i == 0 && (aRefMethod != null && !aRefMethod.IsStatic))
                {
                    continue;
                }
                if (xParams[i].IsDefined(typeof(FieldAccessAttribute), true))
                {
                    continue;
                }
                if (xParamAdded)
                {
                    xBuilder.Append(", ");
                }
                xBuilder.Append(xParams[i].ParameterType.FullName);
                xParamAdded = true;
            }
            xBuilder.Append(")");
            return String.Intern(xBuilder.ToString());
        }

        private ISymbolReader GetSymbolReaderForAssembly(Assembly aAssembly)
        {
            try
            {
                return SymbolAccess.GetReaderForFile(aAssembly.Location);
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        protected void EmitTracer(Op aOp, string aNamespace, int aPos, int[] aCodeOffsets, string aLabel, Assembler.Assembler aAssembler)
        {
            // NOTE - These if statemens can be optimized down - but clarity is
            // more importnat the optimizations would not offer much benefit

            // Determine if a new DebugStub should be emitted
            //bool xEmit = false;
            // Skip NOOP's so we dont have breakpoints on them
            //TODO: Each IL op should exist in IL, and descendants in IL.X86.
            // Because of this we have this hack
            if (aOp.ToString() == "Indy.IL2CPU.IL.X86.Nop")
            {
                return;
            }
            else if (DebugMode == DebugMode.None)
            {
                return;
            }
            else if (DebugMode == DebugMode.Source)
            {
                // If the current position equals one of the offsets, then we have
                // reached a new atomic C# statement
                if (aCodeOffsets != null)
                {
                    if (aCodeOffsets.Contains(aPos) == false)
                    {
                        return;
                    }
                }
            }

            // Check options for Debug Level
            // Set based on TracedAssemblies
            if (TraceAssemblies == TraceAssemblies.Cosmos || TraceAssemblies == TraceAssemblies.User)
            {
                if (aNamespace.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.ToLower() == "system")
                {
                    return;
                }
                else if (aNamespace.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }
            if (TraceAssemblies == TraceAssemblies.User)
            {
                //TODO: Maybe an attribute that could be used to turn tracing on and off
                //TODO: This doesnt match Cosmos.Kernel exact vs Cosmos.Kernel., so a user 
                // could do Cosmos.KernelMine and it will fail. Need to fix this
                if (aNamespace.StartsWith("Cosmos.Kernel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.StartsWith("Cosmos.Sys", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.StartsWith("Cosmos.Hardware", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.StartsWith("Indy.IL2CPU", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }
            // If we made it this far, emit the Tracer
            mCurrentMap.EmitOpDebugHeader(aAssembler, 0, aLabel);
        }

        private List<MLDebugSymbol> mSymbols = new List<MLDebugSymbol>();

    }
}
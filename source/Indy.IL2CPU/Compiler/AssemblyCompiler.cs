using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Indy.IL2CPU.IL;
using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using System.Xml;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;


namespace Indy.IL2CPU.Compiler
{
    /// <summary>
    /// This is our new compiler class. It will compile an assembly completely. It doesn't do any scanning, but just compiles each type and method.
    /// </summary>
    public partial class AssemblyCompiler
    {
        public AssemblyCompiler()
        {
            Plugs = new List<string>();
            AssemblyReferences = new List<string>();
            Types = new List<Type>();
            Methods = new List<MethodBase>();
            StaticFields = new List<FieldInfo>();
        }

#region Settings properties

        public Assembly Assembly
        {
            get;
            set;
        }

        public OpCodeMap OpCodeMap
        {
            get;
            set;
        }

        public Assembler.Assembler Assembler
        {
            get;
            set;
        }

        public List<string> Plugs
        {
            get;
            set;
        }

        public DebugMode DebugMode
        {
            get;
            set;
        }

        public byte DebugComNumber
        {
            get;
            set;
        }

        public bool IsEntrypointAssembly
        {
            get;
            set;
        }

        public List<string> AssemblyReferences
        {
            get;
            set;
        }

        public Action<LogSeverityEnum, string> DebugLog;

#endregion Setting Properties

        public void Execute()
        {
            EnsureCanExecute();
            Initialize();
            ScanAssembly();
            CompileAllMethods();
            CompileAllStaticFields();
        }

        private void Initialize()
        {
            InitializePlugs(Plugs);
            OpCodeMap.SetServiceProvider(this);
            OpCodeMap.Initialize(Assembler, new []{Assembly});
        }

        private void ScanAssembly()
        {
            foreach (var xType in Assembly.GetTypes())
            {
                Types.Add(xType);
            }


            int typesToProcess = Types.Count;

            for (int i = 0 ; i < typesToProcess ;i++)
            {
                var xType = Types[i]; 

                foreach (var xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    Methods.Add(xMethod);
                }
                foreach (var xCtor in xType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    Methods.Add(xCtor);
                }
                foreach (var xField in xType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    StaticFields.Add(xField);
                }

                ////BK Test 
                //Types.RemoveAt(0); 
            }
        }

        private MethodBase GetCustomMethodImplementation(string aMethodName)
        {
            if (mPlugMethods.ContainsKey(aMethodName))
            {
                return mPlugMethods[aMethodName];
            }
            return null;
        }


        private void CompileAllStaticFields()
        {
            foreach (var xField in StaticFields)
            {
                if (xField.DeclaringType.IsGenericTypeDefinition)
                {
                    // the generic type definitions (Nullable<>) shouldnt be emitted
                    continue;
                }
                string xFieldName = xField.GetFullName();
                xFieldName = DataMember.GetStaticFieldName(xField);
                if (Assembler.DataMembers.Count(x => x.Name == xFieldName) == 0)
                {
                    var xItemList = (from item in xField.GetCustomAttributes(false)
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
                        //RegisterType(xCurrentField.FieldType);
                        uint xTheSize;
                        //string theType = "db";
                        Type xFieldTypeDef = xField.FieldType;
                        if (!xFieldTypeDef.IsClass || xFieldTypeDef.IsValueType)
                        {
                            xTheSize = GetFieldStorageSize(xField.FieldType);
                        }
                        else
                        {
                            xTheSize = 4;
                        }
                        byte[] xData = new byte[xTheSize];
                        try
                        {
                            object xValue = xField.GetValue(null);
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
                        Assembler.DataMembers.Add(new DataMember(xFieldName, xData));
                    }
                }
            }
        }

        public List<MethodBase> Methods;
        public List<Type> Types;
        public List<FieldInfo> StaticFields;

        private int mInstructionsToSkip = 0;
        private void CompileAllMethods()
        {
            long xCount = 0;
            Console.WriteLine("Before compiling, amount of methods: {0}", Methods.Count);
            // all methods are in Methods, so we just need to iterate over them.
            foreach (var xCurrentMethod in Methods)
            {
                if (xCount % 100 == 0)
                {
                    Console.Write(new String('-', Console.BufferWidth));
                    Console.WriteLine(xCount / 100);
                    GC.Collect();
                }
                xCount++;

                if (Console.KeyAvailable)
                {
                    throw new Exception("Temporary abort");
                }
                TypeInformation xTypeInfo = null;
                if (xCurrentMethod.IsAbstract)
                {
                    continue;
                }
                if (xCurrentMethod.ContainsGenericParameters)
                {
                    // we only want concrete methods        
                    continue;
                }
                if (xCurrentMethod.DeclaringType.ContainsGenericParameters)
                {
                    continue;
                }
                if (!xCurrentMethod.IsStatic)
                {
                    xTypeInfo = GetTypeInfo(xCurrentMethod.DeclaringType);
                }
                var xMethodName = Label.GenerateLabelName(xCurrentMethod);
                var xMethodScanInfo = new SortedList<string, object>(StringComparer.InvariantCultureIgnoreCase);
                MethodInformation xMethodInfo = GetMethodInfo(xCurrentMethod, xCurrentMethod
                             , xMethodName, xTypeInfo, DebugMode != DebugMode.None, xMethodScanInfo);

                #region Prescan method
                try
                {
                    using (ILReader xReader = new ILReader(xCurrentMethod))
                    {
                        MethodBody xBody = xCurrentMethod.GetMethodBody();
                        // todo: add better detection of implementation state

                        if (xBody != null)
                        {
                            mInstructionsToSkip = 0;
                            Assembler.StackContents.Clear();

                            var xInstructionInfos = new List<DebugSymbolsAssemblyTypeMethodInstruction>();
                            while (xReader.Read())
                            {
                                OpCodeMap.ScanILCode(xReader, xMethodInfo, xMethodScanInfo);
                            }
                        }
                    }
                }
                catch
                {
                }
                #endregion

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
                Op xOp = GetOpFromType(OpCodeMap.MethodHeaderOp, null, xMethodInfo);
                xOp.Assembler = Assembler;
                xOp.SetServiceProvider(this);
                xOp.Assemble();
                MethodBase xCustomImplementation = GetCustomMethodImplementation(xMethodName);
                bool xIsCustomImplementation = (xCustomImplementation != null);
                // what to do if a method doesn't have a body?
                bool xContentProduced = false;
                if (xIsCustomImplementation)
                {
                    // this is for the support for having extra fields on types, and being able to use
                    // them in custom implementation methods
                    CustomMethodImplementationProxyOp xProxyOp = (CustomMethodImplementationProxyOp)GetOpFromType(OpCodeMap.CustomMethodImplementationProxyOp, null, xMethodInfo);
                    xProxyOp.SetServiceProvider(this);
                    xProxyOp.Assembler = Assembler;
                    xProxyOp.ProxiedMethod = xCustomImplementation;
                    xProxyOp.Assemble();
                    xContentProduced = true;
                }
                if (!xContentProduced)
                {
                    Type xOpType = OpCodeMap.GetOpForCustomMethodImplementation(xMethodName);
                    if (xOpType != null)
                    {
                        Op xMethodOp = GetOpFromType(xOpType, null, xMethodInfo);
                        if (xMethodOp != null)
                        {
                            xMethodOp.Assembler = Assembler;
                            xMethodOp.SetServiceProvider(this);
                            xMethodOp.Assemble();
                            xContentProduced = true;
                        }
                    }
                }
                if (!xContentProduced)
                {
                    if (OpCodeMap.HasCustomAssembleImplementation(xMethodInfo))
                    {
                        OpCodeMap.DoCustomAssembleImplementation(Assembler, xMethodInfo);
                        // No plugs, we need to compile the IL from the method
                    }
                    else
                    {
                        MethodBody xBody = xCurrentMethod.GetMethodBody();
                        // todo: add better detection of implementation state
                        if (xBody != null)
                        {
                            mInstructionsToSkip = 0;
                            Assembler.StackContents.Clear();
                            if (xMethodInfo.LabelName == "_BBE9E9A7F7761F7DC23F15255BBB0FF7")
                            {
                                Console.Write("");
                            }
                            var xInstructionInfos = new List<DebugSymbolsAssemblyTypeMethodInstruction>();

                            // Section currently is dead code. Working on matching it up 
                            // with contents from inside the read
                            int[] xCodeOffsets = null;
                            if (DebugMode == DebugMode.Source)
                            {
                                var xSymbolReader = GetSymbolReaderForAssembly(xCurrentMethod.DeclaringType.Assembly);
                                if (xSymbolReader != null)
                                {
                                    var xSmbMethod = xSymbolReader.GetMethod(new SymbolToken(xCurrentMethod.MetadataToken));
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
                                         , xCodeLines, xCodeColumns, xCodeEndLines, xCodeEndColumns);
                                    }
                                }
                            }
                            // Scan each IL op in the method
                            using (var xReader = new ILReader(xCurrentMethod))
                            {
                                while (xReader.Read())
                                {
                                    ExceptionHandlingClause xCurrentHandler = null;

                                    #region Exception handling support code
                                    // todo: add support for nested handlers using a stack or so..
                                    foreach (ExceptionHandlingClause xHandler in xBody.ExceptionHandlingClauses)
                                    {
                                        if (xHandler.TryOffset > 0)
                                        {
                                            if (xHandler.TryOffset <= xReader.NextPosition && (xHandler.TryLength + xHandler.TryOffset) > xReader.NextPosition)
                                            {
                                                if (xCurrentHandler == null)
                                                {
                                                    xCurrentHandler = xHandler;
                                                    continue;
                                                }
                                                else if (xHandler.TryOffset > xCurrentHandler.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentHandler.TryLength + xCurrentHandler.TryOffset))
                                                {
                                                    // only replace if the current found handler is narrower
                                                    xCurrentHandler = xHandler;
                                                    continue;
                                                }
                                            }
                                        }
                                        if (xHandler.HandlerOffset > 0)
                                        {
                                            if (xHandler.HandlerOffset <= xReader.NextPosition && (xHandler.HandlerOffset + xHandler.HandlerLength) > xReader.NextPosition)
                                            {
                                                if (xCurrentHandler == null)
                                                {
                                                    xCurrentHandler = xHandler;
                                                    continue;
                                                }
                                                else if (xHandler.HandlerOffset > xCurrentHandler.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentHandler.HandlerOffset + xCurrentHandler.HandlerLength))
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
                                    xOp = GetOpFromType(OpCodeMap.GetOpForOpCode(xReader.OpCode), xReader, xMethodInfo);

                                    xOp.Assembler = Assembler;
                                    new Comment("StackItems = " + Assembler.StackContents.Count);
                                    foreach (var xStackContent in Assembler.StackContents)
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
                                    EmitTracer(xOp, xCurrentMethod.DeclaringType.Namespace, (int)xReader.Position,
                                        xCodeOffsets, xLabel);

                                    //using (mSymbolsLocker.AcquireWriterLock())
                                    //{
                                    //    if (mSymbols != null)
                                    //    {
                                    //        var xMLSymbol = new MLDebugSymbol();
                                    //        xMLSymbol.LabelName = xLabel;
                                    //        int xStackSize = (from item in mAssembler.StackContents
                                    //                          let xSize = (item.Size % 4 == 0)
                                    //                                          ? item.Size
                                    //                                          : (item.Size + (4 - (item.Size % 4)))
                                    //                          select xSize).Sum();
                                    //        xMLSymbol.StackDifference = xMethodInfo.LocalsSize + xStackSize;
                                    //        try
                                    //        {
                                    //            xMLSymbol.AssemblyFile = xCurrentMethod.DeclaringType.Assembly.Location;
                                    //        }
                                    //        catch (NotSupportedException)
                                    //        {
                                    //            xMLSymbol.AssemblyFile = "DYNAMIC: " + xCurrentMethod.DeclaringType.Assembly.FullName;
                                    //        }
                                    //        xMLSymbol.MethodToken = xCurrentMethod.MetadataToken;
                                    //        xMLSymbol.TypeToken = xCurrentMethod.DeclaringType.MetadataToken;
                                    //        xMLSymbol.ILOffset = (int)xReader.Position;
                                    //        mSymbols.Add(xMLSymbol);
                                    //    }
                                    //}
                                    xOp.SetServiceProvider(this);
                                    xOp.Assemble();
                                }
                            }
                            //if (mSymbols != null && !EMITmode)
                            //{
                            //    MLDebugSymbol[] xSymbols;
                            //    using (mSymbolsLocker.AcquireReaderLock())
                            //    {
                            //        xSymbols = mSymbols.ToArray();
                            //    }
                            //    using (mMethodsLocker.AcquireReaderLock())
                            //    {
                            //        mMethods[xCurrentMethod].Instructions = xSymbols;
                            //    }
                            //}
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
                xOp = GetOpFromType(OpCodeMap.MethodFooterOp, null, xMethodInfo);
                xOp.Assembler = Assembler;
                xOp.SetServiceProvider(this);
                xOp.Assemble();
                Assembler.StackContents.Clear();
            }
        }

        private static Op GetOpFromType(Type aType, ILReader aReader, MethodInformation aMethodInfo)
        {
            return (Op)Activator.CreateInstance(aType, aReader, aMethodInfo);
        }


        private IDictionary<string, MethodBase> mPlugMethods;
        private IDictionary<Type, Dictionary<string, PlugFieldAttribute>> mPlugFields;

        private Assembly CurrentDomain_AssemblyResolve(object sender,
                                                      ResolveEventArgs args)
        {
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                         args.Name + ".dll")))
            {
                return Assembly.ReflectionOnlyLoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                    args.Name + ".dll"));
            }
            return null;
        }

        private void CurrentDomain_AssemblyLoad(object sender,
                                                AssemblyLoadEventArgs args)
        {
            CheckAssemblyForPlugAssemblies(args.LoadedAssembly);
        }

        private static string GetStrippedMethodBaseFullName(MethodBase aMethod,
                                                    MethodBase aRefMethod)
        {
            StringBuilder xBuilder = new StringBuilder();
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
            return xBuilder.ToString();
        }


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
                            if (!mPlugMethods.ContainsKey(Label.GenerateLabelName(xOrigMethodDef)))
                            {
                                mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef),
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
                            if (mPlugMethods.ContainsKey(Label.GenerateLabelName(xOrigMethodDef)))
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef),
                                             xMethod);
                        }
                    }
                }
            }
        }


        private void CheckAssemblyForPlugAssemblies(Assembly aAssembly)
        {
            //If in the GAC, then ignore assembly
            if (aAssembly.GlobalAssemblyCache)
            {
                return;
            }

            // todo: try to get rid of the try..catch. find a way to detect dynamic assemblies.
            try
            {
                //Search for related .config file
                string configFile = aAssembly.Location + ".cosmos-config";
                if (System.IO.File.Exists(configFile))
                {
                    //Load and parse all PlugAssemblies referred to in the .config file
                    foreach (Assembly xAssembly in GetAssembliesFromConfigFile(configFile))
                    {
                        LoadPlugAssembly(xAssembly);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Retrieves a list of plug assemblies from the given .config file.
        /// </summary>
        /// <param name="configFile"></param>
        private IEnumerable<Assembly> GetAssembliesFromConfigFile(string configFile)
        {
            //Parse XML and get all the PlugAssembly names
            XmlDocument xml = new XmlDocument();
            xml.Load(configFile);
            // do version check:
            if (xml.DocumentElement.Attributes["version"] == null || xml.DocumentElement.Attributes["version"].Value != "1")
            {
                throw new Exception(".DLL configuration version mismatch!");
            }

            string xHintPath = null;
            if (xml.DocumentElement.Attributes["hintpath"] != null)
            {
                xHintPath = xml.DocumentElement.Attributes["hintpath"].Value;
            }
            foreach (XmlNode assemblyName in xml.GetElementsByTagName("plug-assembly"))
            {
                string xName = assemblyName.InnerText;
                if (xName.EndsWith(".dll",
                                   StringComparison.InvariantCultureIgnoreCase) || xName.EndsWith(".exe",
                                                                                                  StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!String.IsNullOrEmpty(xHintPath))
                    {
                        yield return Assembly.LoadFile(Path.Combine(xHintPath,
                                                                    xName));
                        continue;
                    }
                }
                yield return Assembly.Load(assemblyName.InnerText);
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

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
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
            var xResult = OpCodeMap.GetPlugAssemblies();
            xResult.Add(typeof(AssemblyCompiler).Assembly);
            return xResult;
        }

        public void LogMessage(LogSeverityEnum aSeverity, string aMessage, params object[] aArgs)
        {
            DebugLog(aSeverity, String.Format(aMessage, aArgs));
        }

        

        private void EnsureCanExecute()
        {
            if (Assembly == null)
            {
                throw new Exception("Cannot execute because no Assembly is specified");
            }
            if (Assembler == null)
            {
                throw new Exception("Cannot execute because no Assembler is specified");
            }
            if (OpCodeMap == null)
            {
                throw new Exception("Cannot execute because no OpCodeMap is specified");
            }
            if (IsEntrypointAssembly)
            {
                if (AssemblyReferences.Count ==0)
                {
                    throw new Exception("EntrypointAssemblies should have their references specified!");
                }
            }
        }

        public readonly bool RunningOnMono;

        public uint GetFieldStorageSize(Type aType)
        {
            if (aType.FullName == "System.Void")
            {
                return 0;
            }
            if ((!aType.IsValueType && aType.IsClass) || aType.IsInterface)
            {
                return 4;
            }
            switch (aType.FullName)
            {
                case "System.Char":
                    return 2;
                case "System.Byte":
                case "System.SByte":
                    return 1;
                case "System.UInt16":
                case "System.Int16":
                    return 2;
                case "System.UInt32":
                case "System.Int32":
                    return 4;
                case "System.UInt64":
                case "System.Int64":
                    return 8;
                // for now hardcode IntPtr and UIntPtr to be 32-bit
                case "System.UIntPtr":
                case "System.IntPtr":
                    return 4;
                case "System.Boolean":
                    return 1;
                case "System.Single":
                    return 4;
                case "System.Double":
                    return 8;
                case "System.Decimal":
                    return 16;
                case "System.Guid":
                    return 16;
                case "System.DateTime":
                    return 8; // todo: check for correct size
            }
            if (aType.FullName != null && aType.FullName.EndsWith("*"))
            {
                // pointer
                return 4;
            }
            // array
            //TypeSpecification xTypeSpec = aType as TypeSpecification;
            //if (xTypeSpec != null) {
            //    return 4;
            //}
            if (aType.IsEnum)
            {
                return GetFieldStorageSize(aType.GetField("value__").FieldType);
            }
            if (aType.IsValueType)
            {
                StructLayoutAttribute xSLA = aType.StructLayoutAttribute;
                if (xSLA != null)
                {
                    if (xSLA.Size > 0)
                    {
                        return (uint)xSLA.Size;
                    }
                }
            }
            uint xResult;
            GetTypeFieldInfo(aType,
                             out xResult);
            return xResult;
        }

        public TraceAssemblies TraceAssemblies;

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

        protected void EmitTracer(Op aOp, string aNamespace, int aPos, int[] aCodeOffsets, string aLabel)
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
            OpCodeMap.EmitOpDebugHeader(Assembler, 0, aLabel);
        }
    }
}
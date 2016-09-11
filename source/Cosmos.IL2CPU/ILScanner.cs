using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs;
using SR = System.Reflection;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU
{
    public delegate void LogExceptionDelegate(Exception e);

    public class ScannerQueueItem
    {
        public _MemberInfo Item;
        public string SourceItem;
        public string QueueReason;

        public override string ToString()
        {
            return Item.MemberType + " " + Item.ToString();
        }
    }

    public class ILScanner : IDisposable
    {
        public LogExceptionDelegate LogException = null;
        public Action<string> LogWarning = null;

        protected ILReader mReader;
        protected AppAssembler mAsmblr;

        // List of asssemblies found during scan. We cannot use the list of loaded
        // assemblies because the loaded list includes compilers, etc, and also possibly
        // other unused assemblies. So instead we collect a list of assemblies as we scan.
        internal List<Assembly> mUsedAssemblies = new List<Assembly>();

        protected OurHashSet<_MemberInfo> mItems = new OurHashSet<_MemberInfo>();
        protected List<object> mItemsList = new List<object>();
        // Contains items to be scanned, both types and methods
        protected Queue<ScannerQueueItem> mQueue = new Queue<ScannerQueueItem>();
        // Virtual methods are nasty and constantly need to be rescanned for
        // overriding methods in new types, so we keep track of them separately.
        // They are also in the main mItems and mQueue.
        protected HashSet<MethodBase> mVirtuals = new HashSet<MethodBase>();

        protected IDictionary<MethodBase, uint> mMethodUIDs = new Dictionary<MethodBase, uint>();
        protected IDictionary<Type, uint> mTypeUIDs = new Dictionary<Type, uint>();

        protected PlugManager mPlugManager = null;

        // Logging
        // Only use for debugging and profiling.
        protected bool mLogEnabled = false;
        protected string mMapPathname;
        protected TextWriter mLogWriter;
        protected struct LogItem
        {
            public string SrcType;
            public object Item;
        }
        protected Dictionary<object, List<LogItem>> mLogMap;

        public ILScanner(AppAssembler aAsmblr)
        {
            mAsmblr = aAsmblr;
            mReader = new ILReader();

            mPlugManager = new PlugManager(LogException, LogWarning);
        }

        public bool EnableLogging(string aPathname)
        {
            mLogMap = new Dictionary<object, List<LogItem>>();
            mMapPathname = aPathname;
            mLogEnabled = true;

            // be sure that file could be written, to prevent exception on Dispose call, cause we could not make Task log in it
            try
            {
                File.CreateText(aPathname).Dispose();
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected void Queue(_MemberInfo aItem, object aSrc, string aSrcType, string sourceItem = null)
        {
            if (aItem == null)
            {
                throw new ArgumentNullException(nameof(aItem));
            }

            var xMemInfo = aItem as MemberInfo;
            //TODO: fix this, as each label/symbol should also contain an assembly specifier.

            if ((xMemInfo != null) && (xMemInfo.DeclaringType != null)
                && (xMemInfo.DeclaringType.FullName == "System.ThrowHelper")
                && (xMemInfo.DeclaringType.Assembly.GetName().Name != "mscorlib"))
            {
                // System.ThrowHelper exists in MS .NET twice...
                // Its an internal class that exists in both mscorlib and system assemblies.
                // They are separate types though, so normally the scanner scans both and
                // then we get conflicting labels. MS included it twice to make exception
                // throwing code smaller. They are internal though, so we cannot
                // reference them directly and only via finding them as they come along.
                // We find it here, not via QueueType so we only check it here. Later
                // we might have to checkin QueueType also.
                // So now we accept both types, but emit code for only one. This works
                // with the current Nasm assembler as we resolve by name in the assembler.
                // However with other assemblers this approach may not work.
                // If AssemblerNASM adds assembly name to the label, this will allow
                // both to exist as they do in BCL.
                // So in the future we might be able to remove this hack, or change
                // how it works.
                //
                // Do nothing
                //
            }
            else if (!mItems.Contains(aItem))
            {
                if (mLogEnabled)
                {
                    LogMapPoint(aSrc, aSrcType, aItem);
                }
                mItems.Add(aItem);
                mItemsList.Add(aItem);

                MethodBase methodBaseSource = aSrc as MethodBase;
                if (methodBaseSource != null)
                {
                    aSrc = methodBaseSource.DeclaringType + "::" + aSrc;
                }

                mQueue.Enqueue(new ScannerQueueItem { Item = aItem, QueueReason = aSrcType, SourceItem = aSrc + Environment.NewLine + sourceItem });
            }
        }

        public void Execute(MethodBase aStartMethod)
        {
            if (aStartMethod == null)
            {
                throw new ArgumentNullException("aStartMethod");
            }
            // TODO: Investigate using MS CCI
            // Need to check license, as well as in profiler
            // http://cciast.codeplex.com/

            #region Description
            // Methodology
            //
            // Ok - we've done the scanner enough times to know it needs to be
            // documented super well so that future changes won't inadvertently
            // break undocumented and unseen requirements.
            //
            // We've tried many approaches including recursive and additive scanning.
            // They typically end up being inefficient, overly complex, or both.
            //
            // -We would like to scan all types/methods so we can plug them.
            // -But we can't scan them until we plug them, because we will scan things
            // that plugs would remove/change the paths of.
            // -Plugs may also call methods which are also plugged.
            // -We cannot resolve plugs ahead of time but must do on the fly during
            // scanning.
            // -TODO: Because we do on the fly resolution, we need to add explicit
            // checking of plug classes and err when public methods are found that
            // do not resolve. Maybe we can make a list and mark, or rescan. Can be done
            // later or as an optional auditing step.
            //
            // This why in the past we had repetitive scans.
            //
            // Now we focus on more passes, but simpler execution. In the end it should
            // be eaiser to optmize and yield overall better performance. Most of the
            // passes should be low overhead versus an integrated system which often
            // would need to reiterate over items multiple times. So we do more loops on
            // with less repetitive analysis, instead of fewer loops but more repetition.
            //
            // -Locate all plug classes
            // -Scan from entry point collecting all types and methods while checking
            // for and following plugs
            // -For each type
            //    -Include all ancestors
            //    -Include all static constructors
            // -For each virtual method
            //    -Scan overloads in descendants until IsFinal, IsSealed or end
            //    -Scan base in ancestors until top or IsAbstract
            // -Go to scan types again, until no new ones found.
            // -Because the virtual method scanning will add to the list as it goes, maintain
            //  2 lists.
            //    -Known Types and Methods
            //    -Types and Methods in Queue - to be scanned
            // -Finally, do compilation
            #endregion

            mPlugManager.FindPlugImpls();
            // Now that we found all plugs, scan them.
            // We have to scan them after we find all plugs, because
            // plugs can use other plugs
            mPlugManager.ScanFoundPlugs();
            foreach (var xPlug in mPlugManager.PlugImpls)
            {
                CompilerHelpers.Debug($"Plug found: '{xPlug.Key.FullName}'");
            }

            ILOp.mPlugManager = mPlugManager;

            // Pull in extra implementations, GC etc.
            Queue(RuntimeEngineRefs.InitializeApplicationRef, null, "Explicit Entry");
            Queue(RuntimeEngineRefs.FinalizeApplicationRef, null, "Explicit Entry");
            //Queue(typeof(CosmosAssembler).GetMethod("PrintException"), null, "Explicit Entry");
            Queue(VTablesImplRefs.SetMethodInfoRef, null, "Explicit Entry");
            Queue(VTablesImplRefs.IsInstanceRef, null, "Explicit Entry");
            Queue(VTablesImplRefs.SetTypeInfoRef, null, "Explicit Entry");
            Queue(VTablesImplRefs.GetMethodAddressForTypeRef, null, "Explicit Entry");
            Queue(GCImplementationRefs.IncRefCountRef, null, "Explicit Entry");
            Queue(GCImplementationRefs.DecRefCountRef, null, "Explicit Entry");
            Queue(GCImplementationRefs.AllocNewObjectRef, null, "Explicit Entry");
            // for now, to ease runtime exception throwing
            Queue(typeof(ExceptionHelper).GetMethod("ThrowNotImplemented", BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof(string)}, null), null, "Explicit Entry");
            Queue(typeof(ExceptionHelper).GetMethod("ThrowOverflow", BindingFlags.Static | BindingFlags.Public, null, new Type[] {}, null), null, "Explicit Entry");
            Queue(RuntimeEngineRefs.InitializeApplicationRef, null, "Explicit Entry");
            Queue(RuntimeEngineRefs.FinalizeApplicationRef, null, "Explicit Entry");
            // register system types:
            Queue(typeof(Array), null, "Explicit Entry");
            Queue(typeof(Array).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null), null, "Explicit Entry");

            var xThrowHelper = Type.GetType("System.ThrowHelper", true);
            Queue(xThrowHelper.GetMethod("ThrowInvalidOperationException", BindingFlags.NonPublic | BindingFlags.Static), null, "Explicit Entry");

            Queue(typeof(MulticastDelegate).GetMethod("GetInvocationList"), null, "Explicit Entry");
            Queue(ExceptionHelperRefs.CurrentExceptionRef, null, "Explicit Entry");
            //System_Delegate____System_MulticastDelegate_GetInvocationList__

            // Start from entry point of this program
            Queue(aStartMethod, null, "Entry Point");

            ScanQueue();
            UpdateAssemblies();
            Assemble();

            mAsmblr.EmitEntrypoint(aStartMethod);
        }

        public void QueueMethod(MethodBase method)
        {
            Queue(method, null, "Explicit entry via QueueMethod");
        }

        /// This method changes the opcodes. Changes are:
        /// * inserting the ValueUID for method ops.
        private void ProcessInstructions(List<ILOpCode> aOpCodes)
        {
            foreach (var xOpCode in aOpCodes)
            {
                var xOpMethod = xOpCode as ILOpCodes.OpMethod;
                if (xOpMethod != null)
                {
                    xOpMethod.Value = (MethodBase) mItems.GetItemInList(xOpMethod.Value);
                    xOpMethod.ValueUID = (uint)GetMethodUID(xOpMethod.Value, true);
                    xOpMethod.BaseMethodUID = GetMethodUID(xOpMethod.Value, false);
                }
            }
        }

        public void Dispose()
        {
            if (mLogEnabled)
            {
                // Create bookmarks, but also a dictionary that
                // we can find the items in
                var xBookmarks = new Dictionary<object, int>();
                int xBookmark = 0;
                foreach (var xList in mLogMap)
                {
                    foreach (var xItem in xList.Value)
                    {
                        xBookmarks.Add(xItem.Item, xBookmark);
                        xBookmark++;
                    }
                }

                using (mLogWriter = new StreamWriter(mMapPathname, false))
                {
                    mLogWriter.WriteLine("<html><body>");
                    foreach (var xList in mLogMap)
                    {
                        var xLogItemText = LogItemText(xList.Key);

                        mLogWriter.WriteLine("<hr>");

                        // Emit bookmarks above source, so when clicking links user doesn't need
                        // to constantly scroll up.
                        foreach (var xItem in xList.Value)
                        {
                            mLogWriter.WriteLine("<a name=\"Item" + xBookmarks[xItem.Item].ToString() + "_S\"></a>");
                        }

                        int xHref;
                        if (!xBookmarks.TryGetValue(xList.Key, out xHref))
                        {
                            xHref = -1;
                        }
                        mLogWriter.Write("<p>");
                        if (xHref >= 0)
                        {
                            mLogWriter.WriteLine("<a href=\"#Item" + xHref.ToString() + "_S\">");
                            mLogWriter.WriteLine("<a name=\"Item{0}\">", xHref);
                        }
                        if (xList.Key == null)
                        {
                            mLogWriter.WriteLine("Unspecified Source");
                        }
                        else
                        {
                            mLogWriter.WriteLine(xLogItemText);
                        }
                        if (xHref >= 0)
                        {
                            mLogWriter.Write("</a>");
                            mLogWriter.Write("</a>");
                        }
                        mLogWriter.WriteLine("</p>");

                        mLogWriter.WriteLine("<ul>");
                        foreach (var xItem in xList.Value)
                        {
                            mLogWriter.Write("<li><a href=\"#Item{1}\">{0}</a></li>", LogItemText(xItem.Item), xBookmarks[xItem.Item]);

                            mLogWriter.WriteLine("<ul>");
                            mLogWriter.WriteLine("<li>" + xItem.SrcType + "</li>");
                            mLogWriter.WriteLine("</ul>");
                        }
                        mLogWriter.WriteLine("</ul>");
                    }
                    mLogWriter.WriteLine("</body></html>");
                }
            }
        }

        public int MethodCount
        {
            get
            {
                return mMethodUIDs.Count;
            }
        }

        protected string LogItemText(object aItem)
        {
            if (aItem is MethodBase)
            {
                var x = (MethodBase) aItem;
                return "Method: " + x.DeclaringType + "." + x.Name + "<br>" + x.GetFullName();
            }
            if (aItem is Type)
            {
                var x = (Type) aItem;
                return "Type: " + x.FullName;
            }
            return "Other: " + aItem;
        }

        protected void ScanMethod(MethodBase aMethod, bool aIsPlug, string sourceItem)
        {
            var xParams = aMethod.GetParameters();
            var xParamTypes = new Type[xParams.Length];
            // Dont use foreach, enum generaly keeps order but
            // isn't guaranteed.
            //string xMethodFullName = LabelName.GenerateFullName(aMethod);

            for (int i = 0; i < xParams.Length; i++)
            {
                xParamTypes[i] = xParams[i].ParameterType;
                Queue(xParamTypes[i], aMethod, "Parameter");
            }
            var xIsDynamicMethod = aMethod.DeclaringType == null;
            // Queue Types directly related to method
            if (!aIsPlug)
            {
                // Don't queue declaring types of plugs
                if (!xIsDynamicMethod)
                {
                    // dont queue declaring types of dynamic methods either, those dont have a declaring type
                    Queue(aMethod.DeclaringType, aMethod, "Declaring Type");
                }
            }
            if (aMethod is SysReflection.MethodInfo)
            {
                Queue(((SysReflection.MethodInfo) aMethod).ReturnType, aMethod, "Return Type");
            }
            if (aMethod.GetFullName().IndexOf("CreateComparer", StringComparison.OrdinalIgnoreCase)!=-1)
            {
                ;
            }
            // Scan virtuals
            #region Virtuals scan
            if (!xIsDynamicMethod && aMethod.IsVirtual)
            {
                // For virtuals we need to climb up the type tree
                // and find the top base method. We then add that top
                // node to the mVirtuals list. We don't need to add the
                // types becuase adding DeclaringType will already cause
                // all ancestor types to be added.

                var xVirtMethod = aMethod;
                var xVirtType = aMethod.DeclaringType;
                MethodBase xNewVirtMethod;
                while (true)
                {
                    xVirtType = xVirtType.BaseType;
                    if (xVirtType == null)
                    {
                        // We've reached object, can't go farther
                        xNewVirtMethod = null;
                    }
                    else
                    {
                        xNewVirtMethod = xVirtType.GetMethod(aMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, xParamTypes, null);
                        if (xNewVirtMethod != null)
                        {
                            if (!xNewVirtMethod.IsVirtual)
                            {
                                // This can happen if a virtual "replaces" a non virtual
                                // above it that is not virtual.
                                xNewVirtMethod = null;
                            }
                        }
                    }
                    // We dont bother to add these to Queue, because we have to do a
                    // full downlevel scan if its a new base virtual anyways.
                    if (xNewVirtMethod == null)
                    {
                        // If its already in the list, we mark it null
                        // so we dont do a full downlevel scan.
                        if (mVirtuals.Contains(xVirtMethod))
                        {
                            xVirtMethod = null;
                        }
                        break;
                    }
                    xVirtMethod = xNewVirtMethod;
                }

                // New virtual base found, we need to downscan it
                // If it was already in mVirtuals, then ScanType will take
                // care of new additions.
                if (xVirtMethod != null)
                {
                    Queue(xVirtMethod, aMethod, "Virtual Base");
                    mVirtuals.Add(xVirtMethod);

                    // List changes as we go, cant be foreach
                    for (int i = 0; i < mItemsList.Count; i++)
                    {
                        if (mItemsList[i] is Type)
                        {
                            var xType = (Type) mItemsList[i];
                            if (xType.IsSubclassOf(xVirtMethod.DeclaringType) || (xVirtMethod.DeclaringType.IsInterface && xVirtMethod.DeclaringType.IsAssignableFrom(xType)))
                            {
                                var xNewMethod = xType.GetMethod(aMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, xParamTypes, null);
                                if (xNewMethod != null)
                                {
                                    // We need to check IsVirtual, a non virtual could
                                    // "replace" a virtual above it?
                                    if (xNewMethod.IsVirtual)
                                    {
                                        Queue(xNewMethod, aMethod, "Virtual Downscan");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            MethodBase xPlug = null;
            // Plugs may use plugs, but plugs won't be plugged over themself
            var inl = aMethod.GetCustomAttribute<InlineAttribute>();
            if (!aIsPlug && !xIsDynamicMethod)
            {
                // Check to see if method is plugged, if it is we don't scan body

                xPlug = mPlugManager.ResolvePlug(aMethod, xParamTypes);
                if (xPlug != null)
                {
                    //ScanMethod(xPlug, true, "Plug method");
                    if (inl == null)
                    {
                        Queue(xPlug, aMethod, "Plug method");
                    }
                }
            }

            if (xPlug == null)
            {
                bool xNeedsPlug = false;
                if ((aMethod.Attributes & MethodAttributes.PinvokeImpl) != 0)
                {
                    // pinvoke methods dont have an embedded implementation
                    xNeedsPlug = true;
                }
                else
                {
                    var xImplFlags = aMethod.GetMethodImplementationFlags();
                    // todo: prob even more
                    if (((xImplFlags & MethodImplAttributes.Native) != 0) ||
                        ((xImplFlags & MethodImplAttributes.InternalCall) != 0))
                    {
                        // native implementations cannot be compiled
                        xNeedsPlug = true;
                    }
                }
                if (xNeedsPlug)
                {
                    throw new Exception("Native code encountered, plug required. Please see https://github.com/CosmosOS/Cosmos/wiki/Plugs). " + LabelName.GenerateFullName(aMethod) + "." + Environment.NewLine + " Called from :" + Environment.NewLine + sourceItem);
                }

                //TODO: As we scan each method, we could update or put in a new list
                // that has the resolved plug so we don't have to reresolve it again
                // later for compilation.

                // Scan the method body for more type and method refs
                //TODO: Dont queue new items if they are plugged
                // or do we need to queue them with a resolved ref in a new list?

                if (inl != null)
                {
                    return; // cancel inline
                }

                List<ILOpCode> xOpCodes;
                xOpCodes = mReader.ProcessMethod(aMethod);
                if (xOpCodes != null)
                {
                    ProcessInstructions(xOpCodes);
                    foreach (var xOpCode in xOpCodes)
                    {
                        if (xOpCode is ILOpCodes.OpMethod)
                        {
                            Queue(((ILOpCodes.OpMethod) xOpCode).Value, aMethod, "Call", sourceItem);
                        }
                        else if (xOpCode is ILOpCodes.OpType)
                        {
                            Queue(((ILOpCodes.OpType) xOpCode).Value, aMethod, "OpCode Value");
                        }
                        else if (xOpCode is ILOpCodes.OpField)
                        {
                            var xOpField = (ILOpCodes.OpField) xOpCode;
                            //TODO: Need to do this? Will we get a ILOpCodes.OpType as well?
                            Queue(xOpField.Value.DeclaringType, aMethod, "OpCode Value");
                            if (xOpField.Value.IsStatic)
                            {
                                //TODO: Why do we add static fields, but not instance?
                                // AW: instance fields are "added" always, as part of a type, but for static fields, we need to emit a datamember
                                Queue(xOpField.Value, aMethod, "OpCode Value");
                            }
                        }
                        else if (xOpCode is ILOpCodes.OpToken)
                        {
                            var xTokenOp = (ILOpCodes.OpToken) xOpCode;
                            if (xTokenOp.ValueIsType)
                            {
                                Queue(xTokenOp.ValueType, aMethod, "OpCode Value");
                            }
                            if (xTokenOp.ValueIsField)
                            {
                                Queue(xTokenOp.ValueField.DeclaringType, aMethod, "OpCode Value");
                                if (xTokenOp.ValueField.IsStatic)
                                {
                                    //TODO: Why do we add static fields, but not instance?
                                    // AW: instance fields are "added" always, as part of a type, but for static fields, we need to emit a datamember
                                    Queue(xTokenOp.ValueField, aMethod, "OpCode Value");
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void ScanType(Type aType)
        {
            // Add immediate ancestor type
            // We dont need to crawl up farther, when the BaseType is scanned
            // it will add its BaseType, and so on.
            if (aType.BaseType != null)
            {
                Queue(aType.BaseType, aType, "Base Type");
            }
            // Queue static ctors
            // We always need static ctors, else the type cannot
            // be created.
            foreach (var xCctor in aType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (xCctor.DeclaringType == aType)
                {
                    Queue(xCctor, aType, "Static Constructor");
                }
            }

            // For each new type, we need to scan for possible new virtuals
            // in our new type if its a descendant of something in
            // mVirtuals.
            foreach (var xVirt in mVirtuals)
            {
                // See if our new type is a subclass of any virt's DeclaringTypes
                // If so our new type might have some virtuals
                if (aType.IsSubclassOf(xVirt.DeclaringType))
                {
                    var xParams = xVirt.GetParameters();
                    var xParamTypes = new Type[xParams.Length];
                    // Dont use foreach, enum generaly keeps order but
                    // isn't guaranteed.
                    for (int i = 0; i < xParams.Length; i++)
                    {
                        xParamTypes[i] = xParams[i].ParameterType;
                    }
                    var xMethod = aType.GetMethod(xVirt.Name, xParamTypes);
                    if (xMethod != null)
                    {
                        // We need to check IsVirtual, a non virtual could
                        // "replace" a virtual above it?
                        if (xMethod.IsVirtual)
                        {
                            Queue(xMethod, aType, "Virtual");
                        }
                    }
                }
                if (!aType.IsGenericParameter && xVirt.DeclaringType.IsInterface)
                {
                    if (!aType.IsInterface && aType.GetInterfaces().Contains(xVirt.DeclaringType))
                    {
                        var xIntfMapping = aType.GetInterfaceMap(xVirt.DeclaringType);
                        if ((xIntfMapping.InterfaceMethods != null) && (xIntfMapping.TargetMethods != null))
                        {
                            var xIdx = Array.IndexOf(xIntfMapping.InterfaceMethods, xVirt);
                            if (xIdx != -1)
                            {
                                Queue(xIntfMapping.TargetMethods[xIdx], aType, "Virtual");
                            }
                        }
                    }
                }
            }
        }

        protected void ScanQueue()
        {
            while (mQueue.Count > 0)
            {
                var xItem = mQueue.Dequeue();
                // Check for MethodBase first, they are more numerous
                // and will reduce compares
                if (xItem.Item is MethodBase)
                {
                    ScanMethod((MethodBase) xItem.Item, false, xItem.SourceItem);
                }
                else if (xItem.Item is Type)
                {
                    var xType = (Type) xItem.Item;
                    ScanType(xType);

                    // Methods and fields cant exist without types, so we only update
                    // mUsedAssemblies in type branch.
                    if (!mUsedAssemblies.Contains(xType.Assembly))
                    {
                        mUsedAssemblies.Add(xType.Assembly);
                    }
                }
                else if (xItem.Item is FieldInfo)
                {
                    // todo: static fields need more processing?
                }
                else
                {
                    throw new Exception("Unknown item found in queue.");
                }
            }
        }

        protected void LogMapPoint(object aSrc, string aSrcType, object aItem)
        {
            // Keys cant be null. If null, we just say ILScanner is the source
            if (aSrc == null)
            {
                aSrc = typeof(ILScanner);
            }

            var xLogItem = new LogItem
            {
                SrcType = aSrcType,
                Item = aItem
            };
            List<LogItem> xList;
            if (!mLogMap.TryGetValue(aSrc, out xList))
            {
                xList = new List<LogItem>();
                mLogMap.Add(aSrc, xList);
            }
            xList.Add(xLogItem);
        }

        private MethodBase GetUltimateBaseMethod(MethodBase aMethod, Type[] aMethodParams, Type aCurrentInspectedType)
        {
            MethodBase xBaseMethod = null;
            //try {
            while (true)
            {
                if (aCurrentInspectedType.BaseType == null)
                {
                    break;
                }
                aCurrentInspectedType = aCurrentInspectedType.BaseType;
                MethodBase xFoundMethod = aCurrentInspectedType.GetMethod(aMethod.Name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    Type.DefaultBinder,
                    aMethodParams,
                    new ParameterModifier[0]);
                if (xFoundMethod == null)
                {
                    break;
                }
                ParameterInfo[] xParams = xFoundMethod.GetParameters();
                bool xContinue = true;
                for (int i = 0; i < xParams.Length; i++)
                {
                    if (xParams[i].ParameterType != aMethodParams[i])
                    {
                        xContinue = false;
                    }
                }
                if (!xContinue)
                {
                    continue;
                }
                if (xFoundMethod != null)
                {
                    xBaseMethod = xFoundMethod;

                    if ((xFoundMethod.IsVirtual == aMethod.IsVirtual) && (xFoundMethod.IsPrivate == false) && (xFoundMethod.IsPublic == aMethod.IsPublic) && (xFoundMethod.IsFamily == aMethod.IsFamily) && (xFoundMethod.IsFamilyAndAssembly == aMethod.IsFamilyAndAssembly) && (xFoundMethod.IsFamilyOrAssembly == aMethod.IsFamilyOrAssembly) && (xFoundMethod.IsFinal == false))
                    {
                        var xFoundMethInfo = xFoundMethod as SR.MethodInfo;
                        var xBaseMethInfo = xBaseMethod as SR.MethodInfo;
                        if ((xFoundMethInfo == null) && (xBaseMethInfo == null))
                        {
                            xBaseMethod = xFoundMethod;
                        }
                        if ((xFoundMethInfo != null) && (xBaseMethInfo != null))
                        {
                            if (xFoundMethInfo.ReturnType.AssemblyQualifiedName.Equals(xBaseMethInfo.ReturnType.AssemblyQualifiedName))
                            {
                                xBaseMethod = xFoundMethod;
                            }
                        }
                        //xBaseMethod = xFoundMethod;
                    }
                }
                //else
                //{
                //    xBaseMethod = xFoundMethod;
                //}
            }
            //} catch (Exception) {
            // todo: try to get rid of the try..catch
            //}
            return xBaseMethod ?? aMethod;
        }

        protected uint GetMethodUID(MethodBase aMethod, bool aExact)
        {
            if (!aExact)
            {
                ParameterInfo[] xParams = aMethod.GetParameters();
                Type[] xParamTypes = new Type[xParams.Length];
                for (int i = 0; i < xParams.Length; i++)
                {
                    xParamTypes[i] = xParams[i].ParameterType;
                }
                var xBaseMethod = GetUltimateBaseMethod(aMethod, xParamTypes, aMethod.DeclaringType);
                if (!mMethodUIDs.ContainsKey(xBaseMethod))
                {
                    var xId = (uint) mMethodUIDs.Count;
                    mMethodUIDs.Add(xBaseMethod, xId);
                }
                return mMethodUIDs[xBaseMethod];
            }
            if (!mMethodUIDs.ContainsKey(aMethod))
            {
                var xId = (uint) mMethodUIDs.Count;
                mMethodUIDs.Add(aMethod, xId);
            }
            return mMethodUIDs[aMethod];
        }

        protected uint GetTypeUID(Type aType)
        {
            if (!mItems.Contains(aType))
            {
                throw new Exception("Cannot get UID of types which are not queued!");
            }
            if (!mTypeUIDs.ContainsKey(aType))
            {
                var xId = (uint) mTypeUIDs.Count;
                mTypeUIDs.Add(aType, xId);
                return xId;
            }
            return mTypeUIDs[aType];
        }

        protected void UpdateAssemblies()
        {
            // It would be nice to keep DebugInfo output into assembler only but
            // there is so much info that is available in scanner that is needed
            // or can be used in a more efficient manner. So we output in both
            // scanner and assembler as needed.
            mAsmblr.DebugInfo.AddAssemblies(mUsedAssemblies);
        }

        protected void Assemble()
        {
            foreach (var xItem in mItems)
            {
                if (xItem is MethodBase)
                {
                        var xMethod = (MethodBase) xItem;
                        var xParams = xMethod.GetParameters();
                        var xParamTypes = xParams.Select(q => q.ParameterType).ToArray();
                        var xPlug = mPlugManager.ResolvePlug(xMethod, xParamTypes);
                        var xMethodType = MethodInfo.TypeEnum.Normal;
                        Type xPlugAssembler = null;
                        MethodInfo xPlugInfo = null;
                        var xMethodInline = xMethod.GetCustomAttribute<InlineAttribute>();
                        if (xMethodInline != null)
                        {
                            // inline assembler, shouldn't come here..
                            continue;
                        }
                        var xMethodIdMethod = mItemsList.IndexOf(xMethod);
                        if (xMethodIdMethod == -1)
                        {
                            throw new Exception("Method not in scanner list!");
                        }
                        if (xPlug != null)
                        {
                            xMethodType = MethodInfo.TypeEnum.NeedsPlug;
                            var xAttrib = xPlug.GetCustomAttribute<PlugMethodAttribute>();
                            var xInline = xPlug.GetCustomAttribute<InlineAttribute>();
                            var xMethodIdPlug = mItemsList.IndexOf(xPlug);
                            if ((xMethodIdPlug == -1) && (xInline == null))
                            {
                                throw new Exception("Plug method not in scanner list!");
                            }
                            if ((xAttrib != null) && (xInline == null))
                            {
                                xPlugAssembler = xAttrib.Assembler;
                                xPlugInfo = new MethodInfo(xPlug, (uint) xMethodIdPlug, MethodInfo.TypeEnum.Plug, null, xPlugAssembler);

                                var xMethodInfo = new MethodInfo(xMethod, (uint) xMethodIdMethod, xMethodType, xPlugInfo);
                                if (xAttrib != null && xAttrib.IsWildcard)
                                {
                                    xPlugInfo.IsWildcard = true;
                                    xPlugInfo.PluggedMethod = xMethodInfo;
                                    var xInstructions = mReader.ProcessMethod(xPlug);
                                    if (xInstructions != null)
                                    {
                                        ProcessInstructions(xInstructions);
                                        mAsmblr.ProcessMethod(xPlugInfo, xInstructions);
                                    }
                                }
                                mAsmblr.GenerateMethodForward(xMethodInfo, xPlugInfo);
                            }
                            else
                            {
                                if (xInline != null)
                                {
                                    var xMethodID = mItemsList.IndexOf(xItem);
                                    if (xMethodID == -1)
                                    {
                                        throw new Exception("Method not in list!");
                                    }
                                    xPlugInfo = new MethodInfo(xPlug, (uint) xMethodID, MethodInfo.TypeEnum.Plug, null, true);

                                    var xMethodInfo = new MethodInfo(xMethod, (uint) xMethodIdMethod, xMethodType, xPlugInfo);

                                    xPlugInfo.PluggedMethod = xMethodInfo;
                                    var xInstructions = mReader.ProcessMethod(xPlug);
                                    if (xInstructions != null)
                                    {
                                        ProcessInstructions(xInstructions);
                                        mAsmblr.ProcessMethod(xPlugInfo, xInstructions);
                                    }
                                    mAsmblr.GenerateMethodForward(xMethodInfo, xPlugInfo);
                                }
                                else
                                {
                                    xPlugInfo = new MethodInfo(xPlug, (uint) xMethodIdPlug, MethodInfo.TypeEnum.Plug, null, xPlugAssembler);

                                    var xMethodInfo = new MethodInfo(xMethod, (uint) xMethodIdMethod, xMethodType, xPlugInfo);
                                    mAsmblr.GenerateMethodForward(xMethodInfo, xPlugInfo);
                                }
                            }
                        }
                        else
                        {
                            PlugMethodAttribute xAttrib = null;
                            foreach (PlugMethodAttribute attrib in xMethod.GetCustomAttributes(typeof(PlugMethodAttribute), true))
                            {
                                xAttrib = attrib;
                            }
                            if (xAttrib != null)
                            {
                                if (xAttrib.IsWildcard)
                                {
                                    continue;
                                }
                                if (xAttrib.PlugRequired)
                                {
                                    throw new Exception(string.Format("Method {0} requires a plug, but none is implemented", xMethod.Name));
                                }
                                xPlugAssembler = xAttrib.Assembler;
                            }

                            var xMethodInfo = new MethodInfo(xMethod, (uint) xMethodIdMethod, xMethodType, xPlugInfo, xPlugAssembler);
                            var xInstructions = mReader.ProcessMethod(xMethod);
                            if (xInstructions != null)
                            {
                                ProcessInstructions(xInstructions);
                                mAsmblr.ProcessMethod(xMethodInfo, xInstructions);
                            }
                        }
                    }
                else if (xItem is FieldInfo)
                {
                        mAsmblr.ProcessField((FieldInfo) xItem);
                }
            }

            var xTypes = new HashSet<Type>();
            var xMethods = new HashSet<MethodBase>();
            foreach (var xItem in mItems)
            {
                if (xItem is MethodBase)
                {
                    xMethods.Add((MethodBase) xItem);
                }
                else if (xItem is Type)
                {
                    xTypes.Add((Type) xItem);
                }
            }
            mAsmblr.GenerateVMTCode(xTypes, xMethods, GetTypeUID, x => GetMethodUID(x, false));
        }
    }
}

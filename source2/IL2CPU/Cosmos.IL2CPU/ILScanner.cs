using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.IL;
using SR = System.Reflection;
using Cosmos.Assembler;
using System.Reflection.Emit;
using _MemberInfo = System.Runtime.InteropServices._MemberInfo;

namespace Cosmos.IL2CPU {
  // This is necessary because HashSet and Dictionary
  // have troubles when different types of objects are stored
  // in them. I dont remember the exact problem, but something
  // with how it compares objects. ie when HashSet<object> is used, this is necessary.
  /*public class HashcodeComparer<T> : IEqualityComparer<T> {
      public bool Equals(T x, T y) {
    return internalEqualsSinceNET40(x, y);// x.GetHashCode() == y.GetHashCode();
      }

  public bool internalEqualsSinceNET40(T left, T right)
  {
    var methodDeclaringType = left.GetType().GetMethod("get_DeclaringType");
    var leftDeclaringType = methodDeclaringType.Invoke(left, null);

    var method = right.GetType().GetMethod("get_DeclaringType");
    var rightDeclaringType = method.Invoke(right, null);

    if (left.ToString() == right.ToString()
      && leftDeclaringType == rightDeclaringType)
    {
      return true;
    }
    return false;
  }

      public int GetHashCode(T obj) {
          return obj.GetHashCode();
      }
  }*/

  public class ScannerQueueItem {
    public _MemberInfo Item;
    public string SourceItem;
    public string QueueReason;

    public override string ToString() {
      return Item.MemberType + " " + Item.ToString();
    }
  }

  public class ILScanner : IDisposable {
    protected ILReader mReader;
    protected AppAssembler mAsmblr;

    // List of asssemblies found during scan. We cannot use the list of loaded
    // assemblies because the loaded list includes compilers, etc, and also possibly
    // other unused assemblies. So instead we collect a list of assemblies as we scan.
    protected List<Assembly> mUsedAssemblies = new List<Assembly>();

    protected OurHashSet<object> mItems = new OurHashSet<object>();
    protected List<object> mItemsList = new List<object>();
    // Contains items to be scanned, both types and methods
    protected Queue<ScannerQueueItem> mQueue = new Queue<ScannerQueueItem>();
    // Virtual methods are nasty and constantly need to be rescanned for
    // overriding methods in new types, so we keep track of them separately. 
    // They are also in the main mItems and mQueue.
    protected HashSet<MethodBase> mVirtuals = new HashSet<MethodBase>();

    protected IDictionary<MethodBase, uint> mMethodUIDs = new Dictionary<MethodBase, uint>();
    protected IDictionary<Type, uint> mTypeUIDs = new Dictionary<Type, uint>();

    // Contains a list of plug implementor classes
    // Key = Target Class
    // Value = List of Implementors. There may be more than one
    protected Dictionary<Type, List<Type>> mPlugImpls = new Dictionary<Type, List<Type>>();
    // List of inheritable plugs. Plugs that start at an ancestor and plug all 
    // descendants. For example, delegates
    protected Dictionary<Type, List<Type>> mPlugImplsInhrt = new Dictionary<Type, List<Type>>();

    // list of field plugs
    protected IDictionary<Type, IDictionary<string, PlugFieldAttribute>> mPlugFields = new Dictionary<Type, IDictionary<string, PlugFieldAttribute>>();

    // Logging
    // Only use for debugging and profiling.
    protected bool mLogEnabled = false;
    protected string mMapPathname;
    protected TextWriter mLogWriter;
    protected struct LogItem {
      public string SrcType;
      public object Item;
    }
    protected Dictionary<object, List<LogItem>> mLogMap;

    //TODO: Look for Field plugs

    public ILScanner(AppAssembler aAsmblr) {
      mAsmblr = aAsmblr;
      mReader = new ILReader();
    }

    public void EnableLogging(string aPathname) {
      mLogMap = new Dictionary<object, List<LogItem>>();
      mMapPathname = aPathname;
      mLogEnabled = true;
    }

    protected void Queue(_MemberInfo aItem, object aSrc, string aSrcType, object sourceItem = null) {
      var xMemInfo = aItem as MemberInfo;
      //TODO: fix this, as each label/symbol should also contain an assembly specifier.

      if (xMemInfo != null && xMemInfo.DeclaringType != null
        && xMemInfo.DeclaringType.FullName == "System.ThrowHelper"
        && xMemInfo.DeclaringType.Assembly.GetName().Name != "mscorlib") {
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
      } else if (!mItems.Contains(aItem)) {
        if (mLogEnabled) {
          LogMapPoint(aSrc, aSrcType, aItem);
        }
        mItems.Add(aItem);
        mItemsList.Add(aItem);

        mQueue.Enqueue(new ScannerQueueItem() { Item = aItem, QueueReason = aSrcType, SourceItem = aSrc + Environment.NewLine + sourceItem });
      }
    }

    protected void ScanPlugs(Dictionary<Type, List<Type>> aPlugs) {
      foreach (var xPlug in aPlugs) {
        var xImpls = xPlug.Value;
        foreach (var xImpl in xImpls) {
          #region PlugMethods scan
          foreach (var xMethod in xImpl.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            PlugMethodAttribute xAttrib = null;
            foreach (PlugMethodAttribute x in xMethod.GetCustomAttributes(typeof(PlugMethodAttribute), false)) {
              xAttrib = x;
            }
            if (xAttrib == null) {
              ScanMethod(xMethod, true, "Plug Sub Method");
            } else {
              if (xAttrib.IsWildcard && xAttrib.Assembler == null) {
                throw new Exception("Wildcard PlugMethods need to use an assembler for now.");
              }
              if (xAttrib.Enabled && !xAttrib.IsMonoOnly) {
                ScanMethod(xMethod, true, ".Net plug Method");
              }
            }
          }
          #endregion
          #region PlugFields scan
          foreach (var xField in xImpl.GetCustomAttributes(typeof(PlugFieldAttribute), true).Cast<PlugFieldAttribute>()) {
            IDictionary<string, PlugFieldAttribute> xFields = null;
            if (!mPlugFields.TryGetValue(xPlug.Key, out xFields)) {
              xFields = new Dictionary<string, PlugFieldAttribute>();
              mPlugFields.Add(xPlug.Key, xFields);
            }
            if (xFields.ContainsKey(xField.FieldId)) {
              throw new Exception("Duplicate PlugField found for field '" + xField.FieldId + "'!");
            }
            xFields.Add(xField.FieldId, xField);
          }
          #endregion
        }
      }
    }

    public event Action<string> TempDebug;
    private void DoTempDebug(string message) {
      if (TempDebug != null) {
        TempDebug(message);
      } else {
        System.Diagnostics.Debug.WriteLine(message);
      }
    }

    public void Execute(System.Reflection.MethodBase aStartMethod) {
      if (aStartMethod == null) {
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
      // -But we can't scan them utnil we plug them, becuase we will scan things
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
      FindPlugImpls();
      // Now that we found all plugs, scan them.
      // We have to scan them after we find all plugs, but because
      // plugs can use other plugs
      ScanPlugs(mPlugImpls);
      ScanPlugs(mPlugImplsInhrt);
      foreach (var xPlug in mPlugImpls) {
        DoTempDebug(String.Format("Plug found: '{0}'", xPlug.Key.FullName));
      }

      ILOp.mPlugFields = mPlugFields;

      // Pull in extra implementations, GC etc.
      Queue(RuntimeEngineRefs.InitializeApplicationRef, null, "Explicit Entry");
      Queue(RuntimeEngineRefs.FinalizeApplicationRef, null, "Explicit Entry");
      //Queue(typeof(CosmosAssembler).GetMethod("PrintException"), null, "Explicit Entry");
      Queue(VTablesImplRefs.LoadTypeTableRef, null, "Explicit Entry");
      Queue(VTablesImplRefs.SetMethodInfoRef, null, "Explicit Entry");
      Queue(VTablesImplRefs.IsInstanceRef, null, "Explicit Entry");
      Queue(VTablesImplRefs.SetTypeInfoRef, null, "Explicit Entry");
      Queue(VTablesImplRefs.GetMethodAddressForTypeRef, null, "Explicit Entry");
      Queue(GCImplementationRefs.IncRefCountRef, null, "Explicit Entry");
      Queue(GCImplementationRefs.DecRefCountRef, null, "Explicit Entry");
      Queue(GCImplementationRefs.AllocNewObjectRef, null, "Explicit Entry");
      // for now, to ease runtime exception throwing
      Queue(typeof(ExceptionHelper).GetMethod("ThrowNotImplemented", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null), null, "Explicit Entry");
      Queue(typeof(ExceptionHelper).GetMethod("ThrowOverflow", BindingFlags.Static | BindingFlags.Public, null, new Type[] { }, null), null, "Explicit Entry");
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

      MethodAndTypeLabelsHolder.GC_IncRefLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef);

      ScanQueue();
      UpdateAssemblies();
      Assemble();

      mAsmblr.EmitEntrypoint(aStartMethod);
    }

    public void QueueMethod(MethodBase method) {
      Queue(method, null, "Explicit entry via QueueMethod");
    }

    /// <summary>
    /// This method changes the opcodes. Changes are:
    /// * inserting the ValueUID for method ops.
    /// </summary>
    /// <param name="aOpCodes"></param>
    private void ProcessInstructions(List<ILOpCode> aOpCodes) {
      foreach (var xOpCode in aOpCodes) {
        var xOpMethod = xOpCode as ILOpCodes.OpMethod;
        if (xOpMethod != null) {
          xOpMethod.Value = (MethodBase)mItems.GetItemInList(xOpMethod.Value);
          xOpMethod.ValueUID = (uint)GetMethodUID(xOpMethod.Value, true);
          xOpMethod.BaseMethodUID = GetMethodUID(xOpMethod.Value, false);
        }
      }
    }

    public void Dispose() {
      if (mLogEnabled) {
        // Create bookmarks, but also a dictionary that
        // we can find the items in
        var xBookmarks = new Dictionary<object, int>();
        int xBookmark = 0;
        foreach (var xList in mLogMap) {
          foreach (var xItem in xList.Value) {
            xBookmarks.Add(xItem.Item, xBookmark);
            xBookmark++;
          }
        }

        using (mLogWriter = new StreamWriter(mMapPathname, false)) {
          mLogWriter.WriteLine("<html><body>");
          foreach (var xList in mLogMap) {
            mLogWriter.WriteLine("<hr>");

            // Emit bookmarks above source, so when clicking links user doesn't need
            // to constantly scroll up.
            foreach (var xItem in xList.Value) {
              mLogWriter.WriteLine("<a name=\"Item" + xBookmarks[xItem.Item].ToString() + "\"></a>");
            }

            int xHref;
            if (!xBookmarks.TryGetValue(xList.Key, out xHref)) {
              xHref = -1;
            }
            mLogWriter.Write("<p>");
            if (xHref >= 0) {
              mLogWriter.WriteLine("<a href=\"#Item" + xHref.ToString() + "\">");
            }
            if (xList.Key == null) {
              mLogWriter.WriteLine("Unspecified Source");
            } else {
              mLogWriter.WriteLine(LogItemText(xList.Key));
            }
            if (xHref >= 0) {
              mLogWriter.Write("</a>");
            }
            mLogWriter.WriteLine("</a></p>");

            mLogWriter.WriteLine("<ul>");
            foreach (var xItem in xList.Value) {
              mLogWriter.Write("<li>" + LogItemText(xItem.Item) + "</li>");

              mLogWriter.WriteLine("<ul>");
              mLogWriter.WriteLine("<li>" + xItem.SrcType + "</<li>");
              mLogWriter.WriteLine("</ul>");
            }
            mLogWriter.WriteLine("</ul>");
          }
          mLogWriter.WriteLine("</body></html>");
        }
      }
    }

    public int MethodCount {
      get {
        return 0;
      }
    }

    protected string LogItemText(object aItem) {
      if (aItem is MethodBase) {
        var x = (MethodBase)aItem;
        return "Method: " + x.DeclaringType + "." + x.Name + "<br>" + x.GetFullName();
      } else if (aItem is Type) {
        var x = (Type)aItem;
        return "Type: " + x.FullName;
      } else {
        return "Other: " + aItem.ToString();
      }
    }

    protected void FindPlugImpls() {
      // TODO: Cache method list with info - so we dont have to keep
      // scanning attributes for enabled etc repeatedly
      // TODO: New plug system, common plug base which all descend from
      // It can have a "this" member and then we
      // can separate static from instance by the static keyword
      // and ctors can be static "ctor" by name
      // Will still need plug attrib though to specify target
      // Also need to handle asm plugs, but those will be different anyways
      // TODO: Allow whole class plugs? ie, a class that completely replaces another class
      // and is substituted on the fly? Plug scanner would direct all access to that
      // class and throw an exception if any method, field, member etc is missing.
      foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies()) {
        if (!xAsm.GlobalAssemblyCache) {
          //if (xAsm.GetName().Name == "Cosmos.IL2CPU.X86") {
          //  // skip this assembly for now. at the moment we introduced the AssemblerMethod.AssembleNew method, for allowing those to work
          //  // with the Cosmos.IL2CPU* stack, we found we could not use the Cosmos.IL2CPU.X86 plugs, as they contained some AssemblerMethods. 
          //  // This would result in a circular reference, thus we copied them to a new assembly. While the Cosmos.IL2CPU.X86 assembly is being
          //  // referenced, we need to skip it here.
          //  continue;
          //}
          // Find all classes marked as a Plug
          foreach (var xPlugType in xAsm.GetTypes()) {
            // Foreach, it is possible there could be one plug class with mult plug targets
            foreach (PlugAttribute xAttrib in xPlugType.GetCustomAttributes(typeof(PlugAttribute), false)) {
              var xTargetType = xAttrib.Target;
              // If no type is specified, try to find by a specified name.
              // This is needed in cross assembly references where the
              // plug cannot reference the assembly of the target type
              if (xTargetType == null) {
                try {
                  xTargetType = Type.GetType(xAttrib.TargetName, true, false);
                } catch (Exception ex) {
                  throw new Exception("Error", ex);
                }
              }
              // Only keep this plug if its for MS.NET.
              // TODO: Integrate with builder options to allow Mono support again.
              if (!xAttrib.IsMonoOnly) {
                var mPlugs = xAttrib.Inheritable ? mPlugImplsInhrt : mPlugImpls;
                List<Type> xImpls;
                if (mPlugs.TryGetValue(xTargetType, out xImpls)) {
                  xImpls.Add(xPlugType);
                } else {
                  xImpls = new List<Type>();
                  xImpls.Add(xPlugType);
                  mPlugs.Add(xTargetType, xImpls);
                }
              }
            }
          }
        }
      }
    }

    protected void ScanMethod(MethodBase aMethod, bool aIsPlug, object sourceItem) {
      var xParams = aMethod.GetParameters();
      var xParamTypes = new Type[xParams.Length];
      // Dont use foreach, enum generaly keeps order but
      // isn't guaranteed.
      for (int i = 0; i < xParams.Length; i++) {
        xParamTypes[i] = xParams[i].ParameterType;
        Queue(xParamTypes[i], MethodInfoLabelGenerator.GenerateFullName(aMethod), "Parameter");
      }
      var xIsDynamicMethod = aMethod.DeclaringType == null;
      // Queue Types directly related to method
      if (!aIsPlug) {
        // Don't queue declaring types of plugs
        if (!xIsDynamicMethod) {
          // dont queue declaring types of dynamic methods either, those dont have a declaring type
          Queue(aMethod.DeclaringType, MethodInfoLabelGenerator.GenerateFullName(aMethod), "Declaring Type");
        }
      }
      if (aMethod is System.Reflection.MethodInfo) {
        Queue(((System.Reflection.MethodInfo)aMethod).ReturnType, MethodInfoLabelGenerator.GenerateFullName(aMethod), "Return Type");
      }

      // Scan virtuals
      #region Virtuals scan
      if (!xIsDynamicMethod && aMethod.IsVirtual) {
        // For virtuals we need to climb up the type tree
        // and find the top base method. We then add that top
        // node to the mVirtuals list. We don't need to add the 
        // types becuase adding DeclaringType will already cause
        // all ancestor types to be added.

        var xVirtMethod = aMethod;
        var xVirtType = aMethod.DeclaringType;
        MethodBase xNewVirtMethod;
        while (true) {
          xVirtType = xVirtType.BaseType;
          if (xVirtType == null) {
            // We've reached object, can't go farther
            xNewVirtMethod = null;
          } else {
            xNewVirtMethod = xVirtType.GetMethod(aMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, xParamTypes, null);
            if (xNewVirtMethod != null) {
              if (!xNewVirtMethod.IsVirtual) {
                // This can happen if a virtual "replaces" a non virtual
                // above it that is not virtual.
                xNewVirtMethod = null;
              }
            }
          }
          // We dont bother to add these to Queue, because we have to do a 
          // full downlevel scan if its a new base virtual anyways.
          if (xNewVirtMethod == null) {
            // If its already in the list, we mark it null 
            // so we dont do a full downlevel scan.
            if (mVirtuals.Contains(xVirtMethod)) {
              xVirtMethod = null;
            }
            break;
          }
          xVirtMethod = xNewVirtMethod;
        }

        // New virtual base found, we need to downscan it
        // If it was already in mVirtuals, then ScanType will take
        // care of new additions.
        if (xVirtMethod != null) {
          Queue(xVirtMethod, MethodInfoLabelGenerator.GenerateFullName(aMethod), "Virtual Base");
          mVirtuals.Add(xVirtMethod);
          if (aMethod.Name == "ToString") {
            Console.Write("");
          }

          // List changes as we go, cant be foreach
          for (int i = 0; i < mItemsList.Count; i++) {
            if (mItemsList[i] is Type) {
              var xType = (Type)mItemsList[i];
              if (xType.IsSubclassOf(xVirtMethod.DeclaringType)) {
                var xNewMethod = xType.GetMethod(aMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, xParamTypes, null);
                if (xNewMethod != null) {
                  // We need to check IsVirtual, a non virtual could
                  // "replace" a virtual above it?
                  if (xNewMethod.IsVirtual) {
                    Queue(xNewMethod, MethodInfoLabelGenerator.GenerateFullName(aMethod), "Virtual Downscan");
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
      if (!aIsPlug && !xIsDynamicMethod) {
        // Check to see if method is plugged, if it is we don't scan body
        xPlug = ResolvePlug(aMethod, xParamTypes);
      }

      if (xPlug == null) {
        bool xNeedsPlug = false;
        if ((aMethod.Attributes & MethodAttributes.PinvokeImpl) != 0) {
          // pinvoke methods dont have an embedded implementation
          xNeedsPlug = true;
        } else {
          var xImplFlags = aMethod.GetMethodImplementationFlags();
          // todo: prob even more
          if ((xImplFlags & MethodImplAttributes.Native) != 0 ||
              (xImplFlags & MethodImplAttributes.InternalCall) != 0) {
            // native implementations cannot be compiled
            xNeedsPlug = true;
          }
        }
        if (xNeedsPlug) {
          throw new Exception("Native code encountered, plug required. Please see http://cosmos.codeplex.com/wikipage?title=Plugs). " + MethodInfoLabelGenerator.GenerateFullName(aMethod) + "." + Environment.NewLine + " Called from :" + Environment.NewLine + sourceItem);
        }

        //TODO: As we scan each method, we could update or put in a new list
        // that has the resolved plug so we don't have to reresolve it again
        // later for compilation.

        // Scan the method body for more type and method refs
        //TODO: Dont queue new items if they are plugged
        // or do we need to queue them with a resolved ref in a new list?

        InlineAttribute inl = null;
        foreach (InlineAttribute inli in aMethod.GetCustomAttributes(typeof(InlineAttribute), false)) {
          inl = inli;
        }
        if (inl != null)
          return;	// cancel inline

        List<ILOpCode> xOpCodes;
        xOpCodes = mReader.ProcessMethod(aMethod);
        if (xOpCodes != null) {
          ProcessInstructions(xOpCodes);
          foreach (var xOpCode in xOpCodes) {
            if (xOpCode is ILOpCodes.OpMethod) {
              Queue(((ILOpCodes.OpMethod)xOpCode).Value, MethodInfoLabelGenerator.GenerateFullName(aMethod), "Call", sourceItem);
            } else if (xOpCode is ILOpCodes.OpType) {
              Queue(((ILOpCodes.OpType)xOpCode).Value, MethodInfoLabelGenerator.GenerateFullName(aMethod), "OpCode Value");
            } else if (xOpCode is ILOpCodes.OpField) {
              var xOpField = (ILOpCodes.OpField)xOpCode;
              //TODO: Need to do this? Will we get a ILOpCodes.OpType as well?
              Queue(xOpField.Value.DeclaringType, MethodInfoLabelGenerator.GenerateFullName(aMethod), "OpCode Value");
              if (xOpField.Value.IsStatic) {
                //TODO: Why do we add static fields, but not instance?
                // AW: instance fields are "added" always, as part of a type, but for static fields, we need to emit a datamember
                Queue(xOpField.Value, MethodInfoLabelGenerator.GenerateFullName(aMethod), "OpCode Value");
              }
            } else if (xOpCode is ILOpCodes.OpToken) {
              var xTokenOp = (ILOpCodes.OpToken)xOpCode;
              if (xTokenOp.ValueIsType) {
                Queue(xTokenOp.ValueType, MethodInfoLabelGenerator.GenerateFullName(aMethod), "OpCode Value");
              }
              if (xTokenOp.ValueIsField) {
                Queue(xTokenOp.ValueField.DeclaringType, MethodInfoLabelGenerator.GenerateFullName(aMethod), "OpCode Value");
                if (xTokenOp.ValueField.IsStatic) {
                  //TODO: Why do we add static fields, but not instance?
                  // AW: instance fields are "added" always, as part of a type, but for static fields, we need to emit a datamember
                  Queue(xTokenOp.ValueField, MethodInfoLabelGenerator.GenerateFullName(aMethod), "OpCode Value");
                }
              }
            }
          }
        }
      }
    }

    protected void ScanType(Type aType) {
      if (aType.IsArray) {
        Console.Write("");
      }
      // Add immediate ancestor type
      // We dont need to crawl up farther, when the BaseType is scanned 
      // it will add its BaseType, and so on.
      if (aType.BaseType != null) {
        Queue(aType.BaseType, aType, "Base Type");
      }
      // Queue static ctors
      // We always need static ctors, else the type cannot 
      // be created.
      foreach (var xCctor in aType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
        if (xCctor.DeclaringType == aType) {
          Queue(xCctor, aType, "Static Constructor");
        }
      }

      // For each new type, we need to scan for possible new virtuals
      // in our new type if its a descendant of something in 
      // mVirtuals.
      foreach (var xVirt in mVirtuals) {
        // See if our new type is a subclass of any virt's DeclaringTypes
        // If so our new type might have some virtuals
        if (aType.IsSubclassOf(xVirt.DeclaringType)) {
          var xParams = xVirt.GetParameters();
          var xParamTypes = new Type[xParams.Length];
          // Dont use foreach, enum generaly keeps order but
          // isn't guaranteed.
          for (int i = 0; i < xParams.Length; i++) {
            xParamTypes[i] = xParams[i].ParameterType;
          }
          var xMethod = aType.GetMethod(xVirt.Name, xParamTypes);
          if (xMethod != null) {
            // We need to check IsVirtual, a non virtual could
            // "replace" a virtual above it?
            if (xMethod.IsVirtual) {
              Queue(xMethod, aType, "Virtual");
            }
          }
        }
        if (!aType.IsGenericParameter && xVirt.DeclaringType.IsInterface) {
          if (aType.GetInterfaces().Contains(xVirt.DeclaringType)) {
            var xIntfMapping = aType.GetInterfaceMap(xVirt.DeclaringType);
            if (xIntfMapping.InterfaceMethods != null && xIntfMapping.TargetMethods != null) {
              var xIdx = Array.IndexOf(xIntfMapping.InterfaceMethods, xVirt);
              if (xIdx != -1) {
                Queue(xIntfMapping.TargetMethods[xIdx], aType, "Virtual");
              }
            }
          }
        }

      }
    }

    protected void ScanQueue() {
      while (mQueue.Count > 0) {
        var xItem = mQueue.Dequeue();
        // Check for MethodBase first, they are more numerous 
        // and will reduce compares
        if (xItem.Item is MethodBase) {
          ScanMethod((MethodBase)xItem.Item, false, xItem.SourceItem);
        } else if (xItem.Item is Type) {
          var xType = (Type)xItem.Item;
          ScanType(xType);

          // Methods and fields cant exist without types, so we only update
          // mUsedAssemblies in type branch.
          if (!mUsedAssemblies.Contains(xType.Assembly)) {
            mUsedAssemblies.Add(xType.Assembly);
          }
        } else if (xItem.Item is FieldInfo) {
          // todo: static fields need more processing?
        } else {
          throw new Exception("Unknown item found in queue.");
        }
      }
    }

    protected void LogMapPoint(object aSrc, string aSrcType, object aItem) {
      // Keys cant be null. If null, we just say ILScanner is the source
      if (aSrc == null) {
        aSrc = typeof(ILScanner);
      }

      var xLogItem = new LogItem() {
        SrcType = aSrcType,
        Item = aItem
      };
      List<LogItem> xList;
      if (!mLogMap.TryGetValue(aSrc, out xList)) {
        xList = new List<LogItem>();
        mLogMap.Add(aSrc, xList);
      }
      xList.Add(xLogItem);
    }

    protected MethodBase ResolvePlug(Type aTargetType, List<Type> aImpls, MethodBase aMethod, Type[] aParamTypes) {
      //TODO: This method is "reversed" from old - remember that when porting
      MethodBase xResult = null;

      // Setup param types for search
      Type[] xParamTypes;
      if (aMethod.IsStatic) {
        xParamTypes = aParamTypes;
      } else {
        // If its an instance method, we have to add this to the ParamTypes to search
        xParamTypes = new Type[aParamTypes.Length + 1];
        if (aParamTypes.Length > 0) {
          aParamTypes.CopyTo(xParamTypes, 1);
        }
        xParamTypes[0] = aTargetType;
      }

      PlugMethodAttribute xAttrib = null;
      foreach (var xImpl in aImpls) {
        // TODO: cleanup this loop, next statement shouldnt be neccessary
        if (xResult != null) {
          break;
        }
        // Plugs methods must be static, and public
        // Search for non signature matches first since signature searches are slower
        xResult = xImpl.GetMethod(aMethod.Name, BindingFlags.Static | BindingFlags.Public
          , null, xParamTypes, null);
        if (xResult == null && aMethod.Name == ".ctor") {
          xResult = xImpl.GetMethod("Ctor", BindingFlags.Static | BindingFlags.Public
            , null, xParamTypes, null);
        }
        if (xResult == null && aMethod.Name == ".cctor") {
          xResult = xImpl.GetMethod("CCtor", BindingFlags.Static | BindingFlags.Public
            , null, xParamTypes, null);
        }

        if (xResult == null) {
          // Search by signature
          foreach (var xSigMethod in xImpl.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
            // TODO: Only allow one, but this code for now takes the last one
            // if there is more than one
            xAttrib = null;
            foreach (PlugMethodAttribute x in xSigMethod.GetCustomAttributes(typeof(PlugMethodAttribute), false)) {
              xAttrib = x;
            }

            if (xAttrib != null && (xAttrib.IsWildcard && !xAttrib.WildcardMatchParameters)) {
              MethodBase xTargetMethod = null;
              if (String.Compare(xSigMethod.Name, "Ctor", true) == 0 ||
                 String.Compare(xSigMethod.Name, "Cctor", true) == 0) {
                xTargetMethod = aTargetType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).SingleOrDefault();
              } else {
                xTargetMethod = (from item in aTargetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                                 where item.Name == xSigMethod.Name
                                 select item).SingleOrDefault();
              }
              if (xTargetMethod == aMethod) {
                xResult = xSigMethod;
              }
            } else {

              var xParams = xSigMethod.GetParameters();
              //TODO: Static method plugs dont seem to be separated 
              // from instance ones, so the only way seems to be to try
              // to match instance first, and if no match try static.
              // I really don't like this and feel we need to find
              // an explicit way to determine or mark the method 
              // implementations.
              //
              // Plug implementations take "this" as first argument
              // so when matching we don't include it in the search
              Type[] xTypesInst = null;
              var xActualParamCount = xParams.Length;
              foreach (var xParam in xParams) {
                if (xParam.GetCustomAttributes(typeof(FieldAccessAttribute), false).Length > 0) {
                  xActualParamCount--;
                }
              }
              Type[] xTypesStatic = new Type[xActualParamCount];
              // If 0 params, has to be a static plug so we skip
              // any copying and leave xTypesInst = null
              // If 1 params, xTypesInst must be converted to Type[0]
              if (xActualParamCount == 1) {
                xTypesInst = new Type[0];

                var xReplaceType = xParams[0].GetCustomAttributes(typeof(FieldTypeAttribute), false);
                if (xReplaceType.Length == 1)
                  xTypesStatic[0] = Type.GetType(((FieldTypeAttribute)xReplaceType[0]).Name, true);
                else
                  xTypesStatic[0] = xParams[0].ParameterType;
              } else if (xActualParamCount > 1) {
                xTypesInst = new Type[xActualParamCount - 1];
                var xCurIdx = 0;
                foreach (var xParam in xParams.Skip(1)) {
                  if (xParam.GetCustomAttributes(typeof(FieldAccessAttribute), false).Length > 0) {
                    continue;
                  }

                  var xReplaceType = xParam.GetCustomAttributes(typeof(FieldTypeAttribute), false);
                  if (xReplaceType.Length == 1)
                    xTypesInst[xCurIdx] = Type.GetType(((FieldTypeAttribute)xReplaceType[0]).Name, true);
                  else
                    xTypesInst[xCurIdx] = xParam.ParameterType;

                  xCurIdx++;
                }
                xCurIdx = 0;
                foreach (var xParam in xParams) {
                  if (xParam.GetCustomAttributes(typeof(FieldAccessAttribute), false).Length > 0) {
                    xCurIdx++;
                    continue;
                  }
                  if (xCurIdx >= xTypesStatic.Length) {
                    break;
                  }
                  xTypesStatic[xCurIdx] = xParam.ParameterType;
                  xCurIdx++;
                }
              }
              System.Reflection.MethodBase xTargetMethod = null;
              // TODO: In future make rule that all ctor plugs are called
              // ctor by name, or use a new attrib
              //TODO: Document all the plug stuff in a document on website
              //TODO: To make inclusion of plugs easy, we can make a plugs master
              // that references the other default plugs so user exes only 
              // need to reference that one.
              // TODO: Skip FieldAccessAttribute if in impl
              if (xTypesInst != null) {
                if (string.Compare(xSigMethod.Name, "ctor", true) == 0) {
                  xTargetMethod = aTargetType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesInst, null);
                } else {
                  xTargetMethod = aTargetType.GetMethod(xSigMethod.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesInst, null);
                }
              }
              // Not an instance method, try static
              if (xTargetMethod == null) {
                if (string.Compare(xSigMethod.Name, "cctor", true) == 0
                  || string.Compare(xSigMethod.Name, "ctor", true) == 0) {
                  xTargetMethod = aTargetType.GetConstructor(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesStatic, null);
                } else {
                  xTargetMethod = aTargetType.GetMethod(xSigMethod.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesStatic, null);
                }
              }
              if (xTargetMethod == aMethod) {
                xResult = xSigMethod;
                break;
              }
              if (xAttrib != null && xAttrib.Signature != null) {
                var xName = DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GenerateFullName(aMethod));
                if (string.Compare(xName, xAttrib.Signature, true) == 0) {
                  xResult = xSigMethod;
                  break;
                }
              }
              xAttrib = null;
            }
          }
        } else {
          // check if signatur is equal
          var xResPara = xResult.GetParameters();
          var xAMethodPara = aMethod.GetParameters();
          if (aMethod.IsStatic) {
            if (xResPara.Length != xAMethodPara.Length)
              return null;
          } else {
            if (xResPara.Length - 1 != xAMethodPara.Length)
              return null;
          }
          for (int i = 0; i < xAMethodPara.Length; i++) {
            int correctIndex = aMethod.IsStatic ? i : i + 1;
            if (xResPara[correctIndex].ParameterType != xAMethodPara[i].ParameterType)
              return null;
          }
          if (xResult.Name == "Ctor" && aMethod.Name == ".ctor") {
          } else if (xResult.Name == "CCtor" && aMethod.Name == ".cctor") {
          } else if (xResult.Name != aMethod.Name)
            return null;
        }
      }
      if (xResult == null)
        return null;

      // If we found a matching method, check for attributes 
      // that might disable it.
      //TODO: For signature ones, we could cache the attrib. Thats 
      // why we check for null here
      if (xAttrib == null) {
        // TODO: Only allow one, but this code for now takes the last one
        // if there is more than one
        foreach (PlugMethodAttribute x in xResult.GetCustomAttributes(typeof(PlugMethodAttribute), false)) {
          xAttrib = x;
        }
      }

      // See if we need to disable this plug
      if (xAttrib != null) {
        if (!xAttrib.Enabled) {
          //xResult = null;
          return null;
        } else if (xAttrib.IsMonoOnly) {
          //TODO: Check this against build options
          //TODO: Two exclusive IsOnly's dont make sense
          // refactor these as a positive rather than negative
          // Same thing at type plug level
          //xResult = null;
          return null;
        }
        //else if (xAttrib.Signature != null) {
        //  var xName = DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GenerateFullName(xResult));
        //  if (string.Compare(xName, xAttrib.Signature, true) != 0) {
        //    xResult = null;
        //  }
        //}
      }

      InlineAttribute xInlineAttrib = null;
      foreach (InlineAttribute inli in xResult.GetCustomAttributes(typeof(InlineAttribute), false)) {
        xInlineAttrib = inli;
      }

      if (xInlineAttrib == null)
        Queue(xResult, null, "Plug Method");
      //if (xAttrib != null && xAttrib.Signature != null)
      //{
      //    var xTargetMethods = aTargetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      //    //System_Void__Indy_IL2CPU_Assembler_Assembler__cctor__
      //    //If signature exists, the search is slow. Signatures
      //    //are infrequent though, so for now we just go slow method
      //    //and have not optimized or cached this info. When we
      //    //redo the plugs, we can fix this.
      //    bool xEnabled=true;
      //    foreach (var xTargetMethod in xTargetMethods)
      //    {
      //        string sName = DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GenerateFullName(xTargetMethod));
      //        if (string.Compare(sName, xAttrib.Signature, true) == 0)
      //        {
      //            //uint xUID = QueueMethod(xPlugImpl.Plug, "Plug", xMethod, true);
      //            //mMethodPlugs.Add(xTargetMethod, new PlugInfo(xUID, xAttrib.Assembler));
      //            // Mark as disabled, because we already handled it
      //            xEnabled = false;
      //            break;
      //        }
      //    }
      //    // if still enabled, we didn't find our method
      //    if (xEnabled)
      //    {
      //        // todo: more precise error: imagine having a 100K line project, and this error happens...
      //        throw new Exception("Plug target method not found.");
      //    }
      //}
      return xResult;
    }

    #region Plug Caching
    private Orvid.Collections.SkipList ResolvedPlugs = new Orvid.Collections.SkipList();
    private static string BuildMethodKeyName(MethodBase m) {
      return MethodInfoLabelGenerator.GenerateFullName(m);
    }
    #endregion

    protected MethodBase ResolvePlug(MethodBase aMethod, Type[] aParamTypes) {
      MethodBase xResult = null;
      if (ResolvedPlugs.Contains(BuildMethodKeyName(aMethod), out xResult)) {
        return xResult;
      } else {
        if (aMethod.DeclaringType.Name == "Delegate" && aMethod.Name == "InternalAllocLike" && aMethod.GetParameters().Length > 0) {
          Console.Write("");
        }

        // TODO: Right now plugs are compiled in, even if they are not needed.
        // Maybe change this so plugs that are not needed are not compiled in?
        // To do so, maybe plugs could be marked as they are used

        List<Type> xImpls;
        // Check for exact type plugs first, they have precedence
        if (mPlugImpls.TryGetValue(aMethod.DeclaringType, out xImpls)) {
          xResult = ResolvePlug(aMethod.DeclaringType, xImpls, aMethod, aParamTypes);
        }

        // Check for inheritable plugs second.
        // We also need to fall through at method level, not just type.
        // That is a exact type plug could exist, but not method match.
        // In such a case the Inheritable methods should still be searched
        // if there is a inheritable type match.
        if (xResult == null) {
          foreach (var xInheritable in mPlugImplsInhrt) {
            if (aMethod.DeclaringType.IsSubclassOf(xInheritable.Key)) {
              xResult = ResolvePlug(aMethod.DeclaringType/*xInheritable.Key*/, xInheritable.Value, aMethod, aParamTypes);
              if (xResult != null) {
                // prevent key overriding.
                break;
              }
            }
          }
        }

        ResolvedPlugs.Add(BuildMethodKeyName(aMethod), xResult);

        return xResult;
      }
    }

    private MethodBase GetUltimateBaseMethod(MethodBase aMethod, Type[] aMethodParams, Type aCurrentInspectedType) {
      MethodBase xBaseMethod = null;
      //try {
      while (true) {
        if (aCurrentInspectedType.BaseType == null) {
          break;
        }
        aCurrentInspectedType = aCurrentInspectedType.BaseType;
        MethodBase xFoundMethod = aCurrentInspectedType.GetMethod(aMethod.Name,
                                                                  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                                                  Type.DefaultBinder,
                                                                  aMethodParams,
                                                                  new ParameterModifier[0]);
        if (xFoundMethod == null) {
          break;
        }
        ParameterInfo[] xParams = xFoundMethod.GetParameters();
        bool xContinue = true;
        for (int i = 0; i < xParams.Length; i++) {
          if (xParams[i].ParameterType != aMethodParams[i]) {
            xContinue = false;
            continue;
          }
        }
        if (!xContinue) {
          continue;
        }
        if (xFoundMethod != null) {
          xBaseMethod = xFoundMethod;

          if (xFoundMethod.IsVirtual == aMethod.IsVirtual && xFoundMethod.IsPrivate == false && xFoundMethod.IsPublic == aMethod.IsPublic && xFoundMethod.IsFamily == aMethod.IsFamily && xFoundMethod.IsFamilyAndAssembly == aMethod.IsFamilyAndAssembly && xFoundMethod.IsFamilyOrAssembly == aMethod.IsFamilyOrAssembly && xFoundMethod.IsFinal == false) {
            var xFoundMethInfo = xFoundMethod as SR.MethodInfo;
            var xBaseMethInfo = xBaseMethod as SR.MethodInfo;
            if (xFoundMethInfo == null && xBaseMethInfo == null) {
              xBaseMethod = xFoundMethod;
            }
            if (xFoundMethInfo != null && xBaseMethInfo != null) {
              if (xFoundMethInfo.ReturnType.AssemblyQualifiedName.Equals(xBaseMethInfo.ReturnType.AssemblyQualifiedName)) {
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

    protected uint GetMethodUID(MethodBase aMethod, bool aExact) {
      if (!aExact) {
        ParameterInfo[] xParams = aMethod.GetParameters();
        Type[] xParamTypes = new Type[xParams.Length];
        for (int i = 0; i < xParams.Length; i++) {
          xParamTypes[i] = xParams[i].ParameterType;
        }
        var xBaseMethod = GetUltimateBaseMethod(aMethod, xParamTypes, aMethod.DeclaringType);
        if (!mMethodUIDs.ContainsKey(xBaseMethod)) {
          var xId = (uint)mMethodUIDs.Count;
          mMethodUIDs.Add(xBaseMethod, xId);
        }
        return mMethodUIDs[xBaseMethod];
      } else {
        if (!mMethodUIDs.ContainsKey(aMethod)) {
          var xId = (uint)mMethodUIDs.Count;
          mMethodUIDs.Add(aMethod, xId);
        }
        return mMethodUIDs[aMethod];
      }
    }

    protected uint GetTypeUID(Type aType) {
      if (!mItems.Contains(aType)) {
        throw new Exception("Cannot get UID of types which are not queued!");
      }
      if (!mTypeUIDs.ContainsKey(aType)) {
        var xId = (uint)mTypeUIDs.Count;
        mTypeUIDs.Add(aType, xId);
        return xId;
      } else {
        return mTypeUIDs[aType];
      }
    }

    protected void UpdateAssemblies() {
      // It would be nice to keep DebugInfo output into assembler only but
      // there is so much info that is available in scanner that is needed
      // or can be used in a more efficient manner. So we output in both 
      // scanner and assembler as needed.
      var xAssemblies = new List<Cosmos.Debug.Common.AssemblyFile>();
      foreach (var xAsm in mUsedAssemblies) {
        var xRow = new Cosmos.Debug.Common.AssemblyFile() {
          ID = Guid.NewGuid(),
          Pathname = xAsm.Location
        };
        xAssemblies.Add(xRow);

        mAsmblr.Assemblies.Add(xAsm, xRow.ID);
      }
      mAsmblr.DebugInfo.AddAssemblies(xAssemblies);
    }

    protected void Assemble() {
      foreach (var xItem in mItems) {
        if (xItem is MethodBase) {
          var xMethod = (MethodBase)xItem;
          var xParams = xMethod.GetParameters();
          var xParamTypes = xParams.Select(q => q.ParameterType).ToArray();
          var xPlug = ResolvePlug(xMethod, xParamTypes);
          var xMethodType = MethodInfo.TypeEnum.Normal;
          Type xPlugAssembler = null;
          MethodInfo xPlugInfo = null;
          if (xPlug != null) {
            xMethodType = MethodInfo.TypeEnum.NeedsPlug;
            PlugMethodAttribute xAttrib = null;
            foreach (PlugMethodAttribute attrib in xPlug.GetCustomAttributes(typeof(PlugMethodAttribute), true)) {
              xAttrib = attrib;
            }
            if (xAttrib != null) {
              xPlugAssembler = xAttrib.Assembler;
              xPlugInfo = new MethodInfo(xPlug, (uint)mItemsList.IndexOf(xPlug), MethodInfo.TypeEnum.Plug, null, xPlugAssembler);

              var xMethodInfo = new MethodInfo(xMethod, (uint)mItemsList.IndexOf(xMethod), xMethodType, xPlugInfo/*, xPlugAssembler*/);
              if (xAttrib != null && xAttrib.IsWildcard) {
                xPlugInfo.PluggedMethod = xMethodInfo;
                var xInstructions = mReader.ProcessMethod(xPlug);
                if (xInstructions != null) {
                  ProcessInstructions(xInstructions);
                  mAsmblr.ProcessMethod(xPlugInfo, xInstructions);
                }
              }
              mAsmblr.GenerateMethodForward(xMethodInfo, xPlugInfo);
            } else {
              InlineAttribute inl = null;
              foreach (InlineAttribute inli in xPlug.GetCustomAttributes(typeof(InlineAttribute), false)) {
                inl = inli;
              }
              if (inl != null) {
                xPlugInfo = new MethodInfo(xPlug, (uint)mItemsList.IndexOf(xItem), MethodInfo.TypeEnum.Plug, null, true);

                var xMethodInfo = new MethodInfo(xMethod, (uint)mItemsList.IndexOf(xMethod), xMethodType, xPlugInfo/*, xPlugAssembler*/);

                xPlugInfo.PluggedMethod = xMethodInfo;
                var xInstructions = mReader.ProcessMethod(xPlug);
                if (xInstructions != null) {
                  ProcessInstructions(xInstructions);
                  mAsmblr.ProcessMethod(xPlugInfo, xInstructions);
                }
                mAsmblr.GenerateMethodForward(xMethodInfo, xPlugInfo);
              } else {
                xPlugInfo = new MethodInfo(xPlug, (uint)mItemsList.IndexOf(xPlug), MethodInfo.TypeEnum.Plug, null, xPlugAssembler);

                var xMethodInfo = new MethodInfo(xMethod, (uint)mItemsList.IndexOf(xMethod), xMethodType, xPlugInfo/*, xPlugAssembler*/);
                if (xAttrib != null && xAttrib.IsWildcard) {
                  xPlugInfo.PluggedMethod = xMethodInfo;
                  var xInstructions = mReader.ProcessMethod(xPlug);
                  if (xInstructions != null) {
                    ProcessInstructions(xInstructions);
                    mAsmblr.ProcessMethod(xPlugInfo, xInstructions);
                  }
                }
                mAsmblr.GenerateMethodForward(xMethodInfo, xPlugInfo);
              }
            }
          } else {
            PlugMethodAttribute xAttrib = null;
            foreach (PlugMethodAttribute attrib in xMethod.GetCustomAttributes(typeof(PlugMethodAttribute), true)) {
              xAttrib = attrib;
            }
            if (xAttrib != null && xAttrib.IsWildcard) {
              continue;
              //xPlugAssembler = xAttrib.Assembler;
            }
            if (xAttrib != null) {
              xPlugAssembler = xAttrib.Assembler;
            }
            var xMethodInfo = new MethodInfo(xMethod, (uint)mItemsList.IndexOf(xMethod), xMethodType, xPlugInfo, xPlugAssembler);
            var xInstructions = mReader.ProcessMethod(xMethod);
            if (xInstructions != null) {
              ProcessInstructions(xInstructions);
              mAsmblr.ProcessMethod(xMethodInfo, xInstructions);
            }
          }
        } else if (xItem is FieldInfo) {
          mAsmblr.ProcessField((FieldInfo)xItem);
        }
      }
    
      var xTypes = new HashSet<Type>();
      var xMethods = new HashSet<MethodBase>();
      foreach (var xItem in mItems) {
        if (xItem is MethodBase) {
          xMethods.Add((MethodBase)xItem);
        } else if (xItem is Type) {
          xTypes.Add((Type)xItem);
        }
      }
      mAsmblr.GenerateVMTCode(xTypes, xMethods, GetTypeUID, x => GetMethodUID(x, false));
    }
  }
}

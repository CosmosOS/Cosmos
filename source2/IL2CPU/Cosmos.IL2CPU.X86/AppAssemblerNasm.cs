using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using System.Reflection;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using Cosmos.Debug.Common;
using Cosmos.Build.Common;
using CPUx86 = Cosmos.Assembler.x86;
using Mono.Cecil;
using System.IO;

namespace Cosmos.IL2CPU.X86 {
  public class AppAssemblerNasm : AppAssembler {
    public AppAssemblerNasm(byte comNumber)
      : base(comNumber) {
    }
    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

    private List<LOCAL_ARGUMENT_INFO> mLocals_Arguments_Infos = new List<LOCAL_ARGUMENT_INFO>();

    protected override void MethodBegin(MethodInfo aMethod) {
      base.MethodBegin(aMethod);
      if (aMethod.PluggedMethod != null) {
        new Cosmos.Assembler.Label("PLUG_FOR___" + MethodInfoLabelGenerator.GenerateLabelName(aMethod.PluggedMethod.MethodBase));
      } else {
        new Cosmos.Assembler.Label(aMethod.MethodBase);
      }
      var xMethodLabel = Cosmos.Assembler.Label.LastFullLabel;
      if (aMethod.MethodBase.IsStatic && aMethod.MethodBase is ConstructorInfo) {
        new Comment("This is a static constructor. see if it has been called already, and if so, return.");
        var xName = DataMember.FilterStringForIncorrectChars("CCTOR_CALLED__" + MethodInfoLabelGenerator.GetFullName(aMethod.MethodBase.DeclaringType));
        var xAsmMember = new DataMember(xName, (byte)0);
        Assembler.DataMembers.Add(xAsmMember);
        new Compare { DestinationRef = Cosmos.Assembler.ElementReference.New(xName), DestinationIsIndirect = true, Size = 8, SourceValue = 1 };
        new ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = ".BeforeQuickReturn" };
        new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New(xName), DestinationIsIndirect = true, Size = 8, SourceValue = 1 };
        new Jump { DestinationLabel = ".AfterCCTorAlreadyCalledCheck" };
        new Cosmos.Assembler.Label(".BeforeQuickReturn");
        new Mov { DestinationReg = RegistersEnum.ECX, SourceValue = 0 };
        new Return { };
        new Cosmos.Assembler.Label(".AfterCCTorAlreadyCalledCheck");
      }

      new Push { DestinationReg = Registers.EBP };
      new Mov { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      //new CPUx86.Push("0");
      //if (!(aLabelName.Contains("Cosmos.Kernel.Serial") || aLabelName.Contains("Cosmos.Kernel.Heap"))) {
      //    new CPUx86.Push(LdStr.GetContentsArrayName(aAssembler, aLabelName));
      //    MethodBase xTempMethod = Engine.GetMethodBase(Engine.GetType("Cosmos.Kernel", "Cosmos.Kernel.Serial"), "Write", "System.Byte", "System.String");
      //    new CPUx86.Call(MethodInfoLabelGenerator.GenerateLabelName(xTempMethod));
      //    Engine.QueueMethod(xTempMethod);
      //}
      #region Load CodeOffset
      ISymbolMethod xMethodSymbols;
      if (DebugMode == DebugMode.Source) {
        var xSymbolReader = GetSymbolReaderForAssembly(aMethod.MethodBase.DeclaringType.Assembly);
        if (xSymbolReader != null) {
          xMethodSymbols = xSymbolReader.GetMethod(new SymbolToken(aMethod.MethodBase.MetadataToken));
          // This gets the Sequence Points.
          // Sequence Points are spots that identify what the compiler/debugger says is a spot
          // that a breakpoint can occur one. Essentially, an atomic source line in C#
          if (xMethodSymbols != null) {
            xCodeOffsets = new int[xMethodSymbols.SequencePointCount];
            var xCodeDocuments = new ISymbolDocument[xMethodSymbols.SequencePointCount];
            xCodeLineNumbers = new int[xMethodSymbols.SequencePointCount];
            var xCodeColumns = new int[xMethodSymbols.SequencePointCount];
            var xCodeEndLines = new int[xMethodSymbols.SequencePointCount];
            var xCodeEndColumns = new int[xMethodSymbols.SequencePointCount];
            xMethodSymbols.GetSequencePoints(xCodeOffsets, xCodeDocuments
             , xCodeLineNumbers, xCodeColumns, xCodeEndLines, xCodeEndColumns);
          }
        }
      }
      #endregion
      if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null && !aMethod.IsInlineAssembler) {
        // the body of aMethod is getting emitted
        var xBody = aMethod.MethodBase.GetMethodBody();
        if (xBody != null) {
          var xLocalsOffset = mLocals_Arguments_Infos.Count;
          foreach (var xLocal in xBody.LocalVariables) {
            var xInfo = new LOCAL_ARGUMENT_INFO {
              METHODLABELNAME = xMethodLabel,
              IsArgument = false,
              INDEXINMETHOD = xLocal.LocalIndex,
              NAME = "Local" + xLocal.LocalIndex,
              OFFSET = 0 - (int)ILOp.GetEBPOffsetForLocalForDebugger(aMethod, xLocal.LocalIndex),
              TYPENAME = xLocal.LocalType.AssemblyQualifiedName
            };
            mLocals_Arguments_Infos.Add(xInfo);

            var xSize = ILOp.Align(ILOp.SizeOfType(xLocal.LocalType), 4);
            new Comment(String.Format("Local {0}, Size {1}", xLocal.LocalIndex, xSize));
            for (int i = 0; i < xSize / 4; i++) {
              new Push { DestinationValue = 0 };
            }
            //new Sub { DestinationReg = Registers.ESP, SourceValue = ILOp.Align(ILOp.SizeOfType(xLocal.LocalType), 4) };
          }
          var xCecilMethod = GetCecilMethodDefinitionForSymbolReading(aMethod.MethodBase);
          if (xCecilMethod != null && xCecilMethod.Body != null) {
            // mLocals_Arguments_Infos is one huge list, so ourlatest additions are at the end
            for (int i = 0; i < xCecilMethod.Body.Variables.Count; i++) {
              mLocals_Arguments_Infos[xLocalsOffset + i].NAME = xCecilMethod.Body.Variables[i].Name;
            }
            for (int i = xLocalsOffset + xCecilMethod.Body.Variables.Count - 1; i >= xLocalsOffset; i--) {
              if (mLocals_Arguments_Infos[i].NAME.Contains('$')) {
                mLocals_Arguments_Infos.RemoveAt(i);
              }
            }
          }
        }

        // debug info:
        var xIdxOffset = 0u;
        if (!aMethod.MethodBase.IsStatic) {
          mLocals_Arguments_Infos.Add(new LOCAL_ARGUMENT_INFO {
            METHODLABELNAME = xMethodLabel,
            IsArgument = true,
            NAME = "this:" + IL.Ldarg.GetArgumentDisplacement(aMethod, 0),
            INDEXINMETHOD = 0,
            OFFSET = IL.Ldarg.GetArgumentDisplacement(aMethod, 0),
            TYPENAME = aMethod.MethodBase.DeclaringType.AssemblyQualifiedName
          });

          xIdxOffset++;
        }

        var xParams = aMethod.MethodBase.GetParameters();
        var xParamCount = (ushort)xParams.Length;

        for (ushort i = 0; i < xParamCount; i++) {
          var xOffset = IL.Ldarg.GetArgumentDisplacement(aMethod, (ushort)(i + xIdxOffset));
          // if last argument is 8 byte long, we need to add 4, so that debugger could read all 8 bytes from this variable in positiv direction
          xOffset -= (int)Cosmos.IL2CPU.X86.ILOp.Align(Cosmos.IL2CPU.X86.ILOp.SizeOfType(xParams[i].ParameterType), 4) - 4;
          mLocals_Arguments_Infos.Add(new LOCAL_ARGUMENT_INFO {
            METHODLABELNAME = xMethodLabel,
            IsArgument = true,
            INDEXINMETHOD = (int)(i + xIdxOffset),
            NAME = xParams[i].Name,
            OFFSET = xOffset,
            TYPENAME = xParams[i].ParameterType.AssemblyQualifiedName
          });
        }
      }
    }

    private Dictionary<string, ModuleDefinition> mLoadedModules = new Dictionary<string, ModuleDefinition>();
    //private Dictionary<Tuple<string, int>, MethodDefinition> mMethods = new 

    private MethodDefinition GetCecilMethodDefinitionForSymbolReading(MethodBase methodBase) {
      var xMethodBase = methodBase;
      if (xMethodBase.IsGenericMethod) {
        var xMethodInfo = (System.Reflection.MethodInfo)xMethodBase;
        xMethodBase = xMethodInfo.GetGenericMethodDefinition();
        if (xMethodBase.IsGenericMethod) {
          // apparently, a generic method can be derived from a generic method..
          throw new Exception("Make recursive");
        }
      }
      var xLocation = xMethodBase.DeclaringType.Assembly.Location;
      ModuleDefinition xModule = null;
      if (!mLoadedModules.TryGetValue(xLocation, out xModule)) {
        // if not in cache, try loading.
        if (xMethodBase.DeclaringType.Assembly.GlobalAssemblyCache || !File.Exists(xLocation)) {
          // file doesn't exist, so assume no symbols
          mLoadedModules.Add(xLocation, null);
          return null;
        } else {
			try {
				xModule = ModuleDefinition.ReadModule(xLocation, new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new Mono.Cecil.Pdb.PdbReaderProvider() });
			}
			catch (InvalidOperationException) {
				throw new Exception("Please check that dll and pdb file is matching on location: " + xLocation);
			}
          if (xModule.HasSymbols) {
            mLoadedModules.Add(xLocation, xModule);
          } else {
            mLoadedModules.Add(xLocation, null);
            return null;
          }
        }
      }
      if (xModule == null) {
        return null;
      }
      // todo: cache MethodDefinition ?
      return xModule.LookupToken(xMethodBase.MetadataToken) as MethodDefinition;
    }

    protected override void MethodEnd(MethodInfo aMethod) {
      base.MethodEnd(aMethod);
      uint xReturnSize = 0;
      var xMethInfo = aMethod.MethodBase as System.Reflection.MethodInfo;
      if (xMethInfo != null) {
        xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
      }
      if (aMethod.PlugMethod == null && !aMethod.IsInlineAssembler) {
        new Cosmos.Assembler.Label(ILOp.GetMethodLabel(aMethod) + EndOfMethodLabelNameNormal);
      }
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
      var xTotalArgsSize = (from item in aMethod.MethodBase.GetParameters()
                            select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
      if (!aMethod.MethodBase.IsStatic) {
        if (aMethod.MethodBase.DeclaringType.IsValueType) {
          xTotalArgsSize += 4; // only a reference is passed
        } else {
          xTotalArgsSize += (int)ILOp.Align(ILOp.SizeOfType(aMethod.MethodBase.DeclaringType), 4);
        }
      }

      if (aMethod.PluggedMethod != null) {
        xReturnSize = 0;
        xMethInfo = aMethod.PluggedMethod.MethodBase as System.Reflection.MethodInfo;
        if (xMethInfo != null) {
          xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
        }
        xTotalArgsSize = (from item in aMethod.PluggedMethod.MethodBase.GetParameters()
                          select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
        if (!aMethod.PluggedMethod.MethodBase.IsStatic) {
          if (aMethod.PluggedMethod.MethodBase.DeclaringType.IsValueType) {
            xTotalArgsSize += 4; // only a reference is passed
          } else {
            xTotalArgsSize += (int)ILOp.Align(ILOp.SizeOfType(aMethod.PluggedMethod.MethodBase.DeclaringType), 4);
          }
        }
      }

      if (xReturnSize > 0) {
        //var xArgSize = (from item in aArgs
        //                let xSize = item.Size + item.Offset
        //                select xSize).FirstOrDefault();
        //new Comment(String.Format("ReturnSize = {0}, ArgumentSize = {1}",
        //                          aReturnSize,
        //                          xArgSize));
        //int xOffset = 4;
        //if(xArgSize>0) {
        //    xArgSize -= xReturnSize;
        //    xOffset = xArgSize;
        //}
        var xOffset = GetResultCodeOffset(xReturnSize, (uint)xTotalArgsSize);
        for (int i = 0; i < xReturnSize / 4; i++) {
          new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
          new CPUx86.Mov {
            DestinationReg = CPUx86.Registers.EBP,
            DestinationIsIndirect = true,
            DestinationDisplacement = (int)(xOffset + ((i + 0) * 4)),
            SourceReg = Registers.EAX
          };
          //        new CPUx86.Move {
          //            DestinationReg = CPUx86.Registers.EBP,
          //            DestinationIsIndirect = true,
          //            DestinationDisplacement = (int)(xOffset + ((i + 1) * 4) + 4 - xReturnSize),
          //            SourceReg = Registers.EAX
          //        };
        }
        // extra stack space is the space reserved for example when a "public static int TestMethod();" method is called, 4 bytes is pushed, to make room for result;
      }
      new Cosmos.Assembler.Label(ILOp.GetMethodLabel(aMethod) + EndOfMethodLabelNameException);
      //for (int i = 0; i < aLocAllocItemCount; i++) {
      //  new CPUx86.Call { DestinationLabel = aHeapFreeLabel };
      //}
      //if (aDebugMode && aIsNonDebuggable) {
      //  new CPUx86.Call { DestinationLabel = "DebugPoint_DebugResume" };
      //}

      //if ((from xLocal in aLocals
      //     where xLocal.IsReferenceType
      //     select 1).Count() > 0 || (from xArg in aArgs
      //                               where xArg.IsReferenceType
      //                               select 1).Count() > 0) {
      //  new CPUx86.Push { DestinationReg = Registers.ECX };
      //  //foreach (MethodInformation.Variable xLocal in aLocals) {
      //  //  if (xLocal.IsReferenceType) {
      //  //    Op.Ldloc(aAssembler,
      //  //             xLocal,
      //  //             false,
      //  //             aGetStorageSizeDelegate(xLocal.VariableType));
      //  //    new CPUx86.Call { DestinationLabel = aDecRefLabel };
      //  //  }
      //  //}
      //  //foreach (MethodInformation.Argument xArg in aArgs) {
      //  //  if (xArg.IsReferenceType) {
      //  //    Op.Ldarg(aAssembler,
      //  //             xArg,
      //  //             false);
      //  //    //,                                 aGetStorageSizeDelegate(xArg.ArgumentType)
      //  //    new CPUx86.Call { DestinationLabel = aDecRefLabel };
      //  //  }
      //  //}
      //  // todo: add GC code
      //  new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
      //}
      if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null && !aMethod.IsInlineAssembler) {
        var xBody = aMethod.MethodBase.GetMethodBody();
        if (xBody != null) {
          uint xLocalsSize = 0;
          for (int j = xBody.LocalVariables.Count - 1; j >= 0; j--) {
            xLocalsSize += ILOp.Align(ILOp.SizeOfType(xBody.LocalVariables[j].LocalType), 4);

            if (xLocalsSize >= 256) {
              new CPUx86.Add {
                DestinationReg = CPUx86.Registers.ESP,
                SourceValue = 255
              };
              xLocalsSize -= 255;
            }
          }
          if (xLocalsSize > 0) {
            new CPUx86.Add {
              DestinationReg = CPUx86.Registers.ESP,
              SourceValue = xLocalsSize
            };
          }
        }
      }
      //new CPUx86.Add(CPUx86.Registers_Old.ESP, "0x4");
      //new CPUx86.Compare { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      //new CPUx86.ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + EndOfMethodLabelNameException + "__2" };
      //new CPUx86.Xchg { DestinationReg = Registers.BX, SourceReg = Registers.BX };
      //new CPUx86.Halt();
      // TODO: not nice coding, still a test
      //new CPUx86.Move { DestinationReg = Registers.ESP, SourceReg = Registers.EBP };
      new Cosmos.Assembler.Label(ILOp.GetMethodLabel(aMethod) + EndOfMethodLabelNameException + "__2");
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBP };
      var xRetSize = ((int)xTotalArgsSize) - ((int)xReturnSize);
      if (xRetSize < 0) {
        xRetSize = 0;
      }
      WriteDebug(aMethod.MethodBase, (uint)xRetSize, IL.Call.GetStackSizeToReservate(aMethod.MethodBase));
      new CPUx86.Return { DestinationValue = (uint)xRetSize };
    }

    public static uint GetResultCodeOffset(uint aResultSize, uint aTotalArgumentSize) {
      uint xOffset = 8;
      if ((aTotalArgumentSize > 0) && (aTotalArgumentSize >= aResultSize)) {
        xOffset += aTotalArgumentSize;
        xOffset -= aResultSize;
      }
      return xOffset;
    }

    private static ISymbolReader GetSymbolReaderForAssembly(Assembly aAssembly) {
      try {
        return SymbolAccess.GetReaderForFile(aAssembly.Location);
      } catch (NotSupportedException) {
        return null;
      }
    }

    protected override void MethodBegin(string aMethodName) {
      base.MethodBegin(aMethodName);
      new Cosmos.Assembler.Label(aMethodName);
      new Push { DestinationReg = Registers.EBP };
      new Mov { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      xCodeOffsets = new int[0];
    }

    protected override void MethodEnd(string aMethodName) {
      base.MethodEnd(aMethodName);
      new Cosmos.Assembler.Label("_END_OF_" + aMethodName);
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBP };
      new CPUx86.Return();
    }

    private static HashSet<string> mDebugLines = new HashSet<string>();
    private static void WriteDebug(MethodBase aMethod, uint aSize, uint aSize2) {
      var xLine = String.Format("{0}\t{1}\t{2}", MethodInfoLabelGenerator.GenerateFullName(aMethod), aSize, aSize2);

    }
    private List<MLSYMBOL> mSymbols = new List<MLSYMBOL>();

    // These are all temp functions until we move to the new assembler.
    // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
    public static string TmpPosLabel(MethodInfo aMethod, int aOffset) {
      return ILOp.GetLabel(aMethod, aOffset);
    }

    public static string TmpPosLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return TmpPosLabel(aMethod, aOpCode.Position);
    }

    public static string TmpBranchLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return TmpPosLabel(aMethod, ((ILOpCodes.OpBranch)aOpCode).Value);
    }

    protected override void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode) {
      base.BeforeOp(aMethod, aOpCode);
      string xLabel = TmpPosLabel(aMethod, aOpCode);
      Assembler.CurrentIlLabel = xLabel;
      new Cosmos.Assembler.Label(xLabel, "IL");
      
      if (mSymbols != null) {
        var xMLSymbol = new MLSYMBOL();
        xMLSymbol.LABELNAME = TmpPosLabel(aMethod, aOpCode);
        xMLSymbol.METHODNAME = aMethod.MethodBase.GetFullName();

        var xStackSize = (from item in mAssembler.Stack
                          let xSize = (item.Size % 4u == 0u) ? item.Size : (item.Size + (4u - (item.Size % 4u)))
                          select xSize).Sum();
        xMLSymbol.STACKDIFF = -1;
        if (aMethod.MethodBase != null) {
          var xBody = aMethod.MethodBase.GetMethodBody();
          if (xBody != null) {
            var xLocalsSize = (from item in xBody.LocalVariables
                               select ILOp.Align(ILOp.SizeOfType(item.LocalType), 4)).Sum();
            xMLSymbol.STACKDIFF = checked((int)(xLocalsSize + xStackSize));
          }
        }
        try {
          xMLSymbol.ILASMFILE = aMethod.MethodBase.DeclaringType.Assembly.Location;
        } catch (NotSupportedException) {
          xMLSymbol.ILASMFILE = "DYNAMIC: " + aMethod.MethodBase.DeclaringType.Assembly.FullName;
        }
        xMLSymbol.METHODTOKEN = aMethod.MethodBase.MetadataToken;
        xMLSymbol.TYPETOKEN = aMethod.MethodBase.DeclaringType.MetadataToken;
        xMLSymbol.ILOFFSET = aOpCode.Position;
        mSymbols.Add(xMLSymbol);
        DebugInfo.WriteSymbols(mSymbols);
      }
      DebugInfo.WriteSymbols(mSymbols, true);
      
      EmitTracer(aMethod, aOpCode, aMethod.MethodBase.DeclaringType.Namespace, xCodeOffsets);
    }

    public TraceAssemblies TraceAssemblies;
    public bool DebugEnabled = false;
    public DebugMode DebugMode;
    public bool IgnoreDebugStubAttribute;

    protected void EmitTracer(MethodInfo aMethod, ILOpCode aOp, string aNamespace, int[] aCodeOffsets) {
      // NOTE - These if statements can be optimized down - but clarity is
      // more important the optimizations. Furthermoer the optimazations available
      // would not offer much benefit

      // Determine if a new DebugStub should be emitted

      if (aOp.OpCode == ILOpCode.Code.Nop) {
        // Skip NOOP's so we dont have breakpoints on them
        //TODO: Each IL op should exist in IL, and descendants in IL.X86.
        // Because of this we have this hack
        return;
      } else if (DebugEnabled == false) {
        return;
      } else if (DebugMode == DebugMode.Source) {
        // If the current position equals one of the offsets, then we have
        // reached a new atomic C# statement
        if (aCodeOffsets != null) {
          var xIndex = Array.IndexOf(aCodeOffsets, aOp.Position);
          if (xIndex == -1) {
            return;
          } else if (xCodeLineNumbers[xIndex] == 0xFEEFEE) {
            // 0xFEEFEE means hiddenline -> we dont want to stop there
            return;
          }
        }
      }

      // Check if the DebugStub has been disabled for this method
      if ((!IgnoreDebugStubAttribute) && (aMethod.DebugStubOff))
      {
          return;
      }

      // Check options for Debug Level
      // Set based on TracedAssemblies
      if (TraceAssemblies == TraceAssemblies.Cosmos || TraceAssemblies == TraceAssemblies.User) {
        if (aNamespace.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase)) {
          return;
        } else if (aNamespace.ToLower() == "system") {
          return;
        } else if (aNamespace.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase)) {
          return;
        }
        if (TraceAssemblies == TraceAssemblies.User) {
          //TODO: Maybe an attribute that could be used to turn tracing on and off
          //TODO: This doesnt match Cosmos.Kernel exact vs Cosmos.Kernel., so a user 
          // could do Cosmos.KernelMine and it will fail. Need to fix this
          if (aNamespace.StartsWith("Cosmos.Kernel", StringComparison.InvariantCultureIgnoreCase)) {
            return;
          } else if (aNamespace.StartsWith("Cosmos.Sys", StringComparison.InvariantCultureIgnoreCase)) {
            return;
          } else if (aNamespace.StartsWith("Cosmos.Hardware", StringComparison.InvariantCultureIgnoreCase)) {
            return;
          } else if (aNamespace.StartsWith("Cosmos.IL2CPU", StringComparison.InvariantCultureIgnoreCase)) {
            return;
          }
        }
      }

      // If we made it this far without a return, emit the Tracer
      new CPUx86.INT3();
    }

    private int[] xCodeOffsets;
    private int[] xCodeLineNumbers;
    protected override void AfterOp(MethodInfo aMethod, ILOpCode aOpCode) {
      base.AfterOp(aMethod, aOpCode);
      var xContents = "";
      foreach (var xStackItem in mAssembler.Stack) {
        xContents += ILOp.Align((uint)xStackItem.Size, 4);
        xContents += ", ";
      }
      if (xContents.EndsWith(", ")) {
        xContents = xContents.Substring(0, xContents.Length - 2);
      }
      new Comment("Stack contains " + mAssembler.Stack.Count + " items: (" + xContents + ")");
    }

    public void FinalizeDebugInfo() {
      DebugInfo.WriteAllLocalsArgumentsInfos(mLocals_Arguments_Infos);
    }
  }
}
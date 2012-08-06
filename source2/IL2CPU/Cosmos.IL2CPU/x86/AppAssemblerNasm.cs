using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Cosmos.Assembler;
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
  public class AppAssemblerNasm : IL2CPU.AppAssembler {

    public AppAssemblerNasm(byte aComPort)
      : base(aComPort) {
    }

    protected override void MethodBegin(MethodInfo aMethod) {
      base.MethodBegin(aMethod);
      if (aMethod.PluggedMethod != null) {
        new Cosmos.Assembler.Label("PLUG_FOR___" + CPU.MethodInfoLabelGenerator.GenerateLabelName(aMethod.PluggedMethod.MethodBase));
      } else {
        new Cosmos.Assembler.Label(aMethod.MethodBase);
      }
      var xMethodLabel = Cosmos.Assembler.Label.LastFullLabel;
      if (aMethod.MethodBase.IsStatic && aMethod.MethodBase is ConstructorInfo) {
        new CPU.Comment("This is a static constructor. see if it has been called already, and if so, return.");
        var xName = CPU.DataMember.FilterStringForIncorrectChars("CCTOR_CALLED__" + CPU.MethodInfoLabelGenerator.GetFullName(aMethod.MethodBase.DeclaringType));
        var xAsmMember = new CPU.DataMember(xName, (byte)0);
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
            new CPU.Comment(String.Format("Local {0}, Size {1}", xLocal.LocalIndex, xSize));
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

    protected MethodDefinition GetCecilMethodDefinitionForSymbolReading(MethodBase methodBase) {
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


  }
}
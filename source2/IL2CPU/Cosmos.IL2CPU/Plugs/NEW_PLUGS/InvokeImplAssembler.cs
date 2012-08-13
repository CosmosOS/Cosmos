using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.IL2CPU.IL;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86.IL;
using CPU = Cosmos.Assembler;
using MethodBase = System.Reflection.MethodBase;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.Plugs.NEW_PLUGS {
  public class InvokeImplAssembler: AssemblerMethod {
    private uint GetArgumentStartOffset(MethodBase aMethod, int aArgument) {
      return 0;
    }

    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      var xAssembler = (Cosmos.Assembler.Assembler)aAssembler;
      var xMethodInfo = (Cosmos.IL2CPU.MethodInfo)aMethodInfo;
      var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
      if (xMethodBaseAsInfo.ReturnType != typeof(void)) {
        throw new Exception("Events with return type not yet supported!");
      }
      new Comment("XXXXXXX");
      new CPUx86.Xchg { DestinationReg = CPUx86.Registers.BX, SourceReg = CPUx86.Registers.BX, Size = 16 };

      /*
 * EAX contains the GetInvocationList() array at the index at which it was last used
 * EDX contains the index at which the EAX is
 * EBX contains the number of items in the array
 * ECX contains the argument size
 */
      new CPUx86.ClrInterruptFlag();
      new CPU.Label(".DEBUG");
      //new CPU.Label("____DEBUG_FOR_MULTICAST___");
      new CPU.Comment("move address of delgate to eax");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = Ldarg.GetArgumentDisplacement(xMethodInfo, 0) };
      var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
      new CPU.Comment("push address of delgate to stack");
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };//addrof this
      new CPUx86.Call { DestinationLabel = CPU.LabelName.Get(xGetInvocationListMethod) };
      new CPU.Comment("get address from return value -> eax");
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
      ;//list
      new CPU.Comment("eax+=8 is where the offset where an array's count is");
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };//addrof list.count??
      new CPU.Comment("store count in ebx");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };//list.count
      new CPU.Comment("eax+=8 is where the offset where an array's items start");
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };//why? -- start of list i think? MtW: the array's .Length is at +8
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceValue = 0 };
      new CPU.Comment("ecx = ptr to delegate object");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = Ldarg.GetArgumentDisplacement(xMethodInfo, 0) };//addrof the delegate
      new CPU.Comment("ecx points to the size of the delegated methods arguments");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true, SourceDisplacement = Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ArgSize$$") };//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
      new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
      ;//make sure edx is 0
      new CPU.Label(".BEGIN_OF_LOOP");
      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBX };//are we at the end of this list
      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".END_OF_INVOKE_" };//then we better stop
      new CPUx86.Pushad();
      new CPU.Comment("esi points to where we will copy the methods argumetns from");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESP };
      new CPU.Comment("edi = ptr to delegate object");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = Ldarg.GetArgumentDisplacement(xMethodInfo, 0) };
      new CPU.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true, SourceDisplacement = Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target") };
      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDI, SourceValue = 0 };
      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = ".NO_THIS" };
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EDI };

      new CPU.Label(".NO_THIS");

      new CPU.Comment("make space for us to copy the arguments too");
      new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.ECX };
      new CPU.Comment("move the current delegate to edi");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
      new CPU.Comment("move the methodptr from that delegate to edi ");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true, SourceDisplacement = Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr") };//
      new CPU.Comment("save methodptr on the stack");
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EDI };
      new CPU.Comment("move location to copy args to");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESP };
      new CPU.Comment("get above the saved methodptr");
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 4 };
      //we allocated the argsize on the stack once, and it we need to get above the original args
      new CPU.Comment("we allocated argsize on the stack once");
      new CPU.Comment("add 32 for the Pushad + 16 for the current stack + 4 for the return value");
      //uint xToAdd = 32; // skip pushad data
      //xToAdd += 4; // method pointer
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EBP };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 8 }; // ebp+8 is first argument
      new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDI };
      new CPUx86.Call { DestinationReg = CPUx86.Registers.EDI };
      new CPU.Comment("store return -- return stored into edi after popad");
      new CPU.Comment("edi = ptr to delegate object");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = Ldarg.GetArgumentDisplacement(xMethodInfo, 0) };
      new CPU.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true, SourceDisplacement = Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target") };//i really dont get the +12, MtW: that's for the object header
      new CPU.Label(".noTHIStoPop");
      new CPUx86.Popad();
      new CPUx86.INC { DestinationReg = CPUx86.Registers.EDX };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
      new CPUx86.Jump { DestinationLabel = ".BEGIN_OF_LOOP" };
      new CPU.Label(".END_OF_INVOKE_");
      new CPU.Comment("get the return value");
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = Ldarg.GetArgumentDisplacement(xMethodInfo, 0) };//addrof the delegate
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true, SourceDisplacement = Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$") };
      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceValue = 0 };
      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".noReturn" };
      //may have to expand the return... idk
      new CPUx86.Xchg { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceReg = CPUx86.Registers.EDX };
      new CPUx86.Xchg { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
      new CPUx86.Xchg { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EDX };
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };//ebp
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 12, SourceReg = CPUx86.Registers.EDI };
      new CPU.Label(".noReturn");
      new CPUx86.Sti();
    }

    #region OLD attempt
    //    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
//      new CPUx86.Xchg { DestinationReg = CPUx86.Registers.BX, SourceReg = CPUx86.Registers.BX, Size = 16 };
//      new CPUx86.Noop();
//      var xAssembler = (CosmosAssembler)aAssembler;
//      var xMethodInfo = (Cosmos.IL2CPU.MethodInfo)aMethodInfo;
//      var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
//      if (xMethodBaseAsInfo.ReturnType != typeof(void)) {
//        throw new Exception("Events with return type not yet supported!");
//      }
//      new Comment("XXXXXXX");
//      //new CPUx86.Halt();
//      // param 0 is instance of eventhandler
//      // param 1 is sender
//      // param 2 is eventargs
//      //Ldarg.Ldfld(aAssembler, aMethodInfo.TypeInfo, "System.Object System.Delegate._target");
//      //new Label(".LoadMethodParams");
//      //for (int i = 1; i < aMethodInfo.Arguments.Length; i++) {
//      //    Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[i]);
//      //}
//      //new Label(".LoadMethodPointer");
//      //Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
//      //Ldarg.Ldfld(aAssembler, aMethodInfo.TypeInfo, "System.IntPtr System.Delegate._methodPtr");
//      //new CPUx86.Pop("eax");
//      //new CPUx86.Call(CPUx86.Registers_Old.EAX);
//      //new CPUx86.Add("esp",
//      //               "4");
//      /*
//       * EAX contains the GetInvocationList() array at the index at which it was last used
//       * EDX contains the index at which the EAX is
//       * EBX contains the number of items in the array
//       * ECX contains the argument size
//       */
//      //new CPU.Label("____DEBUG_FOR_MULTICAST___");
//      //            new CPUx86.Cli();//DEBUG ONLY
//      new CPU.Comment("push address of delgate to stack");
//      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
//      new CPUx86.Call { DestinationLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName(xGetInvocationListMethod) };
//      new CPU.Comment("get address from return value -> eax");
//      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
//      ;//list
//      new CPU.Comment("eax+=8 is where the offset where an array's count is");
//      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };//addrof list.count??
//      new CPU.Comment("store count in ebx");
//      new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };//list.count
//      new CPU.Comment("eax+=8 is where the offset where an array's items start");
//      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };//why? -- start of list i think? MtW: the array's .Length is at +8
//      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceValue = 0 };
//      new CPU.Comment("ecx = ptr to delegate object");

////      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
//      // make ecx point to the size of arguments
//      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "$$ArgSize$$");
//      new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
//      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
//      new CPU.Comment("ecx points to the size of the delegated methods arguments");
//      new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
//      ;//make sure edx is 0
//      new CPU.Label(".BEGIN_OF_LOOP");
//      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBX };//are we at the end of this list
//      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".END_OF_INVOKE_" };//then we better stop
//      //new CPUx86.Compare("edx", 0);
//      //new CPUx86.JumpIfLessOrEqual(".noreturnYet");
//      //new CPUx86.Add("esp", 4);
//      //new CPU.Label(".noreturnYet");
//      //new CPU.Comment("space for the return value");
//      //new CPUx86.Pushd("0");
//      new CPUx86.Pushad();
//      new CPU.Comment("esi points to where we will copy the methods arguments from");
//      new CPUx86.Move { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESP };
//      new CPU.Comment("edi = ptr to delegate object");
//      new CPUx86.Pushad();
//      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target");
//      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDI };
//      new CPUx86.Popad();
//      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDI, SourceValue = 0 };
//      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = ".NO_THIS" };
//      new CPUx86.Push { DestinationReg = CPUx86.Registers.EDI };

//      new CPU.Label(".NO_THIS");

//      new CPU.Comment("make space for us to copy the arguments too");
//      new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EBX };
//      new CPU.Comment("move the current delegate to edi");
//      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
//      new CPU.Comment("move the methodptr from that delegate to the stack ");
//      new CPUx86.Pushad();
//      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr");
//      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESP };
      
//      new CPU.Comment("get above the saved methodptr");
//      new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 4 };
//      //we allocated the argsize on the stack once, and it we need to get above the original args
//      new CPU.Comment("we allocated argsize on the stack once");
//      new CPU.Comment("add 32 for the Pushad + 16 for the current stack + 4 for the return value");
//      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 52 };
//      new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
//      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDI };
//      new CPUx86.Call { DestinationReg = CPUx86.Registers.EDI };
//      new CPU.Comment("store return -- return stored into edi after popad");
//      //new CPUx86.Move("edx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
//      //new CPUx86.Move("edx", "[edx+" + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ReturnsValue$$"].Offset + 12) + "]");
//      //new CPUx86.Compare(Registers_Old.EDX, 0);
//      //new CPUx86.JumpIfEqual(".getReturn");
//      //new CPUx86.Move(Registers_Old.EAX, "[esp]");
//      //new CPUx86.Move("[esp+0x20]", Registers_Old.EAX);
//      //new CPU.Label(".getReturn");
//      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target");
//      // edi contains $this now
//      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDI };
//      //new CPUx86.Compare ("edi", "0");
//      //new CPUx86.JumpIfEqual(".noTHIStoPop");
//      //new CPUx86.Move("edx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
//      //new CPUx86.Move("edx", "[edx+" + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ReturnsValue$$"].Offset + 12) + "]");
//      //new CPUx86.Compare(Registers_Old.EDX, 0);
//      //new JumpIfNotEqual(".needToPopThis");
//      //new CPU.Comment("ecx = ptr to delegate object");
//      //new CPUx86.Move("ecx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
//      //new CPU.Comment("ecx points to the size of the delegated methods arguments");
//      //new CPUx86.Move("ecx", "[ecx + " + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) + "]");//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
//      //new CPUx86.Compare("ecx", "0");
//      //new CPUx86.JumpIfLessOrEqual(".noTHIStoPop");
//      //new CPU.Label(".needToPopThis");
//      //new CPUx86.Pop("edi");
//      //new CPUx86.Move("[esp]", "edi");
//      new CPU.Label(".noTHIStoPop");
//      new CPUx86.Popad();
//      new CPUx86.Inc { DestinationReg = CPUx86.Registers.EDX };
//      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
//      new CPUx86.Jump { DestinationLabel = ".BEGIN_OF_LOOP" };
//      new CPU.Label(".END_OF_INVOKE_");
//      new CPU.Comment("get the return value");
//      // TEMP!!!
//     // new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
//      // END OF TEMP!!
//      //new CPUx86.Pop("eax");
//      //Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
//      //Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$");
//      //new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
//      //if(xMethodInfo.
//      //new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceValue = 0 };
//      //new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".noReturn" };
//      ////may have to expand the return... idk
//      //new CPUx86.Xchg { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceReg = CPUx86.Registers.EDX };
//      //new CPUx86.Xchg { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
//      //new CPUx86.Xchg { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EDX };
//      //new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };//ebp
//      //new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 12, SourceReg = CPUx86.Registers.EDI };
//      new CPU.Label(".noReturn");
//      //            new CPUx86.Sti();
//      //#warning remove this ^ sti call when issue is fixed!!!
//      //MethodInfo.Arguments[0].
//      //            new CPUx86.Move("ebx", "[eax + " + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) + "]");

//      //new CPUx86.Move("eax", CPUx86.Registers_Old.
//      //var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
//      //xGetInvocationListMethod = typeof(Delegate).GetMethod("GetInvocationList");

//      //							new CPUx86.Pop(CPUx86.Registers_Old.EAX);
//      //new CPUx86.Move("esp", "ebp");

//      //new CPUx86.Push {
//      //  DestinationRef = Cosmos.Assembler.ElementReference.New(LdStr.GetContentsArrayName("Events not yet implemented"))
//      //};
//      //new CPUx86.Call {
//      //  DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(typeof(ExceptionHelper).GetMethod("ThrowNotImplemented", BindingFlags.Static | BindingFlags.Public))
//      //};
//      //      throw new NotImplementedException();

    //      }
    #endregion
  }
}

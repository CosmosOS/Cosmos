using System;

using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.X86.IL;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using MethodBase = System.Reflection.MethodBase;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class InvokeImplAssembler : AssemblerMethod
    {
        private uint GetArgumentStartOffset(MethodBase aMethod, int aArgument)
        {
            return 0;
        }

        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            var xAssembler = (Cosmos.Assembler.Assembler)aAssembler;
            var xMethodInfo = (Cosmos.IL2CPU.MethodInfo)aMethodInfo;
            var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
            if (xMethodBaseAsInfo.ReturnType != typeof(void))
            {
                throw new Exception("Events with return type not yet supported!");
            }
            XS.Comment("XXXXXXX");
            XS.Exchange(XSRegisters.BX, XSRegisters.BX);

            /*
       * EAX contains the GetInvocationList() array at the index at which it was last used
       * EDX contains the index at which the EAX is
       * EBX contains the number of items in the array
       * ECX contains the argument size
       */
            XS.ClearInterruptFlag();
            XS.Label(".DEBUG");
            //XS.Label("____DEBUG_FOR_MULTICAST___");
            XS.Comment("move address of delegate to eax");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));

            var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
            XS.Comment("push address of delgate to stack");
            XS.Push(XSRegisters.EAX);//addrof this
            XS.Call(LabelName.Get(xGetInvocationListMethod));
            XS.Comment("get address from return value -> eax");
            XS.Pop(XSRegisters.EAX);
            ;//list
            XS.Comment("eax+=8 is where the offset where an array's count is");
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
            XS.Add(XSRegisters.EAX, 8);//addrof list.Length
            XS.Comment("store count in ebx");
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, sourceIsIndirect: true);//list.count
            XS.Comment("eax+=8 is where the offset where an array's items start");
            XS.Add(XSRegisters.EAX, 8); // Put pointer at the first item in the list.
            XS.Set(XSRegisters.EDI, 0);
            XS.Comment("ecx = ptr to delegate object");
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));//addrof the delegate
            XS.Comment("ecx points to the size of the delegated methods arguments");
            XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceIsIndirect: true);
            XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ArgSize$$"));//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
            XS.Xor(XSRegisters.EDX, XSRegisters.EDX);
            ;//make sure edx is 0
            XS.Label(".BEGIN_OF_LOOP");
            {
                XS.Compare(XSRegisters.EDX, XSRegisters.EBX);//are we at the end of this list
                XS.Jump(CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, ".END_OF_INVOKE_");//then we better stop
                XS.PushAllRegisters();
                XS.Comment("esi points to where we will copy the methods argumetns from");
                XS.Set(XSRegisters.ESI, XSRegisters.ESP);
                XS.Comment("edi = ptr to delegate object");
                XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceIsIndirect: true); // dereference handle
                XS.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));
                XS.Compare(XSRegisters.EDI, 0);
                XS.Jump(CPUx86.ConditionalTestEnum.Zero, ".NO_THIS");
                XS.Push(XSRegisters.EDI);

                XS.Label(".NO_THIS");

                XS.Comment("make space for us to copy the arguments too");
                XS.Sub(XSRegisters.ESP, XSRegisters.ECX);
                XS.Comment("move the current delegate to edi");
                XS.Set(XSRegisters.EDI, XSRegisters.EAX, sourceIsIndirect: true);
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceIsIndirect: true); // dereference
                XS.Comment("move the methodptr from that delegate to edi ");
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr"));//
                XS.Comment("save methodptr on the stack");
                XS.Push(XSRegisters.EDI);
                XS.Comment("move location to copy args to");
                XS.Set(XSRegisters.EDI, XSRegisters.ESP);
                XS.Add(XSRegisters.EDI, 4);
                //new CPU.Comment("get above the saved methodptr");
                //XS.Sub(XSRegisters.ESP, 4);
                //we allocated the argsize on the stack once, and it we need to get above the original args
                XS.Comment("we allocated argsize on the stack once");
                XS.Comment("add 32 for the Pushad + 16 for the current stack + 4 for the return value");
                //uint xToAdd = 32; // skip pushad data
                //xToAdd += 4; // method pointer
                XS.Set(XSRegisters.ESI, XSRegisters.EBP);
                XS.Add(XSRegisters.ESI, 8); // ebp+8 is first argument
                new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
                XS.Pop(XSRegisters.EDI);
                XS.Label(".BeforeCall");
                XS.Call(XSRegisters.EDI);
                XS.Comment("store return -- return stored into edi after popad");
                XS.Comment("edi = ptr to delegate object");
                XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceIsIndirect: true); // dereference handle
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));//i really dont get the +12, MtW: that's for the object header
                XS.Label(".noTHIStoPop");
                XS.PopAllRegisters();
                XS.Increment(XSRegisters.EDX);
                XS.Add(XSRegisters.EAX, 4);
                XS.Jump(".BEGIN_OF_LOOP");
            }
            XS.Label(".END_OF_INVOKE_");
            XS.Comment("get the return value");
            XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));//addrof the delegate
            XS.Set(XSRegisters.EDX, XSRegisters.EDX, sourceIsIndirect: true); // dereference handle
            XS.Set(XSRegisters.EDX, XSRegisters.EDX, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$"));
            XS.Compare(XSRegisters.EDX, 0);
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, ".noReturn");
            //may have to expand the return... idk
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 8);
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 4);
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationIsIndirect: true);
            XS.Push(XSRegisters.EDX);//ebp
            XS.Set(XSRegisters.ESP, XSRegisters.EDI, destinationDisplacement: 12);
            XS.Label(".noReturn");
            XS.EnableInterrupts();
        }

        #region OLD attempt
        //    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
        //      XS.Xchg(XSRegisters.BX, XSRegisters.BX);
        //      XS.Noop();
        //      var xAssembler = (CosmosAssembler)aAssembler;
        //      var xMethodInfo = (Cosmos.IL2CPU.MethodInfo)aMethodInfo;
        //      var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
        //      if (xMethodBaseAsInfo.ReturnType != typeof(void)) {
        //        throw new Exception("Events with return type not yet supported!");
        //      }
        //      new Comment("XXXXXXX");
        //      //XS.Halt();
        //      // param 0 is instance of eventhandler
        //      // param 1 is sender
        //      // param 2 is eventargs
        //      //Ldarg.Ldfld(aAssembler, aMethodInfo.TypeInfo, "System.Object System.Delegate._target");
        //      //XS.Label(".LoadMethodParams");
        //      //for (int i = 1; i < aMethodInfo.Arguments.Length; i++) {
        //      //    Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[i]);
        //      //}
        //      //XS.Label(".LoadMethodPointer");
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
        //      //XS.Label("____DEBUG_FOR_MULTICAST___");
        //      //            new CPUx86.Cli();//DEBUG ONLY
        //      new CPU.Comment("push address of delgate to stack");
        //      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
        //      XS.Call(CPU.MethodInfoLabelGenerator.GenerateLabelName(xGetInvocationListMethod));
        //      new CPU.Comment("get address from return value -> eax");
        //      XS.Pop(XSRegisters.EAX);
        //      ;//list
        //      new CPU.Comment("eax+=8 is where the offset where an array's count is");
        //      XS.Add(XSRegisters.EAX, 8);//addrof list.count??
        //      new CPU.Comment("store count in ebx");
        //      XS.Mov(XSRegisters.EBX, XSRegisters.EAX, sourceIsIndirect: true);//list.count
        //      new CPU.Comment("eax+=8 is where the offset where an array's items start");
        //      XS.Add(XSRegisters.EAX, 8);//why? -- start of list i think? MtW: the array's .Length is at +8
        //      XS.Mov(XSRegisters.EDI, 0);
        //      new CPU.Comment("ecx = ptr to delegate object");

        ////      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      XS.Push(XSRegisters.EAX);
        //      // make ecx point to the size of arguments
        //      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "$$ArgSize$$");
        //      XS.Pop(XSRegisters.ECX);
        //      XS.Pop(XSRegisters.EAX);
        //      new CPU.Comment("ecx points to the size of the delegated methods arguments");
        //      XS.Xor(XSRegisters.EDX, XSRegisters.CPUx86.Registers.EDX);
        //      ;//make sure edx is 0
        //      XS.Label(".BEGIN_OF_LOOP");
        //      XS.Compare(XSRegisters.EDX, XSRegisters.CPUx86.Registers.EBX);//are we at the end of this list
        //      XS.Jump(ConditionalTestEnum.Equal, ".END_OF_INVOKE_");//then we better stop
        //      //new CPUx86.Compare("edx", 0);
        //      //new CPUx86.JumpIfLessOrEqual(".noreturnYet");
        //      //new CPUx86.Add("esp", 4);
        //      //XS.Label(".noreturnYet");
        //      //new CPU.Comment("space for the return value");
        //      //new CPUx86.Pushd("0");
        //      XS.Pushad();
        //      new CPU.Comment("esi points to where we will copy the methods arguments from");
        //      XS.Mov(XSRegisters.ESI, XSRegisters.CPUx86.Registers.ESP);
        //      new CPU.Comment("edi = ptr to delegate object");
        //      XS.Pushad();
        //      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target");
        //      XS.Pop(XSRegisters.EDI);
        //      XS.Popad();
        //      XS.Compare(XSRegisters.EDI, 0);
        //      XS.Jump(ConditionalTestEnum.Zero, ".NO_THIS");
        //      XS.Push(XSRegisters.EDI);

        //      XS.Label(".NO_THIS");

        //      new CPU.Comment("make space for us to copy the arguments too");
        //      XS.Sub(XSRegisters.ESP, XSRegisters.CPUx86.Registers.EBX);
        //      new CPU.Comment("move the current delegate to edi");
        //      XS.Mov(XSRegisters.EDI, XSRegisters.EAX, sourceIsIndirect: true);
        //      new CPU.Comment("move the methodptr from that delegate to the stack ");
        //      XS.Pushad();
        //      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr");
        //      XS.Mov(XSRegisters.EDI, XSRegisters.CPUx86.Registers.ESP);

        //      new CPU.Comment("get above the saved methodptr");
        //      XS.Add(XSRegisters.EDI, 4);
        //      //we allocated the argsize on the stack once, and it we need to get above the original args
        //      new CPU.Comment("we allocated argsize on the stack once");
        //      new CPU.Comment("add 32 for the Pushad + 16 for the current stack + 4 for the return value");
        //      XS.Add(XSRegisters.ESI, 52);
        //      new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        //      XS.Pop(XSRegisters.EDI);
        //      XS.Call(XSRegisters.EDI);
        //      new CPU.Comment("store return -- return stored into edi after popad");
        //      //new CPUx86.Move("edx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
        //      //new CPUx86.Move("edx", "[edx+" + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ReturnsValue$$"].Offset + 12) + "]");
        //      //new CPUx86.Compare(Registers_Old.EDX, 0);
        //      //new CPUx86.JumpIfEqual(".getReturn");
        //      //new CPUx86.Move(Registers_Old.EAX, "[esp]");
        //      //new CPUx86.Move("[esp+0x20]", Registers_Old.EAX);
        //      //XS.Label(".getReturn");
        //      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target");
        //      // edi contains $this now
        //      XS.Pop(XSRegisters.EDI);
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
        //      //XS.Label(".needToPopThis");
        //      //new CPUx86.Pop("edi");
        //      //new CPUx86.Move("[esp]", "edi");
        //      XS.Label(".noTHIStoPop");
        //      XS.Popad();
        //      XS.INC(XSRegisters.EDX);
        //      XS.Add(XSRegisters.EAX, 4);
        //      XS.Jump(".BEGIN_OF_LOOP");
        //      XS.Label(".END_OF_INVOKE_");
        //      new CPU.Comment("get the return value");
        //      // TEMP!!!
        //     // XS.Add(XSRegisters.ESP, 4);
        //      // END OF TEMP!!
        //      //new CPUx86.Pop("eax");
        //      //Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
        //      //Ldfld.DoExecute(xAssembler, xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$");
        //      //XS.Pop(XSRegisters.EDX);
        //      //if(xMethodInfo.
        //      //XS.Compare(XSRegisters.EDX, 0);
        //      //XS.Jump(ConditionalTestEnum.Equal, ".noReturn");
        //      ////may have to expand the return... idk
        //      //XS.Xchg(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 8);
        //      //XS.Xchg(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 4);
        //      //XS.Xchg(XSRegisters.EBP, XSRegisters.EDX, destinationIsIndirect: true);
        //      //XS.Push(XSRegisters.EDX);//ebp
        //      //XS.Mov(XSRegisters.ESP, XSRegisters.EDI, destinationDisplacement: 12);
        //      XS.Label(".noReturn");
        //      //            XS.Sti();
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

using System;
using System.Reflection;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using CPUAll = Cosmos.Assembler;

namespace Cosmos.IL2CPU.Plugs.Assemblers
{
    //TODO: This asm refs Hardware.. should not.. its a higher ring
    public class CPUUpdateIDTAsm : AssemblerMethod
    {
        private static MethodBase GetMethodDef(Assembly aAssembly, string aType, string aMethodName, bool aErrorWhenNotFound)
        {
            Type xType = aAssembly.GetType(aType, false);
            if (xType != null)
            {
                MethodBase xMethod = xType.GetMethod(aMethodName);
                if (xMethod != null)
                {
                    return xMethod;
                }
            }
            if (aErrorWhenNotFound)
            {
                throw new Exception("Method '" + aType + "::" + aMethodName + "' not found!");
            }
            return null;
        }

        private static MethodBase GetInterruptHandler(byte aInterrupt)
        {
            return GetMethodDef(typeof(Cosmos.Core.INTs).Assembly, typeof(Cosmos.Core.INTs).FullName
                , "HandleInterrupt_" + aInterrupt.ToString("X2"), false);
        }

        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            // IDT is already initialized but just for base hooks, and asm only.
            // ie Int 1, 3 and GPF
            // This routine updates the IDT now that we have C# running to allow C# hooks to handle
            // the other INTs

            // We are updating the IDT, disable interrupts
            XS.ClearInterruptFlag();

            for (int i = 0; i < 256; i++)
            {
                // These are already mapped, don't remap them.
                // Maybe in the future we can look at ones that are present
                // and skip them, but some we may want to overwrite anyways.
                if (i == 1 || i == 3)
                {
                    continue;
                }

                XS.Set(XSRegisters.EAX, "__ISR_Handler_" + i.ToString("X2"));
                XS.Set("_NATIVE_IDT_Contents", XSRegisters.AL, destinationDisplacement: (i*8) + 0);
                XS.Set("_NATIVE_IDT_Contents", XSRegisters.AH, destinationDisplacement: (i*8) + 1);
                XS.Set("_NATIVE_IDT_Contents", 0x8, destinationDisplacement: (i*8) + 2, size: XSRegisters.RegisterSize.Byte8);
                XS.Set("_NATIVE_IDT_Contents", 0x8E, destinationDisplacement: (i*8) + 5, size: XSRegisters.RegisterSize.Byte8);
                XS.ShiftRight(XSRegisters.EAX, 16);
                XS.Set("_NATIVE_IDT_Contents", XSRegisters.AL, destinationDisplacement: (i*8) + 6);
                XS.Set("_NATIVE_IDT_Contents", XSRegisters.AH, destinationDisplacement: (i*8) + 7);
            }

            XS.Jump("__AFTER__ALL__ISR__HANDLER__STUBS__");
            var xInterruptsWithParam = new[] {8, 10, 11, 12, 13, 14};
            for (int j = 0; j < 256; j++)
            {
                XS.Label("__ISR_Handler_" + j.ToString("X2"));
                XS.Call("__INTERRUPT_OCCURRED__");

                if (global::System.Array.IndexOf(xInterruptsWithParam, j) == -1)
                {
                    XS.Push(0);
                }
                XS.Push((uint) j);
                XS.PushAllRegisters();

                XS.Sub(XSRegisters.ESP, 4);
                XS.Set(XSRegisters.EAX, XSRegisters.ESP); // preserve old stack address for passing to interrupt handler

                // store floating point data
                XS.And(XSRegisters.ESP, 0xfffffff0); // fxsave needs to be 16-byte alligned
                XS.Sub(XSRegisters.ESP, 512); // fxsave needs 512 bytes
                XS.SSE.FXSave(XSRegisters.ESP, isIndirect: true); // save the registers
                XS.Set(XSRegisters.EAX, XSRegisters.ESP, destinationIsIndirect: true);

                XS.Push(XSRegisters.EAX); //
                XS.Push(XSRegisters.EAX); // pass old stack address (pointer to InterruptContext struct) to the interrupt handler

                XS.JumpToSegment(8, "__ISR_Handler_" + j.ToString("X2") + "_SetCS");
                XS.Label("__ISR_Handler_" + j.ToString("X2") + "_SetCS");
                MethodBase xHandler = GetInterruptHandler((byte) j);
                if (xHandler == null)
                {
                    xHandler = GetMethodDef(typeof(Cosmos.Core.INTs).Assembly, typeof(Cosmos.Core.INTs).FullName, "HandleInterrupt_Default", true);
                }
                XS.Call(CPUAll.LabelName.Get(xHandler));
                XS.Pop(XSRegisters.EAX);
                XS.SSE.FXRestore(XSRegisters.ESP, isIndirect: true);

                XS.Set(XSRegisters.ESP, XSRegisters.EAX); // this restores the stack for the FX stuff, except the pointer to the FX data
                XS.Add(XSRegisters.ESP, 4); // "pop" the pointer

                XS.PopAllRegisters();

                XS.Add(XSRegisters.ESP, 8);
                new CPUAll.Label("__ISR_Handler_" + j.ToString("X2") + "_END");
                XS.InterruptReturn();
            }
            XS.Label("__INTERRUPT_OCCURRED__");
            XS.Return();
            XS.Label("__AFTER__ALL__ISR__HANDLER__STUBS__");
            XS.Noop();
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPUx86.ConditionalTestEnum.Zero, ".__AFTER_ENABLE_INTERRUPTS");

            // reload interrupt list
            XS.Set(XSRegisters.EAX, "_NATIVE_IDT_Pointer");
            XS.Set("static_field__Cosmos_Core_CPU_mInterruptsEnabled", 1, destinationIsIndirect: true);
            XS.LoadIdt(XSRegisters.EAX, isIndirect: true);
            // Reenable interrupts
            XS.EnableInterrupts();

            XS.Label(".__AFTER_ENABLE_INTERRUPTS");
        }
    }
}

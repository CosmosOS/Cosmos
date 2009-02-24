using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Y86 {
        public enum Flags { 
            Zero, NotZero, Equal, NotEqual // Zero is synonym for Equal
        };

        //TODO: Add registers as needed, not all are here yet
        public RegisterEAX EAX = RegisterEAX.Instance;
        public RegisterAL AL = RegisterAL.Instance;
        
        public RegisterEBX EBX = RegisterEBX.Instance;
        public RegisterECX ECX = RegisterECX.Instance;

        public RegisterEDX EDX = RegisterEDX.Instance;
        public RegisterAX AX = RegisterAX.Instance;
        public RegisterDX DX = RegisterDX.Instance;

        public RegisterEBP EBP = RegisterEBP.Instance;
        public RegisterESP ESP = RegisterESP.Instance;
        public RegisterESI ESI = RegisterESI.Instance;

        public readonly Ports Port = new Ports();
        public readonly Memory Memory = new Memory();

        public string Label {
            set { 
                new Indy.IL2CPU.Assembler.Label(value); 
            }
        }

        private uint mLabelCounter = 0;
        public string NewLabel() {
            mLabelCounter++;
            return GetType().Name + mLabelCounter.ToString("X8").ToUpper();
        }

        public void Call(string aLabel) {
            new X86.Call { DestinationLabel = aLabel };
        }


        public void Define(string aSymbol) {
            new Define(aSymbol);
        }

        public void IfDefined(string aSymbol) {
            new IfDefined(aSymbol);
        }

        public void EndIfDefined() {
            new EndIfDefined();
        }

        public void CallIf(Flags aFlags, string aLabel) {
            CallIf(aFlags, aLabel, "");
        }

        public void CallIf(Flags aFlags, string aLabel, string aJumpAfter) {
            // TODO: This is inefficient - lots of jumps
            // Maybe make an invert function for Flags
            var xLabelIf = NewLabel();
            var xLabelExit = NewLabel();

            JumpIf(aFlags, xLabelIf);
            Jump(xLabelExit);

            Label = xLabelIf;
            Call(aLabel);
            if (aJumpAfter != "") {
                Jump(aJumpAfter);
            }

            Label = xLabelExit;
        }

        public void Jump(string aLabel) {
            new X86.Jump { DestinationLabel = aLabel };
        }

        public void JumpIf(Flags aFlags, string aLabel) {
            switch (aFlags) {
                case Flags.Zero:
                case Flags.Equal:
                    new X86.ConditionalJump { Condition = X86.ConditionalTestEnum.Zero, DestinationLabel = aLabel };
                    break;
                case Flags.NotZero:
                case Flags.NotEqual:
                    new X86.ConditionalJump { Condition = X86.ConditionalTestEnum.NotZero, DestinationLabel = aLabel };
                    break;
            }
        }

        public void PopAll32() {
            new Popad();
        }

        public void PushAll32() {
            new Pushad();
        }
        
        public void Return() {
            new X86.Return();
        }
        
        public void Return(UInt16 aBytes) {
            new X86.Return { DestinationValue = aBytes };
        }

        public void EnableInterrupts() {
            new X86.Sti();
        }

        public void DisableInterrupts() {
            new X86.ClrInterruptFlag();
        }

        public ElementReference Reference(string aDataName) {
            return new ElementReference(aDataName);
        }
    }
}

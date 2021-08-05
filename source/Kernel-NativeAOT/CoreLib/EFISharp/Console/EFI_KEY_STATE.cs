using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EFI_KEY_STATE
    {
        //Reflects the currently pressed shift modifiers for the input device. The returned value is valid only if the high order bit has been set.
        public readonly KeyShiftState KeyShiftState;
        //Reflects the current internal state of various toggled attributes. The returned value is valid only if the high order bit has been set.
        public readonly EFI_KEY_TOGGLE_STATE KeyToggleState;

        public EFI_KEY_STATE(KeyShiftState shiftState, EFI_KEY_TOGGLE_STATE toggleState)
        {
            KeyShiftState = shiftState;
            KeyToggleState = toggleState;
        }
    }
}

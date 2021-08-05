using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EFI_KEY_DATA
    {
        /// <summary>The EFI scan code and Unicode value returned from the input device.</summary>
        public readonly EFI_INPUT_KEY Key;
        /// <summary>The current state of various toggled attributes as well as input modifier values.</summary>
        public readonly EFI_KEY_STATE KeyState;

        public EFI_KEY_DATA(EFI_INPUT_KEY key, EFI_KEY_STATE keyState)
        {
            Key = key;
            KeyState = keyState;
        }
    }
}

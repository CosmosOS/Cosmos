using System;

namespace EfiSharp
{
    [Flags]
    public enum EFI_KEY_TOGGLE_STATE : byte
    {
        EFI_TOGGLE_STATE_VALID = 0x80,

        /// <summary>
        /// If enabled the instance of EFI_SIMPLE_INPUT_EX_PROTOCOL that returned this supports the ability to return partial keystrokes.
        /// <para>Therefore the ReadKeyStrokeEx function will allow the return of incomplete keystrokes such as the holding down of certain</para>
        /// <para>keys which are expressed as a part of KeyState when there is no Key data.</para>
        /// </summary>
        EFI_KEY_STATE_EXPOSED = 0x40, 
        EFI_SCROLL_LOCK_ACTIVE = 0x01, 
        EFI_NUM_LOCK_ACTIVE = 0x02, 
        EFI_CAPS_LOCK_ACTIVE = 0x04
    }
}

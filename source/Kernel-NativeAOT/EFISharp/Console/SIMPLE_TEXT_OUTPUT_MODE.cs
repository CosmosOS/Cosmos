using System.Runtime.InteropServices;

namespace EfiSharp
{
    /// <summary>Stores information regarding the output devices(s) mode and its cursor.</summary>
    /// <remarks>This fields here are readonly and can only be modified with their respective functions in <see cref="EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL"/>.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SIMPLE_TEXT_OUTPUT_MODE
    {
        /// <summary>The number of modes supported by QueryMode() and SetMode(). Can not be changed.</summary>
        public readonly int MaxMode;
        // current settings
        /// <summary>The text mode of the output device(s). Can only be changed though <see cref="EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.SetMode"/>.</summary>
        public readonly int Mode;
        /// <summary>The current character output attribute. Can only be changed by moving the cursor with <see cref="EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.SetCursorPosition"/></summary>
        public readonly int Attribute;
        /// <summary>The cursor’s column. Can only be changed though <see cref="EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.SetCursorPosition"/>.</summary>
        public readonly int CursorColumn;
        /// <summary>The cursor’s row. Can only be changed though <see cref="EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.SetCursorPosition"/>.</summary>
        public readonly int CursorRow;
        /// <summary>The cursor is currently visible or not. Can only be changed though <see cref="EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.EnableCursor"/>.</summary>
        public readonly bool CursorVisible;
    }
}

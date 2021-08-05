using System;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    /// <summary>This protocol is used to control text-based output devices.</summary>
    /// <remarks>This is the minimum required protocol for any handle supplied as the ConsoleOut or StandardError device.
    /// <para>In addition, the minimum supported text mode of such devices is at least 80 x 25 characters.</para>
    /// <para>A video device that only supports graphics mode is required to emulate text mode functionality.</para></remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL
    {
        private static readonly EFI_GUID EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL_GUID =
            new(0x387477c2, 0x69c7, 0x11d2, 0x8e, 0x39, 0x00, 0xa0, 0xc9, 0x69, 0x72, 0x3b);

        private readonly IntPtr _pad1;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, char*, EFI_STATUS> _outputString;
        private readonly IntPtr _pad2;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, nuint, nuint*, nuint*, EFI_STATUS> _queryMode;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, nuint, EFI_STATUS> _setMode;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, nuint, EFI_STATUS> _setAttribute;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, EFI_STATUS> _clearScreen;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, nuint, nuint, EFI_STATUS> _setCursorPosition;
        private readonly delegate*<EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL*, bool, EFI_STATUS> _enableCursor;
        public readonly SIMPLE_TEXT_OUTPUT_MODE* Mode;

        /// <summary>Writes <paramref name="buf"/> to the current cursor location on the output device(s).</summary>
        /// <remarks>
        /// For each char in <paramref name="buf"/> the cursor moves according to the rules listed below:
        /// <list type="table">
        ///     <item>
        ///         <term>null (0x0)</term>
        ///         <description>Ignore the character, and do not move the cursor.</description>
        ///     </item>
        ///     <item>
        ///         <term>backspace (0x8)</term>
        ///         <description>If the cursor is not at the left edge of the display, then move the cursor left one column.</description>
        ///     </item>
        ///     <item>
        ///         <term>line feed (0xA)</term>
        ///         <description>If the cursor is at the bottom of the display, then scroll the display one row, and do not update the cursor position. Otherwise, move the cursor down one row.</description>
        ///     </item>
        ///     <item>
        ///         <term>carriage return (0xD)</term>
        ///         <description>Move the cursor to the beginning of the current row.</description>
        ///     </item>
        ///     <item>
        ///         <term>Other</term>
        ///         <description>
        ///             Print the character at the current cursor position and move the cursor right one column.
        ///             <para>If this moves the cursor past the right edge of the display, then the line should wrap to the beginning of the next line.</para>
        ///             <para>This is equivalent to inserting a CR and an LF.</para>
        ///             <para>Note that if the cursor is at the bottom of the display, and the line wraps, then the display will be scrolled one line.</para>
        ///         </description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="buf">Must be a null terminated buffer containing only supported characters, chars in https://en.wikipedia.org/wiki/Basic_Latin_(Unicode_block) and those
        /// in <see cref="EFIOutputRequiredChars"/> are supported at a minimum.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description><paramref name="buf"/> was output to the device.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device reported an error while attempting to output <paramref name="buf"/>.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description><paramref name="buf"/> was output to the device.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_WARN_UNKNOWN_GLYPH"/></term>
        ///         <description><paramref name="buf"/> was output to the device.</description>
        ///     </item>
        ///     <!-- Non spec returns -->
        ///       <item>
        ///         <term><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/></term>
        ///         <description><paramref name="buf"/> was null.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS OutputString(char* buf)
        {
            if (buf == null) return EFI_STATUS.EFI_INVALID_PARAMETER;

            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                return _outputString(_this, buf);
            }
        }

        /// <summary>Writes <paramref name="str"/> to the current cursor location on the output device(s).</summary>
        /// <remarks>
        /// For each char in <paramref name="str"/> the cursor moves according to the rules listed below:
        /// <list type="table">
        ///     <item>
        ///         <term>null (0x0)</term>
        ///         <description>Ignore the character, and do not move the cursor.</description>
        ///     </item>
        ///     <item>
        ///         <term>backspace (0x8)</term>
        ///         <description>If the cursor is not at the left edge of the display, then move the cursor left one column.</description>
        ///     </item>
        ///     <item>
        ///         <term>line feed (0xA)</term>
        ///         <description>If the cursor is at the bottom of the display, then scroll the display one row, and do not update the cursor position. Otherwise, move the cursor down one row.</description>
        ///     </item>
        ///     <item>
        ///         <term>carriage return (0xD)</term>
        ///         <description>Move the cursor to the beginning of the current row.</description>
        ///     </item>
        ///     <item>
        ///         <term>Other</term>
        ///         <description>
        ///             Print the character at the current cursor position and move the cursor right one column.
        ///             <para>If this moves the cursor past the right edge of the display, then the line should wrap to the beginning of the next line.</para>
        ///             <para>This is equivalent to inserting a CR and an LF.</para>
        ///             <para>Note that if the cursor is at the bottom of the display, and the line wraps, then the display will be scrolled one line.</para>
        ///         </description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="str">Must be a string containing only supported characters, chars in https://en.wikipedia.org/wiki/Basic_Latin_(Unicode_block) and those
        /// in <see cref="EFIOutputRequiredChars"/> are supported at a minimum.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description><paramref name="buf"/> was output to the device.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device reported an error while attempting to output <paramref name="buf"/>.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description><paramref name="buf"/> was output to the device.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_WARN_UNKNOWN_GLYPH"/></term>
        ///         <description><paramref name="buf"/> was output to the device.</description>
        ///     </item>
        ///     <!-- Non spec returns -->
        ///       <item>
        ///         <term><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/></term>
        ///         <description><paramref name="buf"/> was null.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS OutputString(string str)
        {
            if (str == null) return EFI_STATUS.EFI_INVALID_PARAMETER;
            fixed (char* pStr = str)
            {
                return OutputString(pStr);
            }
        }

        /// <summary>Returns information for an available text mode that the output device(s) supports.</summary>
        /// <remarks>
        /// <para>It is required that all output devices support at least 80x25 text mode. This mode is defined to be mode 0.</para>
        /// <para>If the output devices support 80x50, that is defined to be mode 1.</para>
        /// <para>All other text dimensions supported by the device will follow as modes 2 and above.</para>
        /// <para>If an output device supports modes 2 and above, but does not support 80x50, then querying for mode 1 will return <see cref="EFI_STATUS.EFI_UNSUPPORTED"/>.</para>
        /// </remarks>
        /// <param name="modeNumber">The mode number to return information on.</param>
        /// <param name="columns">Returns the number of columns of the text output device for the requested <paramref name="modeNumber"/>.</param>
        /// <param name="rows">Returns the number of rows of the text output device for the requested <paramref name="modeNumber"/>.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The requested information about <paramref name="modeNumber"/> was returned.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device had an error and could not complete the request.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The <paramref name="modeNumber"/> was not valid.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS QueryMode(nuint modeNumber, out nuint columns, out nuint rows)
        {
            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                fixed (nuint* pColumns = &columns, pRows = &rows)
                {
                    return _queryMode(_this, modeNumber, pColumns, pRows);
                }
            }
        }

        /// <summary>Sets the output device(s) to a specified mode.</summary>
        /// <remarks>On success the device is in the geometry for the requested mode, and the device has been cleared to the current background color with the cursor at (0,0).</remarks>
        /// <param name="modeNumber">The text mode to set.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The requested <paramref name="modeNumber"/> was set.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device had an error and could not complete the request.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The <paramref name="modeNumber"/> was not valid.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS SetMode(nuint modeNumber)
        {
            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                return _setMode(_this, modeNumber);
            }
        }

        /// <summary>Sets the background and foreground colors for the <see cref="OutputString(char*)"/>, <see cref="OutputString(string)"/> and <see cref="ClearScreen"/> functions.</summary>
        /// <remarks>The color mask can be set even when the device is in an invalid text mode.
        /// <para>Devices supporting a different number of text colors are required to emulate the above colors to the best of the device’s capabilities.</para></remarks>
        /// <param name="attribute">Attribute is processed as two nibbles, the lower nibble is for the text/foreground colour and can be any
        /// <para>colour in System.Console.ConsoleColor, however some of the names are different in the uefi spec.</para>
        /// <para>The upper nibble is for the background colour and can only be between 0 and 7, i.e. the first 8 colours in System.Console.ConsoleColor.</para></param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The requested <paramref name="attribute"/> was set.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device had an error and could not complete the request.</description>
        ///     </item>
        /// </list>
        /// </returns>
        //TODO Check if this function can accept both colours directly, add an internal enum that matches https://uefi.org/sites/default/files/resources/UEFI%20Spec%202.8B%20May%202020.pdf#page=524&zoom=auto,-205,340?
        public EFI_STATUS SetAttribute(nuint attribute)
        {
            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                return _setAttribute(_this, attribute);
            }
        }

        //TODO Support Std Error
        /// <summary>Clears the output device(s) display to the currently selected background color.</summary>
        /// <remarks>The cursor position is set to (0, 0).
        /// <!--<para>It is not recommended to use this function on output messages to StandardError.</para>--></remarks>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The operation completed successfully.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device had an error and could not complete the request.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The output device is not in a valid text mode.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS ClearScreen()
        {
            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                return _clearScreen(_this);
            }
        }

        //TODO Support Std Error
        /// <summary>Sets the current coordinates of the cursor position.</summary>
        /// <remarks>The upper left corner of the screen is defined as coordinate (0, 0).
        /// <!--<para>It is not recommended to use this function on output messages to StandardError.</para>--></remarks>
        /// <param name="column">The column to set the cursor to. Must be greater than or equal to zero and less than the number of columns returned by <see cref="QueryMode"/>.</param>
        /// <param name="row">The row to set the cursor to. Must be greater than or equal to zero and less than the number of rows returned by <see cref="QueryMode"/>.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The operation completed successfully.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device had an error and could not complete the request.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The output device is not in a valid text mode, or the cursor position is invalid for the current mode. See <see cref="QueryMode"/>.</description>
        ///     </item>
        /// </list>
        /// </returns>
        /// 
        public EFI_STATUS SetCursorPosition(nuint column, nuint row)
        {
            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                return _setCursorPosition(_this, column, row);
            }
        }

        /// <summary>Makes the cursor visible or invisible.</summary>
        /// <param name="visible">If True, the cursor is set to be visible. If False, the cursor is set to be invisible.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The operation completed successfully.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The device had an error and could not complete the request or the device does not support changing the cursor mode.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The output device does not support visibility control of the cursor.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS EnableCursor(bool visible)
        {
            fixed (EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* _this = &this)
            {
                return _enableCursor(_this, visible);
            }
        }
    }
}

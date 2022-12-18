using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    // Dont hold state here. This is a raw to hardware class. Virtual screens should be done
    // by memory moves
    public class TextScreen : TextScreenBase
    {
        protected byte Color = 0x0F; // White
        protected ushort mBackgroundClearCellValue;
        protected ushort mTextClearCellValue;
        public uint mRow2Addr;
        public uint mScrollSize;
        protected int mCursorSize = 25; // 25 % as C# Console class
        protected bool mCursorVisible = true;

        public MemoryBlock08 mRAM;

        /// <summary>
        /// Memory.
        /// </summary>
        public MemoryBlock Memory = new MemoryBlock(0xB8000, 80 * 25 * 2);
        // These should probably move to a VGA class later, or this class should be remade into a VGA class
        /// <summary>
        /// Misc. output.
        /// </summary>
        public const int MiscOutput = 0x03C2;
        /// <summary>
        /// First IOPort index.
        /// </summary>
        public const int Idx1 = 0x03C4;
        /// <summary>
        /// First IOPort data.
        /// </summary>
        public const int Data1 = 0x03C5;
        /// <summary>
        /// Second IOPort index.
        /// </summary>
        public const int Idx2 = 0x03CE;
        /// <summary>
        /// Second IOPort data.
        /// </summary>
        public const int Data2 = 0x03CF;
        /// <summary>
        /// Third IOPort index.
        /// </summary>
        public const int Idx3 = 0x03D4;
        /// <summary>
        /// Third IOPort data.
        /// </summary>
        public const int Data3 = 0x03D5;

        /// <summary>
        /// Creat new instance of the <see cref="TextScreen"/> class.
        /// </summary>
        public TextScreen()
        {
            mRAM = Memory.Bytes;
            // Set the Console default colors: White foreground on Black background, the default value of mClearCellValue is set there too as it is linked with the Color
            SetColors(ConsoleColor.White, ConsoleColor.Black);
            mBackgroundClearCellValue = mTextClearCellValue;
            mRow2Addr = (uint)(Cols * 2);
            mScrollSize = (uint)(Cols * (Rows - 1) * 2);
            SetCursorSize(mCursorSize);
            SetCursorVisible(mCursorVisible);
            TextScreenHelpers.Debug("End of TextScreen..ctor");
        }

        public void UpdateWindowSize()
        {
            Memory = new Cosmos.Core.MemoryBlock(0xB8000, (uint)(Cols * Rows * 2));
            mRAM = Memory.Bytes;
            mScrollSize = (uint)(Cols * (Rows - 1) * 2);
            mRow2Addr = (uint)(Cols * 2);
        }

        public override ushort Rows { set; get; } = 25;
        public override ushort Cols { set; get; } = 80;

        /// <summary>
        /// Clear text screen.
        /// </summary>
        public override void Clear()
        {
            TextScreenHelpers.Debug("Clearing screen with value ");
            TextScreenHelpers.DebugNumber(mTextClearCellValue);
            Memory.Fill(mTextClearCellValue);
            mBackgroundClearCellValue = mTextClearCellValue;
        }

        /// <summary>
        /// Scroll screen up.
        /// </summary>
        public override void ScrollUp()
        {
            Memory.MoveDown(0, mRow2Addr, mScrollSize);
            Memory.Fill(mScrollSize, mRow2Addr, mBackgroundClearCellValue);
        }

        public override byte this[int aX, int aY]
        {
            get
            {
                var xScreenOffset = (uint)((aX + aY * Cols) * 2);
                return (byte)mRAM[xScreenOffset];
            }
            set
            {
                var xScreenOffset = (uint)((aX + aY * Cols) * 2);
                mRAM[xScreenOffset] = value;
                mRAM[xScreenOffset + 1] = Color;
            }
        }

        /// <summary>
        /// Set screen foreground and background colors.
        /// </summary>
        /// <param name="aForeground">Foreground color.</param>
        /// <param name="aBackground">Background color.</param>
        public override void SetColors(ConsoleColor aForeground, ConsoleColor aBackground)
        {
            //Color = (byte)((byte)(aForeground) | ((byte)(aBackground) << 4));
            // TODO: Use Real Mode to clear in Mode Control Register Index 10
            //       the third bit to disable blinking and use the seventh bit
            //       as the bright bit on background color for brighter colors :)
            Color = (byte)(((byte)aForeground | ((byte)aBackground << 4)) & 0x7F);

            // The Color | the NUL character this is used to Clear the Screen
            mTextClearCellValue = (ushort)(Color << 8 | 0x00);
        }

        /// <summary>
        /// Set cursor position.
        /// </summary>
        /// <param name="aX">A position on X axis.</param>
        /// <param name="aY">A position on Y axis.</param>
        public override void SetCursorPos(int aX, int aY)
        {
            char xPos = (char)(aY * Cols + aX);
            // Cursor low byte to VGA index register
            IOPort.Write8(Idx3, 0x0F);
            IOPort.Write8(Data3, (byte)(xPos & 0xFF));
            // Cursor high byte to VGA index register
            IOPort.Write8(Idx3, 0x0E);
            IOPort.Write8(Data3, (byte)(xPos >> 8));
        }

        /// <summary>
        /// Get screen color.
        /// </summary>
        /// <returns>byte value.</returns>
        public override byte GetColor()
        {
            return Color;
        }

        /// <summary>
        /// Get cursor size.
        /// </summary>
        /// <returns>int value.</returns>
        public override int GetCursorSize()
        {
            return mCursorSize;
        }

        /// <summary>
        /// Set cursor size.
        /// </summary>
        /// <param name="value">Size value.</param>
        public override void SetCursorSize(int value)
        {
            mCursorSize = value;
            TextScreenHelpers.Debug("Changing cursor size to", value, "%");
            // We need to transform value from a percentage to a value from 15 to 0
            value = 16 - 16 * value / 100;
            // This is the case in which value is in reality 1% and a for a truncation error we get 16 (invalid value)
            if (value >= 16)
            {
                value = 15;
            }
            TextScreenHelpers.Debug("verticalSize is", value);
            // Cursor Vertical Size Register here a value between 0x00 and 0x0F must be set with 0x00 meaning maximum size and 0x0F minimum
            IOPort.Write8(Idx3, 0x0A);
            IOPort.Write8(Data3, (byte)value);
            // Cursor Horizontal Size Register we set it to 0x0F (100%) as a security measure is probably so already
            IOPort.Write8(Idx3, 0x0B);
            IOPort.Write8(Data3, 0x0F);
        }

        /// <summary>
        /// Check if cursor is visible.
        /// </summary>
        /// <returns>bool value.</returns>
        public override bool GetCursorVisible()
        {
            return mCursorVisible;
        }

        /// <summary>
        /// Set cursor visibilty.
        /// </summary>
        /// <param name="value">TRUE - visible.</param>
        public override void SetCursorVisible(bool value)
        {
            mCursorVisible = value;

            // The VGA Cursor is disabled when the value is 1 and enabled when is 0 so we need to invert 'value', sadly the ConvertToByte() function is not working
            // so we need to do the if by hand...
            byte cursorDisable = (byte)(mCursorVisible ? 0 : 1);

            // Cursor Vertical Size Register if the bit 5 is set to 1 the cursor is disabled, if 0 is enabled
            IOPort.Write8(Idx3, 0x0A);
            IOPort.Write8(Data3, (byte)(IOPort.Read8(Data3) | (byte)(cursorDisable << 5)));
            SetCursorSize(mCursorSize);
        }
    }
}

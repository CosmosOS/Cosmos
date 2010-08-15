using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Hardware {
    // Dont hold state here. This is a raw to hardware class. Virtual screens should be done
    // by memory moves
    public class TextScreen {
        protected byte Color = 0x0F; // White

        protected Core.IOGroup.TextScreen IOGroup = Core.Global.BaseIOGroups.TextScreen;
        protected MemoryBlock08 mMemory08;

        public TextScreen() {
            mMemory08 = IOGroup.Memory.Bytes;
        }

        public int Rows { get { return 25; } }
        public int Cols { get { return 80; } }

        public void Clear() {
            // Empty + White + Empty + White
            UInt32 xData = 0x000F000F;
            IOGroup.Memory.Fill(0, (uint)(Cols * Rows * 2 / 4), xData);
        }

        public void ScrollUp() {
            IOGroup.Memory.MoveDown(0, (uint)Cols, (uint)(Cols * (Rows - 1) * 2 / 4));
        }

        public char this[int aX, int aY] {
            get {
                UInt32 xScreenOffset = (UInt32)((aX + aY * Cols) * 2);
                return (char)mMemory08[xScreenOffset];
            }
            set {
                var xScreenOffset = (UInt32)((aX + aY * Cols) * 2);
                mMemory08[xScreenOffset] = (byte)value;
                mMemory08[xScreenOffset + 1] = Color;
            }
        }

        public void SetColors(ConsoleColor aForeground, ConsoleColor aBackground) {
            Color = (byte)((byte)(aForeground) | ((byte)(aBackground) << 4));
        }

        public void SetCursorPos(int aX, int aY) {
            char xPos = (char)((aY * Cols) + aX);
            // Cursor low byte to VGA index register
            IOGroup.Idx3.Byte = 0x0F;
            IOGroup.Data3.Byte = (byte)(xPos & 0xFF);
            // Cursor high byte to VGA index register
            IOGroup.Idx3.Byte = 0x0E;
            IOGroup.Data3.Byte = (byte)(xPos >> 8);
        }

    }
}

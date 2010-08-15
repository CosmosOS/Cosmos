using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Hardware {
    // Dont hold state here. This is a raw to hardware class. Virtual screens should be done
    // by memory moves
    public class TextScreen {
        protected const byte DefaultColor = 15; // White
        protected bool mInitialized = false;
        protected byte Color;

        protected Core.IOGroup.TextScreen IOGroup = Core.Global.BaseIOGroups.TextScreen;
        protected MemoryBlock08 mMemory08;

        public TextScreen() {
            mMemory08 = IOGroup.Memory.Bytes;
        }

        protected void CheckInit() {
            if (!mInitialized) {
                Color = DefaultColor;
                mInitialized = true;
            }
        }

        public int Rows {
            get { return 25; }
        }

        public int Columns {
            get { return 80; }
        }

        public void NewLine() {
            CurrentY += 1;
            CurrentX = 0;
            if (CurrentY > Rows) {
                ScrollUp();
                CurrentY -= 1;
                CurrentX = 0;
            }

            SetCursor();
        }

        public void Clear() {
            CheckInit();
            // Empty + White + Empty + White
            UInt32 xData = 0x000F000F;
            IOGroup.Memory.Fill(0, 80 * 25 * 2 / 4, xData);

            CurrentX = 0;
            CurrentY = 1;

            SetCursor();
        }
        
        public void WriteChar(char aChar) {
            this[CurrentX, CurrentY] = aChar;
            CurrentX++;
            if (CurrentX == Columns) {
                NewLine();
            }
            SetCursor();
        }

        protected void ScrollUp() {
            CheckInit();
            IOGroup.Memory.MoveDown(0, 80, 80 * 24 * 2 / 4);
            SetCursor();
        }

        public char this[int aX, int aY] {
            get { 
                CheckInit();
                UInt32 xScreenOffset = (UInt32)((aX + aY * Columns) * 2);
                return (char)mMemory08[xScreenOffset];
            }
            set {
                CheckInit();
                var xScreenOffset = (UInt32)((aX + aY * Columns) * 2);
                mMemory08[xScreenOffset] = (byte)value;
                mMemory08[xScreenOffset + 1] = Color;
                SetCursor();
            }
        }

        public void SetColors(ConsoleColor aForeground, ConsoleColor aBackground) {
            CheckInit();
            Color = (byte)((byte)(aForeground) | ((byte)(aBackground) << 4));
        }

        private void SetCursor() {
            CheckInit();

            char xPos = (char)((CurrentY * 80) + CurrentX);
            // cursor low byte to VGA index register
            IOGroup.Idx3.Byte = 0x0F;
            IOGroup.Data3.Byte = (byte)(xPos & 0xFF);
            // cursor high byte to vga index register
            IOGroup.Idx3.Byte = 0x0E;
            IOGroup.Data3.Byte = (byte)(xPos >> 8);
        }

        private int mCurrentY = 1;
        public int CurrentY {
            get {
                return mCurrentY;
            }
            set {
                mCurrentY = value;
                SetCursor();
            }
        }

        private int mCurrentX = 0;
        public int CurrentX {
            get {
                return mCurrentX;
            }
            set {
                mCurrentX = value;
                SetCursor();
            }
        }
    }
}

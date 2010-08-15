using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Hardware {
    //TODO: Move all writeline etc and tracking to system ring. This clas is hardware, should be just basic stuff
    //TODO: Move raw writing to core ring
    //TODO: Remove /unsafe option from this asm
    public class TextScreen {
        protected const byte DefaultColor = 15; // White
        protected static bool mInitialized = false;
        protected static byte Color;

        static protected MemoryBlock mMemory;
        static protected MemoryBlock08 mMemory08;

        static TextScreen() {
            mMemory = Core.Global.TextScreenMemory;
            mMemory08 = mMemory.Bytes;
        }

        protected static void CheckInit() {
            if (!mInitialized) {
                Color = DefaultColor;
                mInitialized = true;
            }
        }

        public static int Rows {
            get { return 25; }
        }

        public static int Columns {
            get { return 80; }
        }

        public static void NewLine() {
            CurrentRow += 1;
            CurrentChar = 0;
            if (CurrentRow > Rows) {
                ScrollUp();
                CurrentRow -= 1;
                CurrentChar = 0;
            }

            SetCursor();
        }

        public static void Clear() {
            CheckInit();
            // Empty + White + Empty + White
            UInt32 xData = 0x000F000F;
            mMemory.Fill(0, 80 * 25 * 2 / 4, xData);

            CurrentChar = 0;
            CurrentRow = 1;

            SetCursor();
        }
        
        public static void WriteChar(char aChar) {
            PutChar(CurrentRow, CurrentChar, aChar);
            CurrentChar += 1;
            if (CurrentChar == Columns) {
                NewLine();
            }

            SetCursor();
        }

        protected static void ScrollUp() {
            CheckInit();
            mMemory.MoveDown(0, 80, 80 * 24 * 2 / 4);
            SetCursor();
        }

        //TODO: Change to indexer
        public static void PutChar(int aLine, int aRow, char aChar) {
            CheckInit();
            UInt32 xScreenOffset = (UInt32)((aRow + aLine * Columns) * 2);
            mMemory08[xScreenOffset] = (byte)aChar;
            mMemory08[xScreenOffset + 1] = Color;
            SetCursor();
        }

        public static void SetColors(ConsoleColor aForeground, ConsoleColor aBackground) {
            CheckInit();
            Color = (byte)((byte)(aForeground) | ((byte)(aBackground) << 4));
        }

        //TODO: Change to use Port class
        private static void SetCursor() {
            CheckInit();

            // TODO:
            // Set AH = 0x02
            // Set BH = 0
            // Set DH = CurrentRow
            // Set DL = CurrentChar 
            // Call interrupt 0x10

            //Store a backup of the color so that we can make sure the cursor is white
            byte tempColor = Color;

            Color = DefaultColor;

            char position = (char)((CurrentRow * 80) + CurrentChar);

            // cursor low byte to VGA index register
            Cosmos.Kernel.CPUBus.Write8(0x3D4, 0x0F);
            Cosmos.Kernel.CPUBus.Write8(0x3D5, (byte)(position & 0xFF));
            // cursor high byte to vga index register
            Cosmos.Kernel.CPUBus.Write8(0x3D4, 0x0E);
            Cosmos.Kernel.CPUBus.Write8(0x3D5, (byte)((position >> 8) & 0xFF));

            Color = tempColor;
        }

        private static int mCurrentRow = 1;
        public static int CurrentRow {
            get {
                return mCurrentRow;
            }
            set {
                mCurrentRow = value;
                SetCursor();
            }
        }

        private static int mCurrentChar = 0;
        public static int CurrentChar {
            get {
                return mCurrentChar;
            }
            set {
                mCurrentChar = value;
                SetCursor();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
  public class TextScreen {
    protected const int VideoAddr = 0xB8000;
    protected const byte DefaultColor = 15; //White
    protected static bool mInitialized = false;
    protected static byte Color;
    protected static void CheckInit() {
      if (!mInitialized) {
        Color = DefaultColor;
        mInitialized = true;
      }
    }

    public static int Rows {
      // for now 24 lines, as it's one line for the Heap Debug Display
      get { return 24; }
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

    public static unsafe void Clear() {
      CheckInit();

      byte* xScreenPtr = (byte*)(VideoAddr + (2 * Columns));
      for (int i = 0; i < Columns * (Rows); i++) {
        *xScreenPtr = 0;
        xScreenPtr++;
        //TODO: This and prob bg color too are a bug. We cant put straight in
        // because it causes blink etc. We need to translate/reduce the color.
        *xScreenPtr = Color;
        xScreenPtr++;
      }

      CurrentChar = 0;
      CurrentRow = 1;

      SetCursor();
    }
    public static unsafe void ReallyClearScreen() {
      CheckInit();

      byte* xScreenPtr = (byte*)(VideoAddr);
      for (int i = 0; i < Columns * Rows; i++) {
        *xScreenPtr = 0;
        xScreenPtr++;
        *xScreenPtr = Color;
        xScreenPtr++;
      }

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

    protected static unsafe void ScrollUp() {
      CheckInit();
      int Columns2 = Columns * 2;
      byte* xScreenPtr = (byte*)(VideoAddr + Columns2);
      for (int i = 0; i < Columns * Rows; i++) {
        *xScreenPtr = *(xScreenPtr + Columns2);
        xScreenPtr++;
        *xScreenPtr = *(xScreenPtr + Columns2);
        xScreenPtr++;
      }

      xScreenPtr = (byte*)(VideoAddr + Rows * Columns * 2);
      for (int i = 0; i < Columns; i++) {
        *xScreenPtr = 0;
        xScreenPtr++;
        *xScreenPtr = Color;
        xScreenPtr++;
      }

      SetCursor();
    }

    public unsafe static void RemoveChar(int aLine, int aRow) {
      CheckInit();
      int xScreenOffset = ((aRow + (aLine * Columns)) * 2);
      byte* xScreenPtr = (byte*)(VideoAddr + xScreenOffset);
      *xScreenPtr = 0;
      xScreenPtr++;
      *xScreenPtr = 0;

      SetCursor();
    }

    public unsafe static void PutChar(int aLine, int aRow, char aChar) {
      CheckInit();
      int xScreenOffset = ((aRow + (aLine * Columns)) * 2);
      byte* xScreenPtr = (byte*)(VideoAddr + xScreenOffset);
      byte xVal = (byte)aChar;
      *xScreenPtr = xVal;
      xScreenPtr++;
      *xScreenPtr = Color;
      SetCursor();
    }

    public static void SetColors(ConsoleColor aForeground, ConsoleColor aBackground) {
      CheckInit();
      Color = (byte)((byte)(aForeground) | ((byte)(aBackground) << 4));
    }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL {
  // Dont hold state here. This is a raw to hardware class. Virtual screens should be done
  // by memory moves
  public class TextScreen : TextScreenBase {
    protected byte Color = 0x0F; // White
    protected UInt16 mClearCellValue;
    protected UInt32 mRow2Addr;
    protected UInt32 mScrollSize;

    protected Core.IOGroup.TextScreen IO = new Cosmos.Core.IOGroup.TextScreen();
    protected readonly MemoryBlock08 mRAM;

    public TextScreen() {

      if (this is TextScreen)
      {
        Debugger.DoSend("this is TextScreen");
      }
      else
      {
        Debugger.DoSend("ERROR: This is not of type TextScreen!");
      }
      mRAM = IO.Memory.Bytes;
      // Set the Console default colors: White foreground on Black background, the default value of mClearCellValue is set there too as it is linked with the Color
      SetColors(ConsoleColor.White, ConsoleColor.Black);
      mRow2Addr = (UInt32)(Cols * 2);
      mScrollSize = (UInt32)(Cols * (Rows - 1) * 2);
      Debugger.DoSend("End of TextScreen..ctor");
    }

    public override UInt16 Rows { get { return 25; } }
    public override UInt16 Cols { get { return 80; } }

    public override void Clear() {
        Debugger.DoSend("Clearing screen with value ");
        Debugger.DoSendNumber(mClearCellValue);
        IO.Memory.Fill(mClearCellValue);
    }

    public override void ScrollUp()
    {
      IO.Memory.MoveDown(0, mRow2Addr, mScrollSize);
      //IO.Memory.Fill(mScrollSize, mRowSize32, mClearCellValue32);
      IO.Memory.Fill(mScrollSize, Cols, mClearCellValue);
    }

    public override char this[int aX, int aY]
    {
      get {
        var xScreenOffset = (UInt32)((aX + aY * Cols) * 2);
        return (char)mRAM[xScreenOffset];
      }
      set {
        var xScreenOffset = (UInt32)((aX + aY * Cols) * 2);
        mRAM[xScreenOffset] = (byte)value;
        mRAM[xScreenOffset + 1] = Color;
      }
    }

    public override void SetColors(ConsoleColor aForeground, ConsoleColor aBackground) {
            Color = (byte)((byte)(aForeground) | ((byte)(aBackground) << 4));
            // The Color | the NUL character this is used to Clear the Screen
            mClearCellValue = (UInt16)(Color << 8 | 0x00);
        }

    public override void SetCursorPos(int aX, int aY)
    {
      char xPos = (char)((aY * Cols) + aX);
      // Cursor low byte to VGA index register
      IO.Idx3.Byte = 0x0F;
      IO.Data3.Byte = (byte)(xPos & 0xFF);
      // Cursor high byte to VGA index register
      IO.Idx3.Byte = 0x0E;
      IO.Data3.Byte = (byte)(xPos >> 8);
    }
        public override byte GetColor()
        {
            return Color;
        }
    }
}

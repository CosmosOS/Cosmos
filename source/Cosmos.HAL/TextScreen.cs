using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL {
  // Dont hold state here. This is a raw to hardware class. Virtual screens should be done
  // by memory moves
  public class TextScreen : TextScreenBase {
    protected byte Color = 0x0F; // White
    // Empty + White
    protected UInt16 mClearCellValue = 0x000F;
    protected UInt32 mClearCellValue32;
    protected UInt32 mRow2Addr;
    protected UInt32 mScrollSize;
    protected UInt32 mRowSize32;

    protected Core.IOGroup.TextScreen IO = new Cosmos.Core.IOGroup.TextScreen();
    protected readonly MemoryBlock08 mRAM;

    public TextScreen() {
      mRAM = IO.Memory.Bytes;
      mClearCellValue32 = (UInt32)(mClearCellValue << 16 | mClearCellValue);
      mRow2Addr = (UInt32)(Cols * 2);
      mScrollSize = (UInt32)(Cols * (Rows - 1) * 2);
      mRowSize32 = (UInt32)Cols * 2 / 4;
    }

    public override UInt16 Rows { get { return 25; } }
    public override UInt16 Cols { get { return 80; } }

    public override void Clear() {
      IO.Memory.Fill(mClearCellValue32);
    }

    public override void ScrollUp()
    {
      IO.Memory.MoveDown(0, mRow2Addr, mScrollSize);
      IO.Memory.Fill(mScrollSize, mRowSize32, mClearCellValue32);
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

  }
}

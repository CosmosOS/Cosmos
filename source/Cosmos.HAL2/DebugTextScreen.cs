using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL;

public class DebugTextScreen : TextScreenBase
{
    private readonly Debugger mDebugger;

    private int mCurrentY = 1;

    public DebugTextScreen()
    {
        mDebugger = new Debugger("HAL", "DebugTextScreen");
    }

    public override ushort Cols
    {
        set { }
        get => 80;
    }

    public override ushort Rows
    {
        set { }
        get => 25;
    }


    public override byte this[int x, int y]
    {
        get => throw new NotImplementedException();
        set
        {
            if (y != mCurrentY)
            {
                SendChar(new[]
                {
                    '\r',
                    '\n'
                });
                mCurrentY = y;
            }

            SendChar(new[] { (char)value });
        }
    }

    public override void Clear()
    {
    }

    public override void SetColors(ConsoleColor aForeground, ConsoleColor aBackground)
    {
    }

    public override byte GetColor() => 0x0F;

    private void SendChar(char[] aData)
    {
        var xBytes = new byte[aData.Length];
        for (var i = 0; i < aData.Length; i++)
        {
            xBytes[i] = (byte)aData[i];
        }

        mDebugger.SendChannelCommand(129, 0, xBytes);
    }

    public override void SetCursorPos(int x, int y)
    {
    }

    public override void ScrollUp()
    {
    }

    public override int GetCursorSize() => throw new NotImplementedException();

    public override void SetCursorSize(int value) => throw new NotImplementedException();

    public override bool GetCursorVisible() => throw new NotImplementedException();

    public override void SetCursorVisible(bool value) => throw new NotImplementedException();
}

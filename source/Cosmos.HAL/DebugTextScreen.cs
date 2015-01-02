using System;
using System.Linq;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public class DebugTextScreen: TextScreenBase
    {
        private readonly Debugger mDebugger;
        public DebugTextScreen()
        {
            mDebugger = new Debugger("HAL", "DebugTextScreen");
        }

        public override void Clear()
        {
        }

        public override void SetColors(ConsoleColor aForeground, ConsoleColor aBackground)
        {
        }

        public override ushort Cols
        {
            get
            {
                return 80;
            }
        }

        public override ushort Rows
        {
            get
            {
                return 25;
            }
        }

        private void SendChar(char[] aData)
        {
            var xBytes = new byte[aData.Length];
            for (int i = 0; i < aData.Length; i++)
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

        private int mCurrentY = 1;


        public override char this[int x, int y]
        {
            get
            {
                throw new NotImplementedException();
            }
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
                SendChar(new []{value});
            }
        }
    }
}

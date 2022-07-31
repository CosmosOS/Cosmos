using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Cosmos.VS.Windows
{
    [Guid("A64D0FCC-8DCC-439A-9B16-3C43128AAD51")]
    public class StackTW : ToolWindowPane2
    {
        public StackTW()
        {
            Caption = "Cosmos Stack";
            BitmapResourceID = 301;
            BitmapIndex = 1;

            Content = new StackUC();
        }
    }

    public partial class StackUC : DebuggerUC
    {
        protected byte[] stackData = new byte[0];


        public StackUC()
        {
            InitializeComponent();
        }


        public override async Task UpdateAsync(string aTag, byte[] aData)
        {
            if (aTag == "FRAME")
            {
                mData = aData;
            }
            else
            {
                stackData = aData;
            }

            await DoUpdateAsync(aTag);
        }

        protected override async Task DoUpdateAsync(string aTag)
        {
            if (aTag == "STACK")
            {
                await UpdateStackAsync(stackData);
            }
            else if (aTag == "FRAME")
            {
                await UpdateFrameAsync(mData);
            }
        }

        public async Task UpdateFrameAsync(byte[] aData)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (aData == null)
            {
                memvEBP.Clear();
            }
            else
            {
                try
                {
                    var xValues = MemoryViewUC.Split(aData);
                    int xCount = xValues.Count;
                    memvEBP.Clear();
                    for (int i = 0; i < xCount; i++)
                    {
                        // We start at EBP + 8, because lower is not transmitted
                        // [EBP] is old EBP - not needed
                        // [EBP + 4] is saved EIP - not needed
                        memvEBP.Add("[EBP + " + (i * 4 + 8) + "]", xValues[i]);
                    }
                }
                catch
                {
                    memvEBP.Clear();
                }
            }
        }

        public async Task UpdateStackAsync(byte[] aData)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (aData == null)
            {
                memvESP.Clear();
            }
            else
            {
                try
                {
                    var xValues = MemoryViewUC.Split(aData);
                    int xCount = xValues.Count;
                    memvESP.Clear();
                    for (int i = 0; i < xCount; i++)
                    {
                        memvESP.Add(("[EBP - " + ((xCount - i) * 4) + "]").PadRight(10) + " [ESP + " + (i * 4) + "]", xValues[i]);
                    }
                }
                catch
                {
                    memvESP.Clear();
                }
            }
        }

        public override byte[] GetCurrentState()
        {
            byte[] aFrameData = mData ?? new byte[0];
            byte[] aStackData = stackData ?? new byte[0];
            return BitConverter.GetBytes(aFrameData.Length).Concat(aFrameData.Concat(aStackData)).ToArray();
        }
        public override async Task SetCurrentStateAsync(byte[] aData)
        {
            if (aData == null)
            {
                await UpdateAsync("FRAME", null);
                await UpdateAsync("STACK", null);
            }
            else
            {
                int mDataLength = BitConverter.ToInt32(aData, 0);
                byte[] aFrameData = new byte[mDataLength];
                byte[] aStackData = new byte[aData.Length - mDataLength - 4];
                Array.Copy(aData, 4, aFrameData, 0, aFrameData.Length);
                Array.Copy(aData, 4 + mDataLength, aStackData, 0, aStackData.Length);
                await UpdateAsync("FRAME", aFrameData);
                await UpdateAsync("STACK", aStackData);
            }
        }
    }
}

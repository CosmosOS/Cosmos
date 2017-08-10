using System;
using System.Linq;

namespace Cosmos.Debug.DebugConnectors
{
    partial class DebugConnector
    {
        // this file contains debug stub interaction code.

        public void SendRegisters()
        {
            SendCmd(Vs2Ds.SendRegisters);
        }

        public void SendFrame()
        {
            SendCmd(Vs2Ds.SendFrame);
        }

        public void SendStack()
        {
            SendCmd(Vs2Ds.SendStack);
        }

        public void Ping()
        {
            SendCmd(Vs2Ds.Ping);
        }

        public void SetBreakpoint(int aID, uint aAddress)
        {
            if (aAddress == 0)
            {
                DebugLog("DS Cmd: BP " + aID + " deleted.");
            }
            else
            {
                DebugLog("DS Cmd: BP " + aID + " @ " + aAddress.ToString("X8").ToUpper());
            }

            var xData = new byte[5];
            Array.Copy(BitConverter.GetBytes(aAddress), 0, xData, 0, 4);
            xData[4] = (byte)aID;
            SendCmd(Vs2Ds.BreakOnAddress, xData);
        }

        public void SetAsmBreakpoint(uint aAddress)
        {
            var xData = BitConverter.GetBytes(aAddress);
            SendCmd(Vs2Ds.SetAsmBreak, xData);
        }

        public void SetINT3(uint aAddress)
        {
            var xData = BitConverter.GetBytes(aAddress);
            SendCmd(Vs2Ds.SetINT3, xData);
        }
        public void ClearINT3(uint aAddress)
        {
            var xData = BitConverter.GetBytes(aAddress);
            SendCmd(Vs2Ds.ClearINT3, xData);
        }

        public void Continue()
        {
            SendCmd(Vs2Ds.Continue);
        }

        public byte[] GetMemoryData(uint address, uint size, int dataElementSize = 1)
        {
            //return new byte[size];

            // from debugstub:
            //// sends a stack value
            //// Serial Params:
            ////  1: x32 - address
            ////  2: x32 - size of data to send

            if (!IsConnected)
            {
                return null;
            }
            else if (size == 0)
            {
                // no point in retrieving 0 bytes, better not request at all. also, debugstub "crashes" then
                throw new NotSupportedException("Requested memory data of size = 0");
            }
            else if (size > 512)
            {
                // for now refuse to retrieve large amounts of data:
                throw new NotSupportedException("Too large amount of data requested");
            }
            var xData = new byte[8];
            mDataSize = (int)size;
            Array.Copy(BitConverter.GetBytes(address), 0, xData, 0, 4);
            Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
            SendCmd(Vs2Ds.SendMemory, xData);
            var xResult = MemoryDatas.First();
            MemoryDatas.RemoveAt(0);
            if (xResult.Length != size)
            {
                throw new Exception("Retrieved a different size than requested!");
            }
            return xResult;
        }

        public byte[] GetStackData(int offsetToEBP, uint size)
        {
            //return new byte[size];

            // from debugstub:
            //// sends a stack value
            //// Serial Params:
            ////  1: x32 - offset relative to EBP
            ////  2: x32 - size of data to send

            if (!IsConnected)
            {
                return null;
            }
            var xData = new byte[8];
            mDataSize = (int)size;

            // EBP is first
            //offsetToEBP += 4;

            Array.Copy(BitConverter.GetBytes(offsetToEBP), 0, xData, 0, 4);
            Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
            SendCmd(Vs2Ds.SendMethodContext, xData);

            // todo: make "crossplatform". this code assumes stack space of 32bit per "item"

            byte[] xResult;

            xResult = MethodContextDatas.First();
            MethodContextDatas.RemoveAt(0);
            return xResult;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;
using Cosmos.VS.Debug;
using System.Threading;
using System.Windows.Threading;

namespace Cosmos.Cosmos_VS_Windows
{
  class PipeCallback
  {
    public PipeCallback()
    {
      PipeThread.DataPacketReceived += new Action<byte, byte[]>(PipeThread_DataPacketReceived);
      var xServerThread = new Thread(PipeThread.ThreadStartServer);
      xServerThread.Start();
    }

     void PipeThread_DataPacketReceivedInvoke(byte aCommand, byte[] aData) {
      switch (aCommand) {
        case DwMsgType.Noop:
          break;

        case DwMsgType.Stack:
          break;

        case DwMsgType.Frame:
          break;

        case DwMsgType.Registers:
          RegistersTW.m_UC.Update(aData);
          break;

        case DwMsgType.Quit:
          //Close();
          break;

        case DwMsgType.AssemblySource:
          
          AssemblyTW.m_UC.Update(aData);
          break;
      }
    }

    void PipeThread_DataPacketReceived(byte aCmd, byte[] aMsg)
    {
      //Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
      //{
        //PipeThread_DataPacketReceivedInvoke(aCmd, aMsg);
      //});
    }

    //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    //{
    //  PipeThread.Stop();
    //}
  }
}

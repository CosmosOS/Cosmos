using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Cosmos.Debug.Common;
using Cosmos.Debug.DebugConnectors;

using Cosmos.VS.Windows.ToolWindows;

namespace Cosmos.VS.Windows
{
    [Guid(Guids.PackageGuidString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(AssemblyToolWindow))]
    [ProvideToolWindow(typeof(RegistersToolWindow))]
    [ProvideToolWindow(typeof(StackTW))]
    [ProvideToolWindow(typeof(InternalTW))]
    [ProvideToolWindow(typeof(ConsoleTW))]
    public sealed class CosmosWindowsPackage: Package
    {
        private readonly Queue<ushort> mCommand;
        private readonly Queue<byte[]> mMessage;
        private readonly Timer mTimer;

        private PipeServer mPipeDown;

        public StateStorer StateStorer { get; }

        private readonly Type[] mAllChannelWindowTypes = { typeof(ConsoleTW) };

        public CosmosWindowsPackage()
        {
            StateStorer = new StateStorer();

            mCommand = new Queue<ushort>();
            mMessage = new Queue<byte[]>();

            // There are a lot of threading issues in VSIP, and the WPF dispatchers do not work.
            // So instead we use a stack and a timer to poll it for data.
            mTimer = new Timer(100);
            mTimer.AutoReset = true;
            mTimer.Elapsed += ProcessMessage;
            mTimer.Start();

            mPipeDown = new PipeServer(Pipes.DownName);
            mPipeDown.DataPacketReceived += PipeThread_DataPacketReceived;
            mPipeDown.Start();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var xOutputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
            var xCosmosPaneGuid = Guid.NewGuid();

            ErrorHandler.ThrowOnFailure(
                xOutputWindow.CreatePane(ref xCosmosPaneGuid, "Cosmos", Convert.ToInt32(true), Convert.ToInt32(true)));
            ErrorHandler.ThrowOnFailure(xOutputWindow.GetPane(ref xCosmosPaneGuid, out var xOutputPane));

            Global.OutputPane = xOutputPane;
            xOutputPane.OutputString($"Debugger windows loaded.{Environment.NewLine}");

            CosmosMenuCmdSet.Initialize(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mTimer?.Dispose();
            }

            base.Dispose(disposing);
        }

        void ProcessMessage(object sender, EventArgs e)
        {
            ushort xCmd;
            byte[] xMsg;
            while (true)
            {
                lock (mCommand)
                {
                    if (mCommand.Count == 0)
                    {
                        break;
                    }
                    xCmd = mCommand.Dequeue();
                    xMsg = mMessage.Dequeue();
                }
                if (xCmd <= 127)
                {
                    // debug channel
                    switch (xCmd)
                    {
                        case Debugger2Windows.Noop:
                            break;

                        case Debugger2Windows.Stack:
                            UpdateWindow(typeof(StackTW), "STACK", xMsg);
                            break;

                        case Debugger2Windows.Frame:
                            UpdateWindow(typeof(StackTW), "FRAME", xMsg);
                            break;

                        case Debugger2Windows.Registers:
                            UpdateWindow(typeof(RegistersToolWindow), null, xMsg);
                            break;

                        case Debugger2Windows.Quit:
                            break;

                        case Debugger2Windows.AssemblySource:
                            UpdateWindow(typeof(AssemblyToolWindow), null, xMsg);
                            break;

                        case Debugger2Windows.PongVSIP:
                            UpdateWindow(typeof(InternalTW), null, Encoding.UTF8.GetBytes("Pong from VSIP"));
                            break;

                        case Debugger2Windows.PongDebugStub:
                            UpdateWindow(typeof(InternalTW), null, Encoding.UTF8.GetBytes("Pong from DebugStub"));
                            break;

                        case Debugger2Windows.OutputPane:
                            System.Windows.Application.Current.Dispatcher.Invoke(
                                () => Global.OutputPane.OutputString(Encoding.UTF8.GetString(xMsg)),
                                DispatcherPriority.Normal);
                            break;

                        case Debugger2Windows.OutputClear:
                            System.Windows.Application.Current.Dispatcher.Invoke(
                                () => { Global.OutputPane.Clear(); StateStorer.ClearState(); }, DispatcherPriority.Normal);
                            break;
                    }
                }
                else
                {
                    UpdateChannelWindows(xCmd, xMsg);
                }
            }
        }

        void PipeThread_DataPacketReceived(ushort aCmd, byte[] aMsg)
        {
            lock (mCommand)
            {
                mCommand.Enqueue(aCmd);
                mMessage.Enqueue(aMsg);
            }
        }

        public ToolWindowPane2 FindWindow(Type aWindowType)
        {
            // Get the instance number 0 of this tool window.
            // Our windows are single instance so this instance will be the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var xWindow = FindToolWindow(aWindowType, 0, true);
            if (xWindow?.Frame == null)
            {
                throw new NotSupportedException("Failed to create the Cosmos tool window.");
            }
            return xWindow as ToolWindowPane2;
        }

        public ToolWindowPaneChannel FindChannelWindow(Type aWindowType)
        {
            // Get the instance number 0 of this tool window.
            // Our windows are single instance so this instance will be the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var xWindow = FindToolWindow(aWindowType, 0, true);
            if (xWindow?.Frame == null)
            {
                throw new NotSupportedException("Failed to create the Cosmos tool window.");
            }
            return xWindow as ToolWindowPaneChannel;
        }

        public void UpdateChannelWindows(ushort aChannelAndCommand, byte[] aData)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                (Action)delegate ()
                {
                    foreach (var xType in mAllChannelWindowTypes)
                    {
                        var xWindow = FindChannelWindow(xType);
                        xWindow.UserControl.Package = this;
                        xWindow.UserControl.HandleChannelMessage(aChannelAndCommand, aData);
                    }
                }
            );
        }

        public void UpdateWindow(Type aWindowType, string aTag, byte[] aData)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.DataBind,
                (Action)delegate ()
                {
                    var xWindow = FindWindow(aWindowType);
                    xWindow.UserControl.Package = this;
                    xWindow.UserControl.Update(aTag, aData);
                }
            );
        }

        public void StoreAllStates()
        {
            var cWindow = FindWindow(typeof(StackTW));
            byte[] aData = cWindow.UserControl.GetCurrentState();
            StateStorer.StoreState("StackTW", aData == null ? null : (byte[])aData.Clone());

            cWindow = FindWindow(typeof(RegistersToolWindow));
            aData = cWindow.UserControl.GetCurrentState();
            StateStorer.StoreState("RegistersTW", aData == null ? null : (byte[])aData.Clone());
        }

        public void RestoreAllStates()
        {
            var cWindow = FindWindow(typeof(StackTW));
            byte[] aData = StateStorer.RetrieveState(StateStorer.CurrLineId, "StackTW");
            cWindow.UserControl.SetCurrentState(aData == null ? null : (byte[])aData.Clone());

            cWindow = FindWindow(typeof(RegistersToolWindow));
            aData = StateStorer.RetrieveState(StateStorer.CurrLineId, "RegistersTW");
            cWindow.UserControl.SetCurrentState(aData == null ? null : (byte[])aData.Clone());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Timer = System.Timers.Timer;

using Cosmos.Debug.Common;
using Cosmos.Debug.DebugConnectors;

using Cosmos.VS.Windows.ToolWindows;

namespace Cosmos.VS.Windows
{
    [Guid(Guids.PackageGuidString)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(AssemblyToolWindow))]
    [ProvideToolWindow(typeof(RegistersToolWindow))]
    [ProvideToolWindow(typeof(StackTW))]
    [ProvideToolWindow(typeof(InternalTW))]
    [ProvideToolWindow(typeof(ConsoleTW))]
    public sealed class CosmosWindowsPackage: AsyncPackage
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
            mTimer.Elapsed += (sender, e) => JoinableTaskFactory.RunAsync(() => ProcessMessageAsync(sender, e));
            mTimer.Start();

            mPipeDown = new PipeServer(Pipes.DownName);
            mPipeDown.DataPacketReceived += PipeThread_DataPacketReceived;
            mPipeDown.Start();
        }

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var xOutputWindow = (IVsOutputWindow)await GetServiceAsync(typeof(SVsOutputWindow));
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
        
        private async Task ProcessMessageAsync(object sender, EventArgs e)
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
                            await UpdateWindowAsync(typeof(StackTW), "STACK", xMsg);
                            break;

                        case Debugger2Windows.Frame:
                            await UpdateWindowAsync(typeof(StackTW), "FRAME", xMsg);
                            break;

                        case Debugger2Windows.Registers:
                            await UpdateWindowAsync(typeof(RegistersToolWindow), null, xMsg);
                            break;

                        case Debugger2Windows.Quit:
                            break;

                        case Debugger2Windows.AssemblySource:
                            await UpdateWindowAsync(typeof(AssemblyToolWindow), null, xMsg);
                            break;

                        case Debugger2Windows.PongVSIP:
                            await UpdateWindowAsync(typeof(InternalTW), null, Encoding.UTF8.GetBytes("Pong from VSIP"));
                            break;

                        case Debugger2Windows.PongDebugStub:
                            await UpdateWindowAsync(typeof(InternalTW), null, Encoding.UTF8.GetBytes("Pong from DebugStub"));
                            break;

                        case Debugger2Windows.OutputPane:
                            await JoinableTaskFactory.SwitchToMainThreadAsync();
                            Global.OutputPane.OutputString(Encoding.UTF8.GetString(xMsg));
                            break;

                        case Debugger2Windows.OutputClear:
                            await JoinableTaskFactory.SwitchToMainThreadAsync();
                            Global.OutputPane.Clear();
                            StateStorer.ClearState();
                            break;
                    }
                }
                else
                {
                    await UpdateChannelWindowsAsync(xCmd, xMsg);
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

        public async Task<ToolWindowPane2> FindWindowAsync(Type aWindowType)
        {
            // Get the instance number 0 of this tool window.
            // Our windows are single instance so this instance will be the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var xWindow = await FindToolWindowAsync(aWindowType, 0, true, default);
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

        public async Task<ToolWindowPaneChannel> FindChannelWindowAsync(Type aWindowType)
        {
            // Get the instance number 0 of this tool window.
            // Our windows are single instance so this instance will be the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var xWindow = await FindToolWindowAsync(aWindowType, 0, true, default);
            if (xWindow?.Frame == null)
            {
                throw new NotSupportedException("Failed to create the Cosmos tool window.");
            }
            return xWindow as ToolWindowPaneChannel;
        }

        public async Task UpdateChannelWindowsAsync(ushort aChannelAndCommand, byte[] aData)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            
            foreach (var xType in mAllChannelWindowTypes)
            {
                var xWindow = await FindChannelWindowAsync(xType);

                xWindow.UserControl.Package = this;
                xWindow.UserControl.HandleChannelMessage(aChannelAndCommand, aData);
            }
        }

        private async Task UpdateWindowAsync(Type aWindowType, string aTag, byte[] aData)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var xWindow = await FindWindowAsync(aWindowType);

            xWindow.UserControl.Package = this;
            xWindow.UserControl.Update(aTag, aData);
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

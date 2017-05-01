using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.DebugEngine.Commands
{
    public class DebugExecCommand : BaseDebugCommand
    {
        public DebugExecCommand(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override int Execute(uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int hr;

            if (IsQueryParameterList(pvaIn, pvaOut, nCmdExecOpt))
            {
                Marshal.GetNativeVariantForObject("$", pvaOut);
                return VSConstants.S_OK;
            }

            string arguments;
            hr = EnsureString(pvaIn, out arguments);
            if (hr != VSConstants.S_OK)
                return hr;

            if (string.IsNullOrWhiteSpace(arguments))
                throw new ArgumentException("Expected a Cosmos command to execute (ex: Debug.CosmosDebugExec info sharedlibrary)");

            DebugExec(arguments);

            return VSConstants.S_OK;
        }

        private void  DebugExec(string command)
        {
            var commandWindow = (IVsCommandWindow)serviceProvider.GetService(typeof(SVsCommandWindow));
            bool atBreak = false;
            var debugger = serviceProvider.GetService(typeof(SVsShellDebugger)) as IVsDebugger;
            if (debugger != null)
            {
                DBGMODE[] mode = new DBGMODE[1];
                if (debugger.GetMode(mode) == VSConstants.S_OK)
                {
                    atBreak = mode[0] == DBGMODE.DBGMODE_Break;
                }
            }

            string results = null;

            try
            {
                if (atBreak)
                {
                    commandWindow.ExecuteCommand(String.Format(CultureInfo.InvariantCulture, "Debug.EvaluateStatement -exec {0}", command));
                }
                else
                {
                    //results = await MIDebugCommandDispatcher.ExecuteCommand(command);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    e = e.InnerException;
                }

                commandWindow.Print($"Error: {e.Message}\r\n");
            }

            if (results != null && results.Length > 0)
            {
                // Make sure that we are printing whole lines
                if (!results.EndsWith("\n") && !results.EndsWith("\r\n"))
                {
                    results = results + "\n";
                }

                commandWindow.Print(results);
            }
        }
    }
}
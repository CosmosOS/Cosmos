using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.DebugEngine.Commands
{
    public class DebugLogCommand : BaseDebugCommand
    {
        private const string DebugLogCommandSyntax = "O,On:(d) OutputWindow Off";

        private enum DebugLogCommandSwitchEnum
        {
            On,
            OutputWindow,
            Off
        }

        public DebugLogCommand(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override int Execute(uint nCmdLogOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int hr;

            if (IsQueryParameterList(pvaIn, pvaOut, nCmdLogOpt))
            {
                Marshal.GetNativeVariantForObject("$ /switchdefs:\"" + DebugLogCommandSyntax + "\"", pvaOut);
                return VSConstants.S_OK;
            }

            string arguments;
            hr = EnsureString(pvaIn, out arguments);
            if (hr != VSConstants.S_OK)
                return hr;

            IVsParseCommandLine parseCommandLine = (IVsParseCommandLine)serviceProvider.GetService(typeof(SVsParseCommandLine));
            hr = parseCommandLine.ParseCommandTail(arguments, iMaxParams: -1);
            if (ErrorHandler.Failed(hr))
                return hr;

            hr = parseCommandLine.HasParams();
            if (ErrorHandler.Failed(hr))
                return hr;
            if (hr == VSConstants.S_OK || parseCommandLine.HasSwitches() != VSConstants.S_OK)
            {
                string message = string.Concat("Unexpected syntax for CosmosDebugLaunch command. Expected:\n",
                    "Debug.CosmosDebugLog [/On:<optional_path> [/OutputWindow] | /Off]");
                throw new ApplicationException(message);
            }

            hr = parseCommandLine.EvaluateSwitches(DebugLogCommandSyntax);
            if (ErrorHandler.Failed(hr))
                return hr;

            string logPath = string.Empty;
            bool logToOutput = false;
            if (parseCommandLine.GetSwitchValue((int)DebugLogCommandSwitchEnum.On, out logPath) == VSConstants.S_OK)
            {
                logToOutput = parseCommandLine.IsSwitchPresent((int)DebugLogCommandSwitchEnum.OutputWindow) == VSConstants.S_OK;
                if (parseCommandLine.IsSwitchPresent((int)DebugLogCommandSwitchEnum.Off) == VSConstants.S_OK)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "/On and /Off cannot both appear on command line"));
                }
                if (!logToOutput && string.IsNullOrEmpty(logPath))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Must specify a log file (/On:<path>) or /OutputWindow"));
                }
            }
            else if (parseCommandLine.IsSwitchPresent((int)DebugLogCommandSwitchEnum.Off) != VSConstants.S_OK)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "One of /On or /Off must be present on command line"));
            }

            EnableLogging(logToOutput, logPath);

            return 0;
        }

        private void EnableLogging(bool sendToOutputWindow, string logFile)
        {
            try
            {
                //MIDebugCommandDispatcher.EnableLogging(sendToOutputWindow, logFile);
            }
            catch (Exception e)
            {
                var commandWindow = (IVsCommandWindow)serviceProvider.GetService(typeof(SVsCommandWindow));
                commandWindow.Print(string.Format(CultureInfo.CurrentCulture, "Error: {0}\r\n", e.Message));
            }
        }
    }
}
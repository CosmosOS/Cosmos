using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.DebugEngine.Commands
{
    public class DebugLaunchCommand : BaseDebugCommand
    {
        private const string DebugLaunchCommandSyntax = "E,Executable:!(d) O,OptionsFile:!(d)";

        private enum DebugLaunchCommandSwitchEnum
        {
            Executable,
            OptionsFile
        }

        public DebugLaunchCommand(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override int Execute(uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int hr;

            if (IsQueryParameterList(pvaIn, pvaOut, nCmdExecOpt))
            {
                Marshal.GetNativeVariantForObject("$ /switchdefs:\"" + DebugLaunchCommandSyntax + "\"", pvaOut);
                return VSConstants.S_OK;
            }

            hr = EnsureString(pvaIn, out var arguments);

            if (hr != VSConstants.S_OK)
            {
                return hr;
            }

            var parseCommandLine = (IVsParseCommandLine)serviceProvider.GetService(typeof(SVsParseCommandLine));
            Assumes.Present(parseCommandLine);

            hr = parseCommandLine.ParseCommandTail(arguments, iMaxParams: -1);

            if (ErrorHandler.Failed(hr))
            {
                return hr;
            }

            hr = parseCommandLine.HasParams();

            if (ErrorHandler.Failed(hr))
            {
                return hr;
            }

            if (hr == VSConstants.S_OK || parseCommandLine.HasSwitches() != VSConstants.S_OK)
            {
                var message = String.Concat("Unexpected syntax for CosmosDebugLaunch command. Expected:\n",
                    "Debug.CosmosDebugLaunch /Executable:<path_or_logical_name> /OptionsFile:<path>");
                throw new ApplicationException(message);
            }

            hr = parseCommandLine.EvaluateSwitches(DebugLaunchCommandSyntax);
            if (ErrorHandler.Failed(hr))
            {
                return hr;
            }

            if (parseCommandLine.GetSwitchValue((int)DebugLaunchCommandSwitchEnum.Executable, out var executable) != VSConstants.S_OK ||
                String.IsNullOrWhiteSpace(executable))
            {
                throw new ArgumentException("Executable must be specified");
            }

            var checkExecutableExists = false;
            var options = String.Empty;

            if (parseCommandLine.GetSwitchValue((int)DebugLaunchCommandSwitchEnum.OptionsFile, out var optionsFilePath) == 0)
            {
                // When using the options file, we want to allow the executable to be just a logical name, but if
                // one enters a real path, we should make sure it isn't mistyped. If the path contains a slash, we assume it 
                // is meant to be a real path so enforce that it exists
                checkExecutableExists = (executable.IndexOf('\\') >= 0);

                if (String.IsNullOrWhiteSpace(optionsFilePath))
                {
                    throw new ArgumentException("Value expected for '/OptionsFile' option");
                }

                if (!File.Exists(optionsFilePath))
                {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Options file '{0}' does not exist", optionsFilePath));
                }

                options = File.ReadAllText(optionsFilePath);
            }

            if (checkExecutableExists)
            {
                if (!File.Exists(executable))
                {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Executable '{0}' does not exist", executable));
                }

                executable = Path.GetFullPath(executable);
            }

            LaunchDebugTarget(executable, options);

            return 0;
        }

        private void LaunchDebugTarget(string filePath, string options)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var debugger = (IVsDebugger4)serviceProvider.GetService(typeof(IVsDebugger));
            Assumes.Present(debugger);

            var debugTargets = new VsDebugTargetInfo4[1];
            debugTargets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            debugTargets[0].bstrExe = filePath;
            debugTargets[0].bstrOptions = options;
            debugTargets[0].guidLaunchDebugEngine = Guids.DebugEngineGuid;
            var processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];

            debugger.LaunchDebugTargets4(1, debugTargets, processInfo);
        }

    }
}

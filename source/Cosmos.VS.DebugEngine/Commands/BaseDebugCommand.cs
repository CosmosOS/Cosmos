using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using IServiceProvider = System.IServiceProvider;

namespace Cosmos.VS.DebugEngine.Commands
{
    public abstract class BaseDebugCommand
    {
        protected IServiceProvider serviceProvider;

        protected BaseDebugCommand(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public virtual int Execute(uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            throw new NotImplementedException();
        }

        protected static int EnsureString(IntPtr pvaIn, out string arguments)
        {
            arguments = null;
            if (pvaIn == IntPtr.Zero)
            {
                // No arguments.
                return VSConstants.E_INVALIDARG;
            }

            object vaInObject = Marshal.GetObjectForNativeVariant(pvaIn);
            if (vaInObject == null || vaInObject.GetType() != typeof(string))
            {
                return VSConstants.E_INVALIDARG;
            }

            arguments = vaInObject as string;
            return VSConstants.S_OK;
        }

        protected static bool IsQueryParameterList(IntPtr pvaIn, IntPtr pvaOut, uint nCmdexecopt)
        {
            ushort lo = (ushort)(nCmdexecopt & 0xffff);
            ushort hi = (ushort)(nCmdexecopt >> 16);
            if (lo == (ushort)OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP)
            {
                if (hi == VsMenus.VSCmdOptQueryParameterList)
                {
                    if (pvaOut != IntPtr.Zero)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}

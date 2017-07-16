using System;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    public class AD7Port: IDebugPort2
    {
        int IDebugPort2.EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugPort2.GetPortId(out Guid pguidPort)
        {
            throw new NotImplementedException();
        }

        int IDebugPort2.GetPortName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugPort2.GetPortRequest(out IDebugPortRequest2 ppRequest)
        {
            throw new NotImplementedException();
        }

        int IDebugPort2.GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
        {
            throw new NotImplementedException();
        }

        int IDebugPort2.GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
        {
            throw new NotImplementedException();
        }

    }
}
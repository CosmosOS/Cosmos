﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace Cosmos.Debug.VSDebugEngine
{
    [ComVisible(false)]
    [Guid("2BB39582-4B39-4211-ACCF-ECDABFFBE061")]
    public class AD7PortSupplier : IDebugPortSupplier2
    {

        int IDebugPortSupplier2.AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            throw new NotImplementedException();
        }

        int IDebugPortSupplier2.CanAddPort()
        {
            throw new NotImplementedException();
        }

        int IDebugPortSupplier2.EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugPortSupplier2.GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            throw new NotImplementedException();
        }

        int IDebugPortSupplier2.GetPortSupplierId(out Guid pguidPortSupplier)
        {
            throw new NotImplementedException();
        }

        int IDebugPortSupplier2.GetPortSupplierName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugPortSupplier2.RemovePort(IDebugPort2 pPort)
        {
            throw new NotImplementedException();
        }

    }
}
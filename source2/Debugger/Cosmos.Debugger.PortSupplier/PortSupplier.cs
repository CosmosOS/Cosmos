using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace Cosmos.Debugger.PortSupplier {

  [Guid("83689638-F0EA-46c6-A03E-4D3F8C96328A")]
  public class PortSupplier : IDebugPortSupplier2 {
    #region IDebugPortSupplier2 Members

    int IDebugPortSupplier2.AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort) {
      throw new NotImplementedException();
    }

    int IDebugPortSupplier2.CanAddPort() {
      throw new NotImplementedException();
    }

    int IDebugPortSupplier2.EnumPorts(out IEnumDebugPorts2 ppEnum) {
      throw new NotImplementedException();
    }

    int IDebugPortSupplier2.GetPort(ref Guid guidPort, out IDebugPort2 ppPort) {
      throw new NotImplementedException();
    }

    int IDebugPortSupplier2.GetPortSupplierId(out Guid pguidPortSupplier) {
      throw new NotImplementedException();
    }

    int IDebugPortSupplier2.GetPortSupplierName(out string pbstrName) {
      throw new NotImplementedException();
    }

    int IDebugPortSupplier2.RemovePort(IDebugPort2 pPort) {
      throw new NotImplementedException();
    }

    #endregion
  }
}

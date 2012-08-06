
// Attributes for the CorApiAssembly
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;



// Expose non-CLS-compliant types, so we can't be CLS-compliant
[assembly:CLSCompliant(true)]
[assembly:System.Runtime.InteropServices.ComVisible(false)]
[assembly:SecurityPermission(SecurityAction.RequestMinimum, Unrestricted=true)]
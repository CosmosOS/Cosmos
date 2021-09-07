using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl {
    // This class represents the information that describes a bound breakpoint.
    public class AD7BreakpointResolution : IDebugBreakpointResolution2 {
        private AD7Engine m_engine;
        private uint m_address;
        private AD7DocumentContext m_documentContext;

        public AD7BreakpointResolution(AD7Engine engine, uint address, AD7DocumentContext documentContext) {
            m_engine = engine;
            m_address = address;
            m_documentContext = documentContext;
        }

        // Gets the type of the breakpoint represented by this resolution. 
        int IDebugBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType) {
            // The sample engine only supports code breakpoints.
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        // Gets the breakpoint resolution information that describes this breakpoint.
        int IDebugBreakpointResolution2.GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo) {
	        if ((dwFields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) { 
                // The sample engine only supports code breakpoints.
                var location = new BP_RESOLUTION_LOCATION();
                location.bpType = (uint)enum_BP_TYPE.BPT_CODE;

                // The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
                // to IDebugCodeContex2 and not IUnknown.
                var codeContext = new AD7MemoryAddress(m_engine, m_address);
                codeContext.SetDocumentContext(m_documentContext);
                location.unionmember1 = Marshal.GetComInterfaceForObject(codeContext, typeof(IDebugCodeContext2));
                pBPResolutionInfo[0].bpResLocation = location;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
            }
	        
            if (dwFields.HasFlag(enum_BPRESI_FIELDS.BPRESI_PROGRAM)) {
                pBPResolutionInfo[0].pProgram = m_engine;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
            }
	       
            return VSConstants.S_OK;
        }

    }

    class AD7ErrorBreakpointResolution : IDebugErrorBreakpointResolution2 {

        int IDebugErrorBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType) {
            throw new Exception("The method or operation is not implemented.");
        }

        int IDebugErrorBreakpointResolution2.GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] pErrorResolutionInfo)
        {
            //if ((dwFields.HasFlag(enum_BPERESI_FIELDS.BPERESI_BPRESLOCATION)) {}
            //if ((dwFields.HasFlag(enum_BPERESI_FIELDS.BPERESI_PROGRAM)) {}
            //if ((dwFields.HasFlag(enum_BPERESI_FIELDS.BPERESI_THREAD)) {}
            //if ((dwFields.HasFlag(enum_BPERESI_FIELDS.BPERESI_MESSAGE)) {}
            //if ((dwFields.HasFlag(enum_BPERESI_FIELDS.BPERESI_TYPE))) {}

            throw new Exception("The method or operation is not implemented.");
        }

    }
}

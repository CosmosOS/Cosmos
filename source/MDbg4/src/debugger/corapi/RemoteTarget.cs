//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{
    public sealed class CorRemoteTarget : ICorDebugRemoteTarget
    {
        private string m_hostName;
        
        public CorRemoteTarget(string hostName)
        {
            m_hostName = hostName;
        }

        [CLSCompliant(false)]
        public void GetHostName(uint cchHostName, out uint pcchHostName, char[] szHostName)
        {
            if ((cchHostName == 0) != (szHostName == null))
            {
                throw new ArgumentException();
            }

            // This function is expected to be called by a native caller, which expects the number of 
            // characters to include the null character.
            pcchHostName = (uint)m_hostName.Length + 1;

            if (cchHostName > 0)
            {
                if (cchHostName < pcchHostName)
                {
                    // HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER)
                    throw new COMException("Buffer too small", unchecked((int)0x8007007A));
                }
                m_hostName.CopyTo(0, szHostName, 0, m_hostName.Length);
                szHostName[m_hostName.Length] = '\0';
            }
        }
    } /* class CorRemoteTarget */
} /* namespace */

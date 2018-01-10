/********************************************************************************************

Copyright (c) Microsoft Corporation 
All rights reserved. 

Microsoft Public License: 

This license governs use of the accompanying software. If you use the software, you 
accept this license. If you do not accept the license, do not use the software. 

1. Definitions 
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the 
same meaning here as under U.S. copyright law. 
A "contribution" is the original software, or any additions or changes to the software. 
A "contributor" is any person that distributes its contribution under this license. 
"Licensed patents" are a contributor's patent claims that read directly on its contribution. 

2. Grant of Rights 
(A) Copyright Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free copyright license to reproduce its contribution, prepare derivative works of 
its contribution, and distribute its contribution or any derivative works that you create. 
(B) Patent Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free license under its licensed patents to make, have made, use, sell, offer for 
sale, import, and/or otherwise dispose of its contribution in the software or derivative 
works of the contribution in the software. 

3. Conditions and Limitations 
(A) No Trademark License- This license does not grant you rights to use any contributors' 
name, logo, or trademarks. 
(B) If you bring a patent claim against any contributor over patents that you claim are 
infringed by the software, your patent license from such contributor to the software ends 
automatically. 
(C) If you distribute any portion of the software, you must retain all copyright, patent, 
trademark, and attribution notices that are present in the software. 
(D) If you distribute any portion of the software in source code form, you may do so only 
under this license by including a complete copy of this license with your distribution. 
If you distribute any portion of the software in compiled or object code form, you may only 
do so under a license that complies with this license. 
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give 
no express warranties, guarantees or conditions. You may have additional consumer rights 
under your local laws which this license cannot change. To the extent permitted under your 
local laws, the contributors exclude the implied warranties of merchantability, fitness for 
a particular purpose and non-infringement.

********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Class used to identify a source of events of type SinkType.
    /// </summary>
    [ComVisible(false)]
    internal interface IEventSource<SinkType>
        where SinkType : class
    {
        void OnSinkAdded(SinkType sink);
        void OnSinkRemoved(SinkType sink);
    }

    [ComVisible(true)]
    public class ConnectionPointContainer : IConnectionPointContainer
    {
        private Dictionary<Guid, IConnectionPoint> connectionPoints;
        internal ConnectionPointContainer()
        {
            connectionPoints = new Dictionary<Guid, IConnectionPoint>();
        }
        internal void AddEventSource<SinkType>(IEventSource<SinkType> source)
            where SinkType : class
        {
            if(null == source)
            {
                throw new ArgumentNullException("source");
            }
            if(connectionPoints.ContainsKey(typeof(SinkType).GUID))
            {
                throw new ArgumentException("EventSource guid already added to the list of connection points", "source");
            }
            connectionPoints.Add(typeof(SinkType).GUID, new ConnectionPoint<SinkType>(this, source));
        }

        #region IConnectionPointContainer Members
        void IConnectionPointContainer.EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
        {
            throw new NotImplementedException(); ;
        }
        void IConnectionPointContainer.FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
        {
            ppCP = connectionPoints[riid];
        }
        #endregion
    }

    internal class ConnectionPoint<SinkType> : IConnectionPoint
        where SinkType : class
    {
        Dictionary<uint, SinkType> sinks;
        private uint nextCookie;
        private ConnectionPointContainer container;
        private IEventSource<SinkType> source;
        internal ConnectionPoint(ConnectionPointContainer container, IEventSource<SinkType> source)
        {
            if(null == container)
            {
                throw new ArgumentNullException("container");
            }
            if(null == source)
            {
                throw new ArgumentNullException("source");
            }
            this.container = container;
            this.source = source;
            sinks = new Dictionary<uint, SinkType>();
            nextCookie = 1;
        }
        #region IConnectionPoint Members
        public void Advise(object pUnkSink, out uint pdwCookie)
        {
            SinkType sink = pUnkSink as SinkType;
            if(null == sink)
            {
                Marshal.ThrowExceptionForHR(VSConstants.E_NOINTERFACE);
            }
            sinks.Add(nextCookie, sink);
            pdwCookie = nextCookie;
            source.OnSinkAdded(sink);
            nextCookie += 1;
        }

        public void EnumConnections(out IEnumConnections ppEnum)
        {
            throw new NotImplementedException(); ;
        }

        public void GetConnectionInterface(out Guid pIID)
        {
            pIID = typeof(SinkType).GUID;
        }

        public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
        {
            ppCPC = this.container;
        }

        public void Unadvise(uint dwCookie)
        {
            // This will throw if the cookie is not in the list.
            SinkType sink = sinks[dwCookie];
            sinks.Remove(dwCookie);
            source.OnSinkRemoved(sink);
        }
        #endregion
    }
}

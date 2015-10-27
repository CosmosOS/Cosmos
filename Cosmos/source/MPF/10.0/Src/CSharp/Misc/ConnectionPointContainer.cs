/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

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

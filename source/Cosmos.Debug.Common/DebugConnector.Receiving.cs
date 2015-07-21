using System;

namespace Cosmos.Debug.Common
{
    partial class DebugConnector
    {
        protected abstract void Next(int aPacketSize, Action<byte[]> aCompleted);

    }
}

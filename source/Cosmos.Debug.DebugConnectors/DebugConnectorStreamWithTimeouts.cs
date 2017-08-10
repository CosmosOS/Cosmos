using System;

namespace Cosmos.Debug.DebugConnectors
{
    public abstract class DebugConnectorStreamWithTimeouts : DebugConnectorStreamWithoutTimeouts
    {
        protected override int TryRead(byte[] buffer, int offset, int count, int timeout)
        {
            var xStream = mStream;
            if (xStream == null)
            {
                return 0;
            }
            mStream.ReadTimeout = timeout;
            try
            {
                return xStream.Read(buffer, offset, count);
            }
            catch (TimeoutException)
            {
                return 0;
            }
            catch
            {
                throw;
            }
        }
    }
}

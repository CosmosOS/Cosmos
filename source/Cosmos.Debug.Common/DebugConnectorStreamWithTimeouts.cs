namespace Cosmos.Debug.Common
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
            return xStream.Read(buffer, offset, count);
        }
    }
}

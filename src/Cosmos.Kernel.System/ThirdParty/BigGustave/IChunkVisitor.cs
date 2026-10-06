namespace BigGustave
{
    using System.IO;

    /// <summary>
    /// Enables execution of custom logic whenever a chunk is read.
    /// </summary>
    internal interface IChunkVisitor
    {
        /// <summary>
        /// Called by the PNG reader after a chunk is read.
        /// </summary>
        void Visit(Stream stream, ImageHeader header, ChunkHeader chunkHeader, byte[] data, byte[] crc);
    }
}

namespace BigGustave
{
    using System;

    /// <summary>
    /// The header for a data chunk in a PNG file.
    /// </summary>
    internal readonly struct ChunkHeader
    {
        /// <summary>
        /// The position/start of the chunk header within the stream.
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// The length of the chunk in bytes.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The name of the chunk, uppercase first letter means the chunk is critical (vs. ancillary).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether the chunk is critical (must be read by all readers) or ancillary (may be ignored).
        /// </summary>
        public bool IsCritical => char.IsUpper(Name[0]);

        /// <summary>
        /// A public chunk is one that is defined in the International Standard or is registered in the list of public chunk types maintained by the Registration Authority. 
        /// Applications can also define private (unregistered) chunk types for their own purposes.
        /// </summary>
        public bool IsPublic => char.IsUpper(Name[1]);

        /// <summary>
        /// Whether the (if unrecognized) chunk is safe to copy.
        /// </summary>
        public bool IsSafeToCopy => char.IsUpper(Name[3]);

        /// <summary>
        /// Create a new <see cref="ChunkHeader"/>.
        /// </summary>
        public ChunkHeader(long position, int length, string name)
        {
            if (length < 0)
            {
                throw new ArgumentException($"Length less than zero ({length}) encountered when reading chunk at position {position}.");
            }

            Position = position;
            Length = length;
            Name = name;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} at {Position} (length: {Length}).";
        }
    }
}

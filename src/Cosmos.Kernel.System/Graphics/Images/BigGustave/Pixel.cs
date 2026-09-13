namespace BigGustave
{
    /// <summary>
    /// A pixel in a <see cref="Png"/> image.
    /// </summary>
    internal readonly struct Pixel
    {
        /// <summary>
        /// The red value for the pixel.
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// The green value for the pixel.
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// The blue value for the pixel.
        /// </summary>
        public byte B { get; }

        /// <summary>
        /// The alpha transparency value for the pixel.
        /// </summary>
        public byte A { get; }

        /// <summary>
        /// Whether the pixel is grayscale (if <see langword="true"/> <see cref="R"/>, <see cref="G"/> and <see cref="B"/> will all have the same value).
        /// </summary>
        public bool IsGrayscale { get; }

        /// <summary>
        /// Create a new <see cref="Pixel"/>.
        /// </summary>
        /// <param name="r">The red value for the pixel.</param>
        /// <param name="g">The green value for the pixel.</param>
        /// <param name="b">The blue value for the pixel.</param>
        /// <param name="a">The alpha transparency value for the pixel.</param>
        /// <param name="isGrayscale">Whether the pixel is grayscale.</param>
        public Pixel(byte r, byte g, byte b, byte a, bool isGrayscale)
        {
            R = r;
            G = g;
            B = b;
            A = a;
            IsGrayscale = isGrayscale;
        }

        /// <summary>
        /// Create a new <see cref="Pixel"/> which has <see cref="IsGrayscale"/> false and is fully opaque.
        /// </summary>
        /// <param name="r">The red value for the pixel.</param>
        /// <param name="g">The green value for the pixel.</param>
        /// <param name="b">The blue value for the pixel.</param>
        public Pixel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 255;
            IsGrayscale = false;
        }

        /// <summary>
        /// Create a new grayscale <see cref="Pixel"/>.
        /// </summary>
        /// <param name="grayscale">The grayscale value.</param>
        public Pixel(byte grayscale)
        {
            R = grayscale;
            G = grayscale;
            B = grayscale;
            A = 255;
            IsGrayscale = true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Pixel pixel)
            {
                return IsGrayscale == pixel.IsGrayscale
                       && A == pixel.A
                       && R == pixel.R
                       && G == pixel.G
                       && B == pixel.B;
            }

            return false;
        }

        /// <summary>
        /// Whether the pixel values are equal.
        /// </summary>
        /// <param name="other">The other pixel.</param>
        /// <returns><see langword="true"/> if all pixel values are equal otherwise <see langword="false"/>.</returns>
        public bool Equals(Pixel other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A && IsGrayscale == other.IsGrayscale;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                hashCode = (hashCode * 397) ^ IsGrayscale.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({R}, {G}, {B}, {A})";
        }
    }
}

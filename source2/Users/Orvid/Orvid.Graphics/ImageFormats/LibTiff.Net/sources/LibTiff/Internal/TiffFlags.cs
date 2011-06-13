using System;

namespace BitMiracle.LibTiff.Classic.Internal
{
    [Flags]
    enum TiffFlags
    {
        /// <summary>
        /// Use MSB2LSB (most significant -> least) fill order
        /// </summary>
        MSB2LSB = 1,

        /// <summary>
        /// Use LSB2MSB (least significant -> most) fill order
        /// </summary>
        LSB2MSB = 2,

        /// <summary>
        /// natural bit fill order for machine
        /// </summary>
        FILLORDER = 0x0003,

        /// <summary>
        /// current directory must be written
        /// </summary>
        DIRTYDIRECT = 0x0008,

        /// <summary>
        /// data buffers setup
        /// </summary>
        BUFFERSETUP = 0x0010,

        /// <summary>
        /// encoder/decoder setup done
        /// </summary>
        CODERSETUP = 0x0020,

        /// <summary>
        /// written 1+ scanlines to file
        /// </summary>
        BEENWRITING = 0x0040,

        /// <summary>
        /// byte swap file information
        /// </summary>
        SWAB = 0x0080,

        /// <summary>
        /// inhibit bit reversal logic
        /// </summary>
        NOBITREV = 0x0100,

        /// <summary>
        /// my raw data buffer; free on close
        /// </summary>
        MYBUFFER = 0x0200,

        /// <summary>
        /// file is tile, not strip- based
        /// </summary>
        ISTILED = 0x0400,

        /// <summary>
        /// need call to postencode routine
        /// </summary>
        POSTENCODE = 0x1000,

        /// <summary>
        /// currently writing a subifd
        /// </summary>
        INSUBIFD = 0x2000,

        /// <summary>
        /// library is doing data up-sampling
        /// </summary>
        UPSAMPLED = 0x4000,

        /// <summary>
        /// enable strip chopping support
        /// </summary>
        STRIPCHOP = 0x8000,

        /// <summary>
        /// read header only, do not process the first directory
        /// </summary>
        HEADERONLY = 0x10000,

        /// <summary>
        /// skip reading of raw uncompressed image data
        /// </summary>
        NOREADRAW = 0x20000,
    }
}

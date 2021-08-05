using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EFI_RNG_ALGORITHM
    {
        private readonly EFI_GUID Algorithm;

        private EFI_RNG_ALGORITHM(EFI_GUID algorithm)
        {
            Algorithm = algorithm;
        }

        /// <summary>
        /// <para>NIST SP 800-90, “Recommendation for Random Number Generation Using Deterministic Random Bit Generators,” March 2007.</para>
        /// <para>See “Links to UEFI-Related Documents” (http://uefi.org/uefi) under the heading “Recommendation for Random Number Generation Using Deterministic Random Bit Generators”.</para>
        /// </summary>
        public static readonly EFI_RNG_ALGORITHM EFI_RNG_ALGORITHM_SP800_90_HASH_256_GUID =
            new(new EFI_GUID(0xa7af67cb, 0x603b, 0x4d42, 0xba, 0x21, 0x70, 0xbf, 0xb6, 0x29, 0x3f, 0x96));

        /// <summary>
        /// <para>NIST SP 800-90, “Recommendation for Random Number Generation Using Deterministic Random Bit Generators,” March 2007.</para>
        /// <para>See “Links to UEFI-Related Documents” (http://uefi.org/uefi) under the heading “Recommendation for Random Number Generation Using Deterministic Random Bit Generators”.</para>
        /// </summary>
        public static readonly EFI_RNG_ALGORITHM EFI_RNG_ALGORITHM_SP800_90_HMAC_256_GUID =
            new(new EFI_GUID(0xc5149b43, 0xae85, 0x4f53, 0x99, 0x82, 0xb9, 0x43, 0x35, 0xd3, 0xa9, 0xe7));

        /// <summary>
        /// <para>NIST SP 800-90, “Recommendation for Random Number Generation Using Deterministic Random Bit Generators,” March 2007.</para>
        /// <para>See “Links to UEFI-Related Documents” (http://uefi.org/uefi) under the heading “Recommendation for Random Number Generation Using Deterministic Random Bit Generators”.</para>
        /// </summary>
        public static readonly EFI_RNG_ALGORITHM EFI_RNG_ALGORITHM_SP800_90_CTR_256_GUID =
                new(new EFI_GUID(0x44f0de6e, 0x4d8c, 0x4045, 0xa8, 0xc7, 0x4d, 0xd1, 0x68, 0x85, 0x6b, 0x9e));

        /// <summary>
        /// <para>NIST, “Recommended Random Number Generator Based on ANSI X9.31 Appendix A.2.4 Using the 3-Key Triple DES and AES Algorithms,” January 2005.</para>
        /// <para>See “Links to UEFI-Related Documents” (http://uefi.org/uefi) under the heading “Recommended Random Number Generator Based on ANSI X9.31”</para>
        /// </summary>
        public static readonly EFI_RNG_ALGORITHM EFI_RNG_ALGORITHM_X9_31_3DES_GUID =
                new(new EFI_GUID(0x63c4785a, 0xca34, 0x4012, 0xa3, 0xc8, 0x0b, 0x6a, 0x32, 0x4f, 0x55, 0x46));

        /// <summary>
        /// <para>NIST, “Recommended Random Number Generator Based on ANSI X9.31 Appendix A.2.4 Using the 3-Key Triple DES and AES Algorithms,” January 2005.</para>
        /// <para>See “Links to UEFI-Related Documents” (http://uefi.org/uefi) under the heading “Recommended Random Number Generator Based on ANSI X9.31”</para>
        /// </summary>
        public static readonly EFI_RNG_ALGORITHM EFI_RNG_ALGORITHM_X9_31_AES_GUID =
                new(new EFI_GUID(0xacd03321, 0x777e, 0x4d3d, 0xb1, 0xc8, 0x20, 0xcf, 0xd8, 0x88, 0x20, 0xc9));

        /// <summary>
        /// The “raw” algorithm, when supported, is intended to provide entropy directly from the source, without it going through some deterministic random bit generator.
        /// </summary>
        public static readonly EFI_RNG_ALGORITHM EFI_RNG_ALGORITHM_RAW =
                new(new EFI_GUID(0xe43176d7, 0xb6e8, 0x4827, 0xb7, 0x84, 0x7f, 0xfd, 0xc4, 0xb6, 0x85, 0x61));
    }
}

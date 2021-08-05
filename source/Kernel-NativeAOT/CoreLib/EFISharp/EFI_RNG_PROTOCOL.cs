using System;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFI_RNG_PROTOCOL
    {
        private readonly delegate*<EFI_RNG_PROTOCOL*, nuint*, EFI_RNG_ALGORITHM*, EFI_STATUS> _getInfo;
        private readonly delegate*<EFI_RNG_PROTOCOL*, EFI_RNG_ALGORITHM*, nuint, byte*, EFI_STATUS> _getRNG;

        /// <summary>
        /// This function returns information about supported RNG algorithms.
        /// </summary>
        /// <param name="rngAlgorithmList"><para>A buffer filled by the driver with one EFI_RNG_ALGORITHM element for each supported RNG algorithm.</para>
        /// <para>The list must not change across multiple calls to the same driver. Note that the first algorithm in the list is the default algorithm for the driver. </para></param>
        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the <paramref name="rngAlgorithmList"/> was returned successfully.</para>
        /// <para><see cref="EFI_STATUS.EFI_UNSUPPORTED"/> if the service is not supported by this driver.</para>
        /// <para><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/> if the list of algorithms could not be retrieved due to a hardware or firmware error.</para></returns>
        public EFI_STATUS GetInfo(out EFI_RNG_ALGORITHM[] rngAlgorithmList)
        {
            fixed (EFI_RNG_PROTOCOL* pThis = &this)
            {
                nuint byteCount = 0;
                EFI_STATUS status = _getInfo(pThis, &byteCount, null);

                if (status != EFI_STATUS.EFI_BUFFER_TOO_SMALL)
                {
                    rngAlgorithmList = null;
                    return status;
                }

                rngAlgorithmList = new EFI_RNG_ALGORITHM[(int) byteCount / sizeof(EFI_RNG_ALGORITHM)];
                fixed (EFI_RNG_ALGORITHM* pRNGAlgorithmList = rngAlgorithmList)
                {
                    return _getInfo(pThis, &byteCount, pRNGAlgorithmList);
                }
            }
        }

        /// <summary>
        /// <para>This function fills the <paramref name="rngValue"/> buffer with random bytes<!-- from the specified RNG algorithm-->.</para>
        /// <para>The driver must not reuse random bytes across calls to this function.</para>
        /// <para>It is the caller’s responsibility to allocate the <paramref name="rngValue"/> buffer.</para>
        /// </summary>
        /// <param name="rngValue">A caller-allocated memory buffer filled by the driver with the resulting RNG value.</param>
        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the RNG value was returned successfully.</para>
        /// <!--<para><see cref="EFI_STATUS.EFI_UNSUPPORTED"/> if algorithm specified by RNGAlgorithm is not supported by this driver.</para>-->
        /// <para><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/> if an RNG value could not be retrieved due to a hardware or firmware error.</para>
        /// <para><see cref="EFI_STATUS.EFI_NOT_READY"/> if there is not enough random data available to satisfy the length of <paramref name="rngValue"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="rngValue"/>'s length is zero.</para>
        /// </returns>
        public EFI_STATUS GetRNG(byte[] rngValue)
        {
            fixed (EFI_RNG_PROTOCOL* pThis = &this)
            {
                fixed (byte* prngValue = rngValue)
                {
                    return _getRNG(pThis, null, (nuint)rngValue.Length, prngValue);
                }
            }
        }

        /// <summary>
        /// <para>This function fills the <paramref name="rngValue"/> buffer with random bytes<!-- from the specified RNG algorithm-->.</para>
        /// <para>The driver must not reuse random bytes across calls to this function.</para>
        /// <para>It is the caller’s responsibility to allocate the <paramref name="rngValue"/> buffer.</para>
        /// </summary>
        /// <param name="efiRngAlgorithm">Identifies which supported RNG algorithm to use.</param>
        /// <param name="rngValue">A caller-allocated memory buffer filled by the driver with the resulting RNG values.</param>
        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the RNG value was returned successfully.</para>
        /// <!--<para><see cref="EFI_STATUS.EFI_UNSUPPORTED"/> if algorithm specified by RNGAlgorithm is not supported by this driver.</para>-->
        /// <para><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/> if an RNG value could not be retrieved due to a hardware or firmware error.</para>
        /// <para><see cref="EFI_STATUS.EFI_NOT_READY"/> if there is not enough random data available to satisfy the length of <paramref name="rngValue"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="rngValue"/>'s length is zero.</para>
        /// </returns>
        public EFI_STATUS GetRNG(EFI_RNG_ALGORITHM efiRngAlgorithm, byte[] rngValue)
        {
            fixed (EFI_RNG_PROTOCOL* pThis = &this)
            {
                fixed (byte* prngValue = rngValue)
                {
                    return _getRNG(pThis, &efiRngAlgorithm, (nuint)rngValue.Length, prngValue);
                }
            }
        }

        public static readonly EFI_GUID Guid = new(0x3152bca5, 0xeade, 0x433d, 0x86, 0x2e, 0xc0, 0x1c, 0xdc, 0x29, 0x1f, 0x44);
    }
}

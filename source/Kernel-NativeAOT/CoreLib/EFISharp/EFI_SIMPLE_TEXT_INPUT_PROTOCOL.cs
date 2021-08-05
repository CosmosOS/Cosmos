using System;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFI_SIMPLE_TEXT_INPUT_PROTOCOL
    {
        private readonly IntPtr _pad;
        private readonly delegate*<EFI_SIMPLE_TEXT_INPUT_PROTOCOL*, EFI_INPUT_KEY*, EFI_STATUS> _readKeyStroke;
        public readonly IntPtr _waitForKey;

        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the keystroke information was returned.</para>
        /// <para><see cref="EFI_STATUS.EFI_NOT_READY"/> if there was no keystroke data available.</para>
        /// <para><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/> if the keystroke information was not returned due to hardware errors.</para>
        /// </returns>
        public EFI_STATUS ReadKeyStroke(EFI_INPUT_KEY* key)
        {
            fixed (EFI_SIMPLE_TEXT_INPUT_PROTOCOL* _this = &this)
            {
                return _readKeyStroke(_this, key);
            }
        }
    }
}

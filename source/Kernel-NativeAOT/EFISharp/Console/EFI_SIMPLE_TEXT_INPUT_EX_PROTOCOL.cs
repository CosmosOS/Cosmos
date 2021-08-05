using System;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    /// <summary>This protocol is used to obtain input from the ConsoleIn device.</summary>
    /// <remarks>The EFI specification requires that the EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL supports the same languages as the corresponding EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL
    {
        public static readonly EFI_GUID EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL_GUID =
            new(0xdd9e7534, 0x7762, 0x4698, 0x8c, 0x14, 0xf5, 0x85, 0x17, 0xa6, 0x25, 0xaa);

        private readonly IntPtr _pad1;
        private readonly delegate*<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, EFI_KEY_DATA*, EFI_STATUS> _readKeyStrokeEx;
        /// <summary>Event to use with WaitForEvent() to wait for a key to be available. An Event will only be triggered if KeyData.Key has information contained within it.</summary>
        public readonly EFI_EVENT WaitForKeyEx;
        private readonly IntPtr _pad2;
        private readonly delegate*<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, EFI_KEY_DATA*, delegate*<EFI_KEY_DATA*,
            EFI_STATUS>, EFIKeyNotifyHandle*, EFI_STATUS> _registerKeyNotify;

        //TODO Support Scan codes and SetState which should be called on startup to ensure EFI_KEY_STATE_EXPOSED is set
        /// <summary>Reads the next keystroke from the input device.</summary>
        /// <remarks>The <paramref name="keyData"/>.Key.UnicodeChar is the actual printable character or is zero if the key does not represent a printable character (control key, function key, etc.).
        /// <para>The <paramref name="keyData"/>.KeyState is the modifier shift state for the character reflected in <paramref name="keyData"/>.Key.UnicodeChar <!-- or <paramref name="keyData"/>.Key.ScanCode-->.</para>
        /// <para>When interpreting the data from this function, it should be noted that if a class of printable characters that are normally adjusted by shift modifiers (e.g. Shift Key + "f" key) would be presented solely as a <paramref name="keyData"/>.Key.UnicodeChar without the associated shift state.</para>
        /// <para>This of course would not typically be the case for non-printable characters such as the pressing of the Right Shift Key + F10 key since the corresponding returned data would be reflected <!--both--> in the <paramref name="keyData"/>.KeyState.KeyShiftState <!--and <paramref name="keyData"/>.Key.ScanCode--> value<!--s-->.</para>
        /// <para> It should also be noted that certain input devices may not be able to produce shift or toggle state information, and in those cases the high order bit in the respective Toggle and Shift state fields should not be active.</para>
        /// <para>With <see cref="EFI_KEY_TOGGLE_STATE.EFI_KEY_STATE_EXPOSED"/> bit enabled, this function will allow the return of incomplete keystrokes such as the holding down of certain keys which are expressed as a part of KeyState when there is no Key data.</para>
        /// </remarks>
        /// <param name="keyData">A buffer that is filled in with the keystroke state data for the key that was pressed.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The keystroke information was returned.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_NOT_READY"/></term>
        ///         <description>There was no keystroke data available. Current <paramref name="keyData"/>.KeyState values are exposed if <see cref="EFI_KEY_TOGGLE_STATE.EFI_KEY_STATE_EXPOSED"/> is set.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_DEVICE_ERROR"/></term>
        ///         <description>The keystroke information was not returned due to hardware errors.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS ReadKeyStrokeEx(out EFI_KEY_DATA keyData)
        {
            fixed (EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL* pThis = &this)
            {
                fixed (EFI_KEY_DATA* pKey = &keyData)
                {
                    return _readKeyStrokeEx(pThis, pKey);
                }
            }
        }

        /// <summary>Register a notification function for a particular keystroke for the input device.</summary>
        /// <param name="keyData">A buffer that is filled in with the keystroke information for the key that was pressed.
        /// <para>If KeyData.Key, KeyData.KeyState.KeyToggleState and KeyData.KeyState.KeyShiftState are 0, then any incomplete keystroke will trigger a notification of the <paramref name="keyNotificationFunction"/>.</para></param>
        /// <param name="keyNotificationFunction">Points to the function to be called when the key sequence specified by <paramref name="keyData"/> is typed.</param>
        /// <param name="notifyHandle">Points to the unique handle assigned to the registered notification</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>Key notify was registered successfully</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_OUT_OF_RESOURCES"/></term>
        ///         <description>Unable to allocate necessary data structures.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS RegisterKeyNotify(EFI_KEY_DATA keyData,
            delegate*<EFI_KEY_DATA*, EFI_STATUS> keyNotificationFunction, out EFIKeyNotifyHandle notifyHandle)
        {
            fixed (EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL* pThis = &this)
            {
                fixed (EFIKeyNotifyHandle* pNotifyHandle = &notifyHandle)
                {
                    return _registerKeyNotify(pThis, &keyData, keyNotificationFunction, pNotifyHandle);
                }
            }
        }
    }
}

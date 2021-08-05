using System;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace EfiSharp
{
    //TODO Use ref/out instead of pointers when not used for arrays
    //TODO Use array instead of pointer arrays
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFI_BOOT_SERVICES
    {
        private readonly EFI_TABLE_HEADER Hdr;

        // Task Priority Services
        private readonly IntPtr _pad1;
        private readonly IntPtr _pad2;

        // Memory Services
        private readonly IntPtr _pad3;
        private readonly IntPtr _pad4;
        private readonly IntPtr _pad5;
        private readonly delegate*<EFI_MEMORY_TYPE, nuint, void**, EFI_STATUS> _allocatePool;
        private readonly delegate*<void*, EFI_STATUS> _freePool;

        // Event & Timer Services
        private readonly IntPtr _pad8;
        private readonly IntPtr _pad9;
        private readonly delegate*<uint, EFI_EVENT*, uint*, EFI_STATUS> _waitForEvent;
        private readonly IntPtr _pad10;
        private readonly IntPtr _pad11;
        private readonly delegate*<EFI_EVENT, EFI_STATUS> _checkEvent;

        // Protocol Handler Services
        //This is InstallProtocolInterface and is ignored in favour of OpenProtocol
        private readonly IntPtr _pad13;
        private readonly IntPtr _pad14;
        private readonly IntPtr _pad15;
        //This is HandleProtocol and is ignored in favour of OpenProtocol
        private readonly IntPtr _pad16;
        private readonly void* _pad17;
        private readonly IntPtr _pad18;
        private readonly delegate*<EFI_LOCATE_SEARCH_TYPE, EFI_GUID*, void*, nuint*, EFI_HANDLE*, EFI_STATUS>
            _locateHandle;
        private readonly IntPtr _pad20;
        private readonly IntPtr _pad21;

        // Image Services
        private readonly IntPtr _pad22;
        private readonly IntPtr _pad23;
        private readonly delegate*<EFI_HANDLE, EFI_STATUS, nuint, char*, EFI_STATUS> _exit;
        private readonly IntPtr _pad25;
        private readonly IntPtr _pad26;

        // Miscellaneous Services
        private readonly IntPtr _pad27;
        private readonly IntPtr _pad28;
        private readonly delegate*<nuint, ulong, nuint, char*, EFI_STATUS> _setWatchDogTimer;

        // DriverSupport Services
        private readonly IntPtr _pad30;
        private readonly IntPtr _pad31;

        // Open and Close Protocol Services
        private readonly delegate*<EFI_HANDLE, EFI_GUID*, void**, EFI_HANDLE, EFI_HANDLE, EFI_OPEN_PROTOCOL, EFI_STATUS> _openProtocol;
        private readonly IntPtr _pad32;
        private readonly IntPtr _pad33;

        private readonly IntPtr _pad34;
        private readonly IntPtr _pad35;
        private readonly IntPtr _pad36;
        private readonly IntPtr _pad37;
        private readonly IntPtr _pad38;
        private readonly IntPtr _pad39;

        //Miscellaneous Services
        private readonly delegate*<void*, void*, nuint, void> _copyMem;
        private readonly delegate*<void*, nuint, byte, void> _setMem;

        //TODO Add summary and params descriptions
        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if allocation was successful.</para>
        /// <para><see cref="EFI_STATUS.EFI_OUT_OF_RESOURCES"/> if there was not enough memory free.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="poolType"/> was <see cref="EFI_MEMORY_TYPE.EfiPersistentMemory"/>, <see cref="EFI_MEMORY_TYPE.EfiMaxMemoryType"/> or an undefined type higher than that.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="buffer"/> was null.</para>
        /// </returns>
        public EFI_STATUS AllocatePool(EFI_MEMORY_TYPE poolType, nuint size, out IntPtr buffer)
        {
            fixed (IntPtr* pBuffer = &buffer)
            {
                return _allocatePool(poolType, size, (void**)pBuffer);
            }
        }

        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the freeing was successful.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="buffer"/> was invalid.</para>
        /// </returns>
        public EFI_STATUS FreePool(IntPtr buffer) => _freePool((void*)buffer);

        /// <summary>
        /// Stops execution until an event is signaled.
        /// </summary>
        /// <param name="events">An array of <see cref="EFI_EVENT"/> to wait on. Only one needs to be signaled for this function tor return.</param>
        /// <param name="index">Index of the event which satisfied the wait condition.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The event indicated by <paramref name="index"/> was signaled.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/></term>
        ///         <description>The size of <paramref name="events"/> is zero.</description>
        ///     </item>
        ///      <!--<item>
        ///         <term><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/></term>
        ///         <description>The event indicated by <paramref name="index"/> is of type EVT_NOTIFY_SIGNAL.</description>
        ///     </item>-->
        ///      <!--<item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The current TPL is not TPL_APPLICATION.</description>
        ///     </item>-->
        /// </list>
        /// </returns>
        //TODO Add TPL and EVT_NOTIFY_SIGNAL?
        //TODO Ensure this works for arrays with multiple items
        public EFI_STATUS WaitForEvent(EFI_EVENT[] events, out uint index)
        {
            fixed (EFI_EVENT* pEvents = events)
            {
                fixed (uint* pIndex = &index)
                {
                    return _waitForEvent((uint)events.Length, pEvents, pIndex);
                }
            }
        }

        /// <summary>
        /// Stops execution until an event is signaled.
        /// </summary>
        /// <param name="_event">An <see cref="EFI_EVENT"/> to wait on. Only one needs to be signaled for this function tor return.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The event <paramref name="_event"/> was signaled.</description>
        ///     </item>
        ///      <!--<item>
        ///         <term><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/></term>
        ///         <description>The event indicated by <paramref name="index"/> is of type EVT_NOTIFY_SIGNAL.</description>
        ///     </item>-->
        ///      <!--<item>
        ///         <term><see cref="EFI_STATUS.EFI_UNSUPPORTED"/></term>
        ///         <description>The current TPL is not TPL_APPLICATION.</description>
        ///     </item>-->
        /// </list>
        /// </returns>
        //TODO Add TPL and EVT_NOTIFY_SIGNAL?
        public EFI_STATUS WaitForEvent(EFI_EVENT _event)
        {
            uint index;
            return _waitForEvent(1, &_event, &index);
        }

        /// <summary>
        /// Checks whether an event is in the signaled state.
        /// </summary>
        /// <param name="_event">The event to check.</param>
        /// <returns>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_SUCCESS"/></term>
        ///         <description>The event <paramref name="_event"/> is in the signaled state.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="EFI_STATUS.EFI_NOT_READY"/></term>
        ///         <description>The event <paramref name="_event"/> is not in the signaled state.</description>
        ///     </item>
        ///      <item>
        ///         <term><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/></term>
        ///         <description><paramref name="_event"/> is of type EVT_NOTIFY_SIGNAL.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public EFI_STATUS CheckEvent(EFI_EVENT _event)
        {
            return _checkEvent(_event);
        }

        //TODO Merge with the other public version below when nullable<T> is supported
        /// <summary>
        /// <para>This function returns an array of handles in <paramref name="buffer"/> that match the <paramref name="searchType"/> request.</para>
        /// </summary>
        /// <param name="searchType">Specifies which handle(s) are to be returned.</param>
        /// <param name="buffer">The buffer in which the array is returned.</param>
        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the array of handles was returned in <paramref name="buffer"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_NOT_FOUND"/> if no handles match the search.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="searchType"/> is not a member of <see cref="EFI_LOCATE_SEARCH_TYPE"/>.</para>
        /// <!--TODO Add RegisterProtocolNotify-->
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="searchType"/> is <see cref="EFI_LOCATE_SEARCH_TYPE.ByRegisterNotify"/><!-- and SearchKey is NULL-->.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="searchType"/> is <see cref="EFI_LOCATE_SEARCH_TYPE.ByProtocol"/>.</para>
        /// </returns>
        public EFI_STATUS LocateHandle(EFI_LOCATE_SEARCH_TYPE searchType, out EFI_HANDLE[] buffer)
        {
            if (searchType != EFI_LOCATE_SEARCH_TYPE.ByProtocol) return LocateHandle(searchType, null, out buffer);

            buffer = null;
            return EFI_STATUS.EFI_INVALID_PARAMETER;
        }

        /// <summary>
        /// <para>This function returns an array of handles in <paramref name="buffer"/> that match the <paramref name="searchType"/> request.</para>
        /// </summary>
        /// <param name="searchType">Specifies which handle(s) are to be returned.</param>
        /// <param name="protocol">Specifies the protocol to search by. This parameter is only valid if <paramref name="searchType"/> is <see cref="EFI_LOCATE_SEARCH_TYPE.ByProtocol"/>.</param>
        /// <param name="buffer">The buffer in which the array is returned.</param>
        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if the array of handles was returned in <paramref name="buffer"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_NOT_FOUND"/> if no handles match the search.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="searchType"/> is not a member of <see cref="EFI_LOCATE_SEARCH_TYPE"/>.</para>
        /// <!--TODO Add RegisterProtocolNotify-->
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="searchType"/> is <see cref="EFI_LOCATE_SEARCH_TYPE.ByRegisterNotify"/><!-- and SearchKey is NULL-->.</para>
        /// </returns>
        public EFI_STATUS LocateHandle(EFI_LOCATE_SEARCH_TYPE searchType, EFI_GUID protocol, out EFI_HANDLE[] buffer)
        {
            return LocateHandle(searchType, &protocol, out buffer);
        }

        private EFI_STATUS LocateHandle(EFI_LOCATE_SEARCH_TYPE searchType, EFI_GUID* pProtocol, out EFI_HANDLE[] buffer)
        {
            nuint byteCount = 0;
            EFI_STATUS status = _locateHandle(searchType, pProtocol, null, &byteCount, null);

            if (status != EFI_STATUS.EFI_BUFFER_TOO_SMALL)
            {
                buffer = null;
                return status;
            }

            buffer = new EFI_HANDLE[(int)byteCount / sizeof(EFI_HANDLE)];
            fixed (EFI_HANDLE* pBuffer = buffer)
            {
                return _locateHandle(searchType, pProtocol, null, &byteCount, pBuffer);
            }
        }

        //TODO Ensure this works with an array
        public EFI_STATUS Exit(EFI_HANDLE imageHandle, EFI_STATUS exitStatus, char[] exitData = null)
        {
            if (exitData == null)
            {
                return _exit(imageHandle, exitStatus, 0, null);
            }

            fixed (char* pExitData = exitData)
            {
                return _exit(imageHandle, exitStatus, (nuint)exitData.Length, pExitData);
            }
        }

        //TODO Ensure this works with an array
        public EFI_STATUS SetWatchdogTimer(nuint timeout, ulong watchdogCode, char[] watchdogData = null)
        {
            if (watchdogData == null)
            {
                return _setWatchDogTimer(timeout, watchdogCode, 0, null);
            }

            fixed (char* pWatchdogData = watchdogData)
            {
                return _setWatchDogTimer(timeout, watchdogCode, (nuint)watchdogData.Length, pWatchdogData);
            }
        }

        /// <returns>
        /// <para><see cref="EFI_STATUS.EFI_SUCCESS"/> if <paramref name="protocol"/> was opened, added to the list of open protocols and returned in <paramref name="_interface"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_UNSUPPORTED"/> if <paramref name="handle"/> does not support the given <paramref name="protocol"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="protocol"/> was NULL.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="_interface"/> was NULL and <paramref name="attributes"/> was not <see cref="EFI_OPEN_PROTOCOL.TEST_PROTOCOL"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="handle"/> is NULL.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="attributes"/> is not a legal value.</para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="agentHandle"/> is null and <paramref name="attributes"/> is one of: <see cref="EFI_OPEN_PROTOCOL.BY_CHILD_CONTROLLER"/>, <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>, <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/>, or <see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/></para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="controllerHandle"/> is null and <paramref name="attributes"/> is one of: <see cref="EFI_OPEN_PROTOCOL.BY_CHILD_CONTROLLER"/>, <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>, or <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/></para>
        /// <para><see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> if <paramref name="handle"/> is equal to <paramref name="controllerHandle"/> and <paramref name="attributes"/> is <see cref="EFI_OPEN_PROTOCOL.BY_CHILD_CONTROLLER"/></para>
        /// <para><see cref="EFI_STATUS.EFI_ACCESS_DENIED"/> if <paramref name="attributes"/> is <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>, or <see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/> and the current list of open protocols contains one with an attribute of either <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/>, or <see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/></para>
        /// <para><see cref="EFI_STATUS.EFI_ACCESS_DENIED"/> if <paramref name="attributes"/> is <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/> and the current list of open protocols contains one with an attribute of <see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_ACCESS_DENIED"/> if <paramref name="attributes"/> is <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>, or <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/> and the current list of open protocols contains one with the same attribute and an agent handle that is different to <paramref name="agentHandle"/>.</para>
        /// <para><see cref="EFI_STATUS.EFI_ACCESS_DENIED"/> if <paramref name="attributes"/> is <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/> or <see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/> and the current list of open protocols contains one with an attribute of <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/> that could not be removed when EFI_BOOT_SERVICES.DisconnectController() was called on it.</para>
        /// <para><see cref="EFI_STATUS.EFI_ALREADY_STARTED"/> if <paramref name="attributes"/> is <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>, or <see cref="EFI_OPEN_PROTOCOL.BY_DRIVER"/>|<see cref="EFI_OPEN_PROTOCOL.EXCLUSIVE"/> and the current list of open protocols contains one with the same attribute and an agent handle that is the same as <paramref name="agentHandle"/>.</para>
        /// </returns>
        public EFI_STATUS OpenProtocol(EFI_HANDLE handle, EFI_GUID protocol, out void* _interface, EFI_HANDLE agentHandle,
            EFI_HANDLE controllerHandle, EFI_OPEN_PROTOCOL attributes)
        {
            fixed (void** pInterface = &_interface)
            {
                return _openProtocol(handle, &protocol, pInterface, agentHandle, controllerHandle, attributes);
            }
        }

        //TODO Describe copy and set
        //TODO Use ref byte instead of void*?
        public void CopyMem(void* destination, void* source, nuint length)
        {
            _copyMem(destination, source, length);
        }

        public void SetMem(void* buffer, nuint size, byte value)
        {
            _setMem(buffer, size, value);
        }
    }
}

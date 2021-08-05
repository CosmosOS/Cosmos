namespace EfiSharp
{
    /// <summary>
    /// Used to indicate the search type used for <see cref="EFI_BOOT_SERVICES.LocateHandle(EFI_LOCATE_SEARCH_TYPE, EFI_GUID, out EFI_HANDLE[])"/>
    /// </summary>
    public enum EFI_LOCATE_SEARCH_TYPE
    {
        /// <summary>
        /// Indicates that protocol and searchKey should be ignored and that the function should return an array of every handle in the system. 
        /// </summary>
        AllHandles,
        /// <summary>
        /// <!--<para>Indicates that searchKey supplies the Registration value returned by <see cref="EFI_BOOT_SERVICES.RegisterProtocolNotify"/>.</para>
        /// <para>The function then returns the next handle that is new for the registration.</para>
        /// <para>Note that only one handle is returned at a time, starting with the first, and the the function call must be looped until no more handles are returned.</para>
        /// <para>Also note that protocol is ignored for this search type.</para>-->
        /// Currently unused, using this option will cause <see cref="EFI_STATUS.EFI_INVALID_PARAMETER"/> to be returned.
        /// </summary>
        ByRegisterNotify,
        /// <summary>
        /// Indicates that all handles that support protocol are returned. Note that searchKey is ignored for this search type.
        /// </summary>
        ByProtocol
    }
}
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Boot.Limine;

/// <summary>
/// Limine EFI System Table request.
/// Provides a pointer to the EFI system table, allowing access to EFI Runtime Services
/// (e.g. GetTime) which remain valid even after ExitBootServices.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LimineEfiSystemTableRequest()
{
    public readonly LimineID ID = new(0x5ceba5163eaaf6d6, 0x0a6981610cf65fcc);
    public readonly ulong Revision = 0;
    public readonly LimineEfiSystemTableResponse* Response;
}

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LimineEfiSystemTableResponse
{
    public readonly ulong Revision;
    /// <summary>Pointer to the EFI system table.</summary>
    public readonly EfiSystemTable* Address;
}

// ── Minimal EFI type definitions ────────────────────────────────────────────

/// <summary>
/// Common header present at the start of every EFI table.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct EfiTableHeader
{
    public ulong Signature;
    public uint Revision;
    public uint HeaderSize;
    public uint CRC32;
    public uint Reserved;
}

/// <summary>
/// EFI_TIME as defined in the UEFI specification.
/// Size = 16 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct EfiTime
{
    public ushort Year;        // 1900–9999
    public byte Month;       // 1–12
    public byte Day;         // 1–31
    public byte Hour;        // 0–23
    public byte Minute;      // 0–59
    public byte Second;      // 0–59
    public byte Pad1;
    public uint Nanosecond;  // 0–999,999,999
    public short TimeZone;    // minutes offset from UTC; 0x07FF = unspecified
    public byte Daylight;
    public byte Pad2;
}

/// <summary>
/// Minimal layout of EFI_RUNTIME_SERVICES up to and including GetTime.
/// GetTime is the first function pointer after the table header (offset 24).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EfiRuntimeServices
{
    public EfiTableHeader Hdr;   // 24 bytes

    /// <summary>
    /// EFI_STATUS GetTime(OUT EFI_TIME *Time, OUT EFI_TIME_CAPABILITIES *Capabilities OPTIONAL)
    /// Returns 0 (EFI_SUCCESS) on success.
    /// Kept untyped: EFIAPI is the Microsoft x64 convention on x86-64, not the
    /// System V one a <c>delegate* unmanaged</c> call would use, so the kernel
    /// invokes it through <c>Cosmos.Kernel.Core.Bridge.EfiNative.Call</c>.
    /// </summary>
    public void* GetTime; // offset 24
}

/// <summary>
/// Minimal layout of EFI_SYSTEM_TABLE up to and including RuntimeServices.
/// RuntimeServices pointer is at offset 88.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EfiSystemTable
{
    public EfiTableHeader Hdr;                   // offset  0, size 24
    public char* FirmwareVendor;        // offset 24, size  8
    public uint FirmwareRevision;      // offset 32, size  4
    private uint _pad;                  // offset 36, size  4 (alignment)
    public void* ConsoleInHandle;       // offset 40, size  8
    public void* ConIn;                 // offset 48, size  8
    public void* ConsoleOutHandle;      // offset 56, size  8
    public void* ConOut;                // offset 64, size  8
    public void* StandardErrorHandle;   // offset 72, size  8
    public void* StdErr;                // offset 80, size  8
    public EfiRuntimeServices* RuntimeServices;       // offset 88, size  8
}

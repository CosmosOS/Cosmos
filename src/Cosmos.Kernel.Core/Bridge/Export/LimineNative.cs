using System.Runtime.InteropServices;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.Utilities;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Wrapper for C code to access managed Limine data (RSDP address, HHDM offset).
/// NOTE: This is a data accessor wrapper - C code gets managed data, then continues in C.
/// We do NOT call C code from managed code - only provide data access.
/// </summary>
internal static unsafe class LimineNative
{
    /// <summary>
    /// Wrapper to expose Limine RSDP address to C bootstrap for LAI ACPI initialization
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__get_limine_rsdp_address")]
    public static void* GetRsdpAddress()
    {
        if (Limine.Rsdp.Response != null)
        {
            return Limine.Rsdp.Response->Address;
        }
        return null;
    }

    /// <summary>
    /// Expose Limine HHDM offset to C bootstrap for physical-to-virtual address translation.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__get_limine_hhdm_offset")]
    public static ulong GetHhdmOffset()
    {
        if (Limine.HHDM.Response != null)
        {
            return Limine.HHDM.Response->Offset;
        }
        return 0;
    }

    /// <summary>
    /// Wrapper to expose Limine cmdline pointer.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__get_limine_cmd_line")]
    public static byte* GetCmdLine()
    {
        if (Limine.ExecutableCmdline.Response != null)
        {
            return Limine.ExecutableCmdline.Response->Cmdline;
        }

        return null;
    }

    /// <summary>
    /// Wrapper to expose the <see cref="ArgvParser.BuildArgv"/> method, utility to parse byte* into argv style.
    /// </summary>
    /// <param name="input">Input pointer to be parsed</param>
    /// <param name="argc">Pointer to Save the number of parameters</param>
    /// <returns>The argv pointer</returns>
    /// <remarks>The string "cosmos" is added as the parameter at index 0 in result, this is to take place of the exe.</remarks>
    [UnmanagedCallersOnly(EntryPoint = "__build_argv")]
    public static byte** __build_argv(byte* input, int* argc)
    {
        return ArgvParser.BuildArgv(input, argc);
    }
}

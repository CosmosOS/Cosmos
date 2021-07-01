using System;
using System.Runtime.InteropServices;

public unsafe class Program
{
    static void Main() { }

    static EFI_SYSTEM_TABLE* efitable;

    [System.Runtime.RuntimeExport("EfiMain")]
    public static long EfiMain(IntPtr imageHandle, EFI_SYSTEM_TABLE* systemTable)
    {
        efitable = systemTable;

        Write("Cosmos EFI Bootloader booted successfully.\r\n");
        Write("Loader version: 1.0\r\n");
        Write("EFI Version: ");
        //Write(systemTable->FirmwareRevision.ToString());
        Write("\r\n");
        Write("EFI Firmware Vendor: ");
        Write(systemTable->FirmwareVendor);
        Write("\r\n");
        while (true);
    }

    public static void Write(string str)
    {
        fixed (char* pHello = str)
        {
            Write(pHello);
        }
    }

    public static void Write(char *str)
    {
        efitable->ConOut->OutputString(efitable->ConOut, str);
    }
}
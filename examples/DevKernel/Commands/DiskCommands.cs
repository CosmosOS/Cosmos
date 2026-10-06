using System;
using System.Diagnostics;
using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.System.Storage;
using DevKernel.Shell;
using DevKernel.Storage;

namespace DevKernel.Commands;

/// <summary>
/// Raw block-device access: geometry, sector dumps, and a write/read roundtrip.
/// </summary>
internal static class DiskCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Storage";

    /// <summary>Block count passed to read/write when operating on a single sector.</summary>
    private const ulong SingleBlock = 1;

    /// <summary>Number of bytes hex-dumped by the diskread command.</summary>
    private const int HexDumpLengthBytes = 64;

    /// <summary>Number of bytes shown per line of the diskread hex dump.</summary>
    private const int HexDumpBytesPerLine = 16;

    /// <summary>Format string rendering one dumped byte as two uppercase hex digits.</summary>
    private const string HexByteFormat = "X2";

    /// <summary>LBA the disktest roundtrip writes to; far enough in to miss the boot sector.</summary>
    private const ulong DiskTestLba = 0xCAFE;

    /// <summary>Sentinel meaning the readback matched the written pattern at every byte.</summary>
    private const int NoMismatch = -1;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "diskinfo",
                Aliases = ["lsdisk"],
                Usage = "diskinfo",
                Description = "Show storage devices, geometry and partition table type",
                Execute = static (context, args) =>
                {
                    Terminal.Header("Storage Information:");
                    if (StorageView.RequireDevices())
                    {
                        StorageView.PrintDevices(detailed: true);
                    }
                },
            },
            new ShellCommand
            {
                Name = "diskread",
                Usage = "diskread <lba>",
                Description = "Read 1 block, hex-dump first 64 bytes",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetULong(0, out ulong lba))
                    {
                        args.PrintUsage();
                        return;
                    }

                    DiskRead(lba);
                },
            },
            new ShellCommand
            {
                Name = "diskwrite",
                Usage = "diskwrite <lba> <hex-byte>",
                Description = "Fill 1 block with byte value (e.g. A5) and write",
                MinArgs = 2,
                MaxArgs = 2,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetULong(0, out ulong lba) || !args.TryGetHexByte(1, out byte value))
                    {
                        args.PrintUsage();
                        return;
                    }

                    DiskWrite(lba, value);
                },
            },
            new ShellCommand
            {
                Name = "disktest",
                Usage = "disktest",
                Description = "Run a quick write/read roundtrip on the primary disk",
                Execute = static (context, args) => DiskTest(),
            });
    }

    /// <summary>Returns the primary device, reporting its absence when there is none.</summary>
    private static IBlockDevice? RequirePrimaryDevice()
    {
        IBlockDevice? device = StorageManager.PrimaryDevice;
        if (device == null)
        {
            Terminal.Error("No primary storage device.");
        }

        return device;
    }

    /// <summary>True when <paramref name="lba"/> addresses a block that exists on <paramref name="device"/>.</summary>
    private static bool IsAddressable(IBlockDevice device, ulong lba, string what)
    {
        if (lba < device.BlockCount)
        {
            return true;
        }

        Terminal.Error($"{what} {lba} out of range (max {device.BlockCount - 1}).");
        return false;
    }

    private static void DiskRead(ulong lba)
    {
        IBlockDevice? device = RequirePrimaryDevice();
        if (device == null || !IsAddressable(device, lba, "LBA"))
        {
            return;
        }

        Span<byte> buffer = new byte[device.BlockSize];
        device.ReadBlock(lba, SingleBlock, buffer);

        Terminal.Header($"Block {lba} (first {HexDumpLengthBytes} bytes):");

        int show = (int)Math.Min((ulong)HexDumpLengthBytes, device.BlockSize);
        for (int i = 0; i < show; i++)
        {
            if (i > 0 && i % HexDumpBytesPerLine == 0)
            {
                Console.WriteLine();
            }

            Console.Write(buffer[i].ToString(HexByteFormat));
            Console.Write(' ');
        }

        Console.WriteLine();
    }

    private static void DiskWrite(ulong lba, byte value)
    {
        IBlockDevice? device = RequirePrimaryDevice();
        if (device == null || !IsAddressable(device, lba, "LBA"))
        {
            return;
        }

        Span<byte> buffer = new byte[device.BlockSize];
        buffer.Fill(value);
        device.WriteBlock(lba, SingleBlock, buffer);

        Terminal.Success($"Wrote {device.BlockSize} bytes of 0x{value:X2} to LBA {lba}.");
    }

    private static void DiskTest()
    {
        IBlockDevice? device = RequirePrimaryDevice();
        if (device == null || !IsAddressable(device, DiskTestLba, "Test LBA"))
        {
            return;
        }

        // Save the original contents so the roundtrip is non-destructive on
        // whatever image is attached.
        Span<byte> original = new byte[device.BlockSize];
        device.ReadBlock(DiskTestLba, SingleBlock, original);

        // Tick-derived seed: with a fixed pattern the test false-passes on
        // repeat runs — after one success the pattern persists on the image,
        // so a driver whose writes silently no-op would still read the stale
        // pattern back.
        byte seed = (byte)Stopwatch.GetTimestamp();
        Span<byte> writeBuf = new byte[device.BlockSize];
        for (int i = 0; i < (int)device.BlockSize; i++)
        {
            writeBuf[i] = (byte)(i ^ seed);
        }

        device.WriteBlock(DiskTestLba, SingleBlock, writeBuf);

        Span<byte> readBuf = new byte[device.BlockSize];
        device.ReadBlock(DiskTestLba, SingleBlock, readBuf);

        int mismatch = NoMismatch;
        for (int i = 0; i < (int)device.BlockSize; i++)
        {
            if (writeBuf[i] != readBuf[i])
            {
                mismatch = i;
                break;
            }
        }

        // Restore before reporting so even a failed compare leaves the image as
        // we found it.
        device.WriteBlock(DiskTestLba, SingleBlock, original);

        if (mismatch != NoMismatch)
        {
            Terminal.Error($"Mismatch at byte {mismatch}: wrote 0x{writeBuf[mismatch]:X2}, read 0x{readBuf[mismatch]:X2}.");
            return;
        }

        Terminal.Success(
            $"Disk W/R roundtrip OK at LBA {DiskTestLba} ({device.BlockSize} bytes, seed 0x{seed:X2}), original contents restored.");
    }
}

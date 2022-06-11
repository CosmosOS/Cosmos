//#define COSMOSDEBUG

using System;
using System.Collections.Generic;

namespace Cosmos.HAL.BlockDevice;

/// <summary>
///     Partition class. Used to read and write blocks of data.
/// </summary>
public class Partition : BlockDevice
{
    public static List<Partition> Partitions = new();

    /// <summary>
    ///     Hosting device.
    /// </summary>
    public readonly BlockDevice Host;

    /// <summary>
    ///     Starting sector.
    /// </summary>
    public readonly ulong StartingSector;

    /// <summary>
    ///     Create new instance of the <see cref="Partition" /> class.
    /// </summary>
    /// <param name="aHost">A hosting device.</param>
    /// <param name="aStartingSector">A starting sector.</param>
    /// <param name="aSectorCount">A sector count.</param>
    public Partition(BlockDevice aHost, ulong StartingSector, ulong SectorCount)
    {
        Host = aHost;
        this.StartingSector = StartingSector;
        mBlockCount = SectorCount;
        mBlockSize = Host.BlockSize;
    }

    public override BlockDeviceType Type => Host.Type;

    /// <summary>
    ///     Read block from partition.
    /// </summary>
    /// <param name="aBlockNo">A block to read from.</param>
    /// <param name="aBlockCount">A number of blocks in the partition.</param>
    /// <param name="aData">A data that been read.</param>
    /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
    /// <exception cref="Exception">Thrown when data size invalid.</exception>
    public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
    {
        Global.mDebugger.SendInternal("-- Partition.ReadBlock --");
        Global.mDebugger.SendInternal($"aBlockNo = {aBlockNo}");
        Global.mDebugger.SendInternal($"aBlockCount = {aBlockCount}");
        CheckDataSize(aData, aBlockCount);
        var xHostBlockNo = StartingSector + aBlockNo;
        CheckBlockNo(xHostBlockNo, aBlockCount);
        Host.ReadBlock(xHostBlockNo, aBlockCount, ref aData);
        Global.mDebugger.SendInternal("Returning -- Partition.ReadBlock --");
    }

    /// <summary>
    ///     Write block to partition.
    /// </summary>
    /// <param name="aBlockNo">A block number to write to.</param>
    /// <param name="aBlockCount">A number of blocks in the partition.</param>
    /// <param name="aData">A data to write.</param>
    /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
    /// <exception cref="Exception">Thrown when data size invalid.</exception>
    public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
    {
        CheckDataSize(aData, aBlockCount);
        var xHostBlockNo = StartingSector + aBlockNo;
        CheckBlockNo(xHostBlockNo, aBlockCount);
        Host.WriteBlock(xHostBlockNo, aBlockCount, ref aData);
    }

    /// <summary>
    ///     To string.
    /// </summary>
    /// <returns>string value.</returns>
    public override string ToString() => "Partition";
}

/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.SMBIOS.Table
{
    public class ProcessorInformation : BaseInfo, IProcessorInformation
    {

        public string SocketDesignation { get; private set; }
        public byte ProcessorType { get; private set; }
        public byte ProcessorFamily { get; private set; }
        public string ProcessorManufacturer { get; private set; }
        public ulong ProcessorID { get; private set; }
        public string ProcessorVersion { get; private set; }
        public byte Voltage { get; private set; }
        public ushort ExternalClock { get; private set; }
        public ushort MaxSpeed { get; private set; }
        public ushort CurrentSpeed { get; private set; }
        public byte Status { get; private set; }
        public byte ProcessorUpgrade { get; private set; }
        public ushort L1CacheHandle { get; private set; }
        public ushort L2CacheHandle { get; private set; }
        public ushort L3CacheHandle { get; private set; }
        public string SerialNumber { get; private set; }
        public string AssetTag { get; private set; }
        public string PartNumber { get; private set; }
        public byte CoreCount { get; private set; }
        public byte CoreEnabled { get; private set; }
        public byte ThreadCount { get; private set; }
        public ushort ProcessorCharacteristics { get; private set; }
        public ushort ProcessorFamily2 { get; private set; }

        public ProcessorInformation( BaseTable bt )
        {
            if( bt.TableType != (int) TableTypes.ProcessorInformation )
                return;

            if( bt.SMBIOSMinorVersion >= 0 )
            {
                Handle = bt.Handle;
                HardwareType = bt.TableType;
                SocketDesignation = bt.GetString( bt.Data.Read8Unchecked( 0x04 ) );
                ProcessorType = bt.Data.Read8Unchecked( 0x05 );
                ProcessorFamily = bt.Data.Read8Unchecked( 0x06 );
                ProcessorManufacturer = bt.GetString( bt.Data.Read8Unchecked( 0x07 ) );
                ProcessorID = bt.Data.Read64Unchecked( 0x08 );
                ProcessorVersion = bt.GetString( bt.Data.Read8Unchecked( 0x10 ) );
                Voltage = bt.Data.Read8Unchecked( 0x11 );
                ExternalClock = bt.Data.Read16Unchecked( 0x12 );
                MaxSpeed = bt.Data.Read16Unchecked( 0x14 );
                CurrentSpeed = bt.Data.Read16Unchecked( 0x16 );
                Status = bt.Data.Read8Unchecked( 0x18 );
                ProcessorUpgrade = bt.Data.Read8Unchecked( 0x19 );
            }
            else return;

            if( bt.SMBIOSMinorVersion >= 1 )
            {
                L1CacheHandle = bt.Data.Read16Unchecked( 0x1A );
                L2CacheHandle = bt.Data.Read16Unchecked( 0x1C );
                L3CacheHandle = bt.Data.Read16Unchecked( 0x1E );
            }
            else return;

            if( bt.SMBIOSMinorVersion >= 3 )
            {

                SerialNumber = bt.GetString( bt.Data.Read8Unchecked( 0x20 ) );
                AssetTag = bt.GetString( bt.Data.Read8Unchecked( 0x21 ) );
                PartNumber = bt.GetString( bt.Data.Read8Unchecked( 0x22 ) );
            }
            else return;

            if( bt.SMBIOSMinorVersion >= 5 )
            {

                CoreCount = bt.Data.Read8Unchecked( 0x23 );
                CoreEnabled = bt.Data.Read8Unchecked( 0x24 );
                ThreadCount = bt.Data.Read8Unchecked( 0x25 );
                ProcessorCharacteristics = bt.Data.Read16Unchecked( 0x26 );
            }
            else return;

            if( bt.SMBIOSMinorVersion >= 6 )
            {
                ProcessorFamily2 = bt.Data.Read16Unchecked( 0x28 );
            }
            else return;

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
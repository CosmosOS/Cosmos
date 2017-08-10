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
    public class BIOSTable : BaseInfo, IBIOSInformation
    {
        #region IBIOSInformation Members

        public string Vendor { get; private set; }
        public string BIOSVersion { get; private set; }
        public ushort BIOSStartingAddressSegment { get; private set; }
        public string BIOSReleaseDate { get; private set; }
        public byte BIOSROMSize { get; private set; }
        public ulong BIOSCharacteristics { get; private set; }
        public int BIOSCharacteristicsExtensionByteCount { get; private set; }
        public byte[] BIOSCharacteristicsExtensionBytes { get; private set; }
        public byte SystemBIOSMajorRelease { get; private set; }
        public byte SystemBIOSMinorRelease { get; private set; }
        public byte EmbeddedControllerFirmwareMajorRelease { get; private set; }
        public byte EmbeddedControllerFirmwareMinorRelease { get; private set; }
        #endregion

        public BIOSTable( BaseTable bt )
        {
            if( bt.TableType != 0 )
                //throw new ArgumentException( "Wrong TableType" );
                return;
            HardwareType = bt.TableType; 
            Handle = bt.Handle;

            if( bt.SMBIOSMinorVersion >= 0 )
            {
                Vendor = bt.GetString( bt.Data.Read8Unchecked( 0x04 ) );
                BIOSVersion = bt.GetString( bt.Data.Read8Unchecked( 0x05 ) );
                BIOSStartingAddressSegment = bt.Data.Read16Unchecked( 0x08 );
                BIOSReleaseDate = bt.GetString( bt.Data.Read8Unchecked( 0x08 ) );
                BIOSROMSize = bt.Data.Read8Unchecked( 0x09 );
                BIOSCharacteristics = bt.Data.Read64Unchecked( 0x0A );

                BIOSCharacteristicsExtensionByteCount = (int)bt.TableLength - 0x13;
                BIOSCharacteristicsExtensionBytes = new byte[ BIOSCharacteristicsExtensionByteCount ];

                for( uint i = 0; i < BIOSCharacteristicsExtensionByteCount; i++ )
                    bt.Data.Read8Unchecked( 0x12 + i );

                SystemBIOSMajorRelease = bt.Data.Read8Unchecked( 0x14 );
                SystemBIOSMinorRelease = bt.Data.Read8Unchecked( 0x15 );
                EmbeddedControllerFirmwareMajorRelease = bt.Data.Read8Unchecked( 0x16 );
                EmbeddedControllerFirmwareMinorRelease = bt.Data.Read8Unchecked( 0x17 );
            }           

        }

    }
}
/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.SMBIOS
{
    public class SMBIOS_Data
    {
        //4 Byte string
        public byte[] AnchorString;

        public byte EntryPointStructureChecksum;
        public byte EntryPointLength;
        public byte SMBIOSMajorVersion;
        public byte SMBIOSMinorVersion;
        public ushort MaximumStructureSize;
        public byte EntryPointRevision;

        //5 Byte string 
        public byte[] FormattedArea;

        //5 Byte string 
        public byte[] IntermediateAnchorString;

        public byte IntermediateChecksum;
        public ushort StructureTableLength;
        public uint StructureTableAddress;
        public ushort NumberOfSMBIOSStructures;
        public byte SMBIOSBCDRevision;

        public SMBIOS_Data()
        {

            AnchorString = new byte[ 4 ];
            FormattedArea = new byte[ 5 ];
            IntermediateAnchorString = new byte[ 5 ];
        }

    }
}
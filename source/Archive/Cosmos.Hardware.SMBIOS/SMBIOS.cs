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
    public class SMBIOS
    {
        public uint SMBBIOSAddress;
        public bool HasSMBIOS;
        public uint Signature;
        public SMBIOS_Data SMBIOS_Data;

        public List<BaseTable> TableList;

        //private Dictionary<byte, List<IHardwareType>> TypeBasedList;
        //private Dictionary<ushort, IHardwareType> HandleBasedList;

        public List<BaseInfo> HardwareList;

        public bool CheckSMBIOS()
        {
            HasSMBIOS = false;
            //0x000F0000 Start

            //0x000F69E0 VMWare
            //0x000FFF30 VirtualBox

            //0x000FFFFF End
            SMBBIOSAddress = 0x000F0000;
            
            Cosmos.Kernel.MemoryAddressSpace Memory = new Cosmos.Kernel.MemoryAddressSpace( 0, 0 );

            while( SMBBIOSAddress <= 0x000FFFFF )
            {
                //if( Memory.Get32( SMBBIOSAddress ) == ( uint )0x5F534D5F || // "_SM_"  
                //    Memory.Get32( SMBBIOSAddress ) == ( uint )0x5F4D535F ) // "_MS_" indianess 
                if( Memory.Read8Unchecked( SMBBIOSAddress ) == ( byte )0x5F ) // 
                    if( Memory.Read8Unchecked( SMBBIOSAddress + 1 ) == ( byte )0x53 )
                        if( Memory.Read8Unchecked( SMBBIOSAddress + 2 ) == ( byte )0x4D )
                            if( Memory.Read8Unchecked( SMBBIOSAddress + 3 ) == ( byte )0x5F )
                            {

                                Signature = Memory.Read32Unchecked( SMBBIOSAddress );
                                HasSMBIOS = true;
                                break;
                            }

                SMBBIOSAddress++;
            }

            return HasSMBIOS;
        }

        public int ReadTables()
        {
            if( TableList == null )
                TableList = new List<BaseTable>();
            else if( TableList.Count > 0 )
                return TableList.Count;

            if( SMBIOS_Data.NumberOfSMBIOSStructures == 0 )
                return 0;

            BaseTable bsd;

            uint NextAddress = SMBIOS_Data.StructureTableAddress;

            for( int i = 0; i < SMBIOS_Data.NumberOfSMBIOSStructures; i++ )
            {
                bsd = new BaseTable( NextAddress, SMBIOS_Data.SMBIOSMinorVersion );
                if( bsd.ReadData() == false || bsd.TableType == 127)
                {
                    //Something went wrong or we finished
                    break;
                }

                NextAddress = bsd.EndAddress;
                TableList.Add( bsd ); 
            }



            return SMBIOS_Data.NumberOfSMBIOSStructures;

        }

        //public T GetInterface<T>() where T : IHardwareType 
        //{
        //    Console.WriteLine( " [GetInterface] Start" );
        //    BaseInfo ht;
        //    for( int i = 0; i < HardwareList.Count; i++ )
        //    {
        //        ht = HardwareList[ i ];
        //        if( ht is T )
        //            return (T)((object)ht);
        //    }
        //    Console.WriteLine( " [GetInterface] End" );
        //    throw new Exception( "Interface not found" );
        //}

        //public List<IHardwareType> GetHardwareDescriptorByType( byte Type )
        //{
        //    return TypeBasedList[ Type ]; 
        //}

        //public IHardwareType GetHardwareDescriptorByHandle( ushort Handle )
        //{
        //    return HandleBasedList[ Handle ]; 
        //}

        public List<BaseInfo> GetHardwareDescriptorByType( TableTypes Type )
        {
            //Console.WriteLine( " [GetHardwareDescriptorByType] Start" );
            List<BaseInfo> tmp = new List<BaseInfo>();

            BaseInfo ht;

            for( int i = 0; i < HardwareList.Count; i++ )
            {
                ht = HardwareList[ i ];
                if( ht.HardwareType == (byte)Type )
                    tmp.Add( ht );
            }
            //Console.WriteLine( " [GetHardwareDescriptorByType] End" );
            return tmp;
        }

        public IHardwareType GetHardwareDescriptorByHandle( ushort Handle )
        {
            //Console.WriteLine( " [GetHardwareDescriptorByHandle] Start" );
            BaseInfo ht;

            for( int i = 0; i < HardwareList.Count; i++ )
            {
                ht = HardwareList[ i ];
                if( ht.Handle == Handle )
                    return ht;
            }
            //Console.WriteLine( " [GetHardwareDescriptorByHandle] End" );
            return null;
        }

        public void InterpretData()
        {
            //if( TypeBasedList == null )
            //    TypeBasedList = new Dictionary<byte, List<IHardwareType>>();

            //if( HandleBasedList == null )
            //    HandleBasedList = new Dictionary<ushort, IHardwareType>();

            if( HardwareList == null )
                HardwareList = new List<BaseInfo>();
            BaseTable bt;
            BaseInfo ht;
            for( int i = 0; i < TableList.Count; i++ )
            {
                
                bt = TableList[ i ];
                
                ht = TableFactory.CreateTable( bt );

                if( ht == null )
                    continue;

                HardwareList.Add( ht ); 


                //if( TypeBasedList.ContainsKey( ht.HardwareType ) == false )
                //    TypeBasedList.Add( ht.HardwareType, new List<IHardwareType>() );

                //TypeBasedList[ ht.HardwareType ].Add( ht );
                //HandleBasedList.Add( bt.Handle, ht );  

            }
        }


        public bool GetSMBIOS_Data()
        {
            if( HasSMBIOS == false && CheckSMBIOS() == false )
                return false;

            Cosmos.Kernel.MemoryAddressSpace Memory = new Cosmos.Kernel.MemoryAddressSpace( SMBBIOSAddress, 32 );
            
            SMBIOS_Data = new SMBIOS_Data();
            
            //4 Byte array
            SMBIOS_Data.AnchorString[ 0 ] = Memory.Read8Unchecked( 0 );
            SMBIOS_Data.AnchorString[ 1 ] = Memory.Read8Unchecked( 1 );
            SMBIOS_Data.AnchorString[ 2 ] = Memory.Read8Unchecked( 2 );
            SMBIOS_Data.AnchorString[ 3 ] = Memory.Read8Unchecked( 3 );

            SMBIOS_Data.EntryPointStructureChecksum = Memory.Read8Unchecked( 4 );
            SMBIOS_Data.EntryPointLength = Memory.Read8Unchecked( 5 );
            SMBIOS_Data.SMBIOSMajorVersion = Memory.Read8Unchecked( 6 );
            SMBIOS_Data.SMBIOSMinorVersion = Memory.Read8Unchecked( 7 );
            SMBIOS_Data.MaximumStructureSize = Memory.Read16Unchecked( 8 );

            SMBIOS_Data.EntryPointRevision = Memory.Read8Unchecked( 10 );

            //5 Byte array
            SMBIOS_Data.FormattedArea[ 0 ] = Memory.Read8Unchecked( 11 );
            SMBIOS_Data.FormattedArea[ 1 ] = Memory.Read8Unchecked( 12 );
            SMBIOS_Data.FormattedArea[ 2 ] = Memory.Read8Unchecked( 13 );
            SMBIOS_Data.FormattedArea[ 3 ] = Memory.Read8Unchecked( 14 );
            SMBIOS_Data.FormattedArea[ 4 ] = Memory.Read8Unchecked( 15 );

            //5 Byte array
            SMBIOS_Data.IntermediateAnchorString[ 0 ] = Memory.Read8Unchecked( 16 );
            SMBIOS_Data.IntermediateAnchorString[ 1 ] = Memory.Read8Unchecked( 17 );
            SMBIOS_Data.IntermediateAnchorString[ 2 ] = Memory.Read8Unchecked( 18 );
            SMBIOS_Data.IntermediateAnchorString[ 3 ] = Memory.Read8Unchecked( 19 );
            SMBIOS_Data.IntermediateAnchorString[ 4 ] = Memory.Read8Unchecked( 20 );

            SMBIOS_Data.IntermediateChecksum = Memory.Read8Unchecked( 21 );
            SMBIOS_Data.StructureTableLength = Memory.Read16Unchecked( 22 );
            SMBIOS_Data.StructureTableAddress = Memory.Read32Unchecked( 24 );
            SMBIOS_Data.NumberOfSMBIOSStructures = Memory.Read16Unchecked( 28 );
            SMBIOS_Data.SMBIOSBCDRevision = Memory.Read8Unchecked( 30 );

            return true;
        }
    }
}
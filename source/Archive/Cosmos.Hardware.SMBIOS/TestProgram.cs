using System;
using System.Collections.Generic;
using System.Text;

using Cosmos.Hardware.SMBIOS.Table;

namespace Cosmos.Hardware.SMBIOS
{
    public class TestProgram
    {
        public static void DumpBytes( uint from, uint count, uint column )
        {
            Cosmos.Kernel.MemoryAddressSpace Memory = new Cosmos.Kernel.MemoryAddressSpace( 0, 0 );
            
            uint j = 0;

            for( uint i = 0; i < count; i++ )
            {
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( Memory.Read8Unchecked( from + i ), 2 ) );
                Console.Write( " " );
                j++;
                if( j == column )
                {
                    Console.WriteLine( "" );
                    j = 0;
                }
            }
        }


        public static void Init()
        {
            // prevent interrupts from being enabled for now. 
            bool xTest = true;
            if (xTest)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }

            SMBIOS a = new SMBIOS();

            if( a.CheckSMBIOS() )
            {
                a.GetSMBIOS_Data();
 
                Console.Write( " SMBios found at: 0x" );
                //Console.Write(Cosmos.Kernel.HexExtension.ToHex(a.SMBBIOSAddress, 8));
                Console.Write( " Version: " );
                Console.Write(a.SMBIOS_Data.SMBIOSMajorVersion);
                Console.Write( "." );
                Console.WriteLine(a.SMBIOS_Data.SMBIOSMinorVersion);

                Console.Write( " Signature: " );
                //Console.WriteLine( Cosmos.Kernel.HexExtension.ToHex( a.Signature ) );

                Console.WriteLine( " SMBIOS dump: " );

                DumpBytes( a.SMBBIOSAddress, 32, 16 );
                Console.WriteLine( "" );

                //Console.Write( " SMBIOS anchor:  " );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.AnchorString[ 0 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.AnchorString[ 1 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.AnchorString[ 2 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.AnchorString[ 3 ] ) );

                //Console.Write( " DMI anchor: " );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.IntermediateAnchorString[ 0 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.IntermediateAnchorString[ 1 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.IntermediateAnchorString[ 2 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.IntermediateAnchorString[ 3 ] ) );
                //Console.Write( Cosmos.Kernel.HexExtension.ToHex( a.SMBIOS_Data.IntermediateAnchorString[ 4 ] ) );

                Console.Write( " Number of tables:" );
                Console.Write( a.SMBIOS_Data.NumberOfSMBIOSStructures );

                Console.WriteLine( "" );
                Console.Write( a.SMBIOS_Data.StructureTableLength );
                Console.WriteLine( " bytes of data" );
                
                Console.Write( " Reading " );
                Console.Write(" tables... " );
                int read = a.ReadTables();
                Console.Write( read );
                Console.WriteLine( " tables read." );

                Console.WriteLine( " Found Hardware Types:" ); 
                for( int i = 0; i < a.TableList.Count; i++ )
                {
                    Console.Write( a.TableList[ i ].TableType );
                    Console.Write( " " );
                }
                Console.WriteLine( "" ); 

                Console.Write( " Interpreting data... " );
                a.InterpretData(); 
                Console.WriteLine( " done." );


                List<BaseInfo> lst = a.GetHardwareDescriptorByType( TableTypes.BIOSInformation );
                BIOSTable biosinfo;
                if( lst.Count > 0 )
                {
                    BaseInfo ht = lst[ 0 ];
                    biosinfo = ( BIOSTable )ht;
                    Console.WriteLine( " BIOS Information: " );
                    Console.Write( "   Vendor: " );
                    Console.WriteLine( biosinfo.Vendor );  
                }


                ProcessorInformation CPUInfo;
                lst = a.GetHardwareDescriptorByType( TableTypes.ProcessorInformation );

                if( lst.Count > 0 )
                {
                    CPUInfo = ( ProcessorInformation )lst[ 0 ];
                    if( CPUInfo == null )
                        Console.WriteLine( " No CPU Information." );
                    else
                    {
                        Console.WriteLine( " CPU Information: " );
                        Console.Write( "   CurrentSpeed: " );
                        Console.Write( CPUInfo.CurrentSpeed );
                        Console.Write( " MHz  Core Count: " );
                        Console.WriteLine( CPUInfo.CoreCount );
                        Console.WriteLine( CPUInfo.ProcessorManufacturer );
                    }
                }


                   
            }
            Console.WriteLine( "Press a key to shutdown..." );
            Console.Read();

              
            Console.WriteLine("Done");
            Cosmos.Sys.Deboot.ShutDown();
        }
    }
}

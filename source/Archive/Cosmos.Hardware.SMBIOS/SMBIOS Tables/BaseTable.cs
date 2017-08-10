/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/
using System;
using System.Collections.Generic;

using Cosmos.Kernel;

namespace Cosmos.Hardware.SMBIOS
{
    public class BaseTable
    {
        private uint BaseAddress;
        public uint EndAddress;

        public ushort Handle;

        public byte TableType;
        public byte TableLength;

        public List<string> Strings;

        private uint NextAddress;

        private Cosmos.Kernel.MemoryAddressSpace Memory;

        public Cosmos.Kernel.MemoryAddressSpace Data;

        public byte SMBIOSMinorVersion;

        public BaseTable( uint BaseAddress, byte SMBIOSMinorVersion )
        {
            this.BaseAddress = BaseAddress;
            this.SMBIOSMinorVersion = SMBIOSMinorVersion;
            Memory = new MemoryAddressSpace( 0, 0 ); 
            NextAddress = BaseAddress; 
        }

        public bool ReadData()
        {

            TableType = GetNextByte();
            TableLength = GetNextByte();
            Handle = GetNextWord();

            NextAddress = BaseAddress;
            Data = new MemoryAddressSpace( BaseAddress, TableLength ); 

            GetStrings( BaseAddress + TableLength );

            EndAddress = NextAddress;

            return true;
        }

        private byte GetNextByte()
        {
            byte tmp = Memory.Read8Unchecked( NextAddress );
            NextAddress += 1; 
            return tmp;
        }

        private ushort GetNextWord()
        {
            ushort tmp = Memory.Read16Unchecked( NextAddress );
            NextAddress += 2; 
            return tmp;
        }

        private uint GetNextDWord()
        {
            uint tmp = Memory.Read32Unchecked( NextAddress );
            NextAddress += 4;
            return tmp;
        }

        private ulong GetNextQWord()
        {
            ulong tmp = Memory.Read64Unchecked( NextAddress );
            NextAddress += 8;
            return tmp;
        }

        public string GetString( int number )
        {
            if( number == 0 )
                return "";

            if( number - 1 > Strings.Count )
            {
                //Console.Write( " BaseTable: Tried to get string not in table #" );
                //Console.WriteLine( number );
            }
            else
            {
                //Console.Write( " BaseTable: Getting string #" );
                //Console.WriteLine( number );
                return Strings[ number - 1 ]; 
            }

            return "";

        }
        private void GetStrings( uint offset )
        {
            NextAddress = offset;
            byte chr = GetNextByte();
            byte counter = 0;
            string str = "";
            
            if( Strings == null )
                Strings = new List<string>();

            do
            {
                while( chr > 0 )
                {
                    str = str + ( char )chr;
                    counter++;
                    if( counter > 63 )
                        break;
                    chr = GetNextByte();
                }
                Strings.Add( str );
                
                str = "";
                counter = 0;
                chr = GetNextByte();
            } while( chr > 0 ); 
        }
    }
}
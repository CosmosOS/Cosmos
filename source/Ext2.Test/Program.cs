using System;
using System.IO;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;
using Cosmos.Sys;

namespace Ext2.Test
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main ( string [] args )
        {
            BuildUI.Run ();
        }
        #endregion

        // Main entry point of the kernel
        public static void Init ()
        {
            var xboot = new Cosmos.Sys.Boot ( ) ;
            xboot.Execute ( ) ;

            Console.WriteLine ( "Welcome! This project is for ext2 filesystem testing ..." ) ;
            Console.WriteLine ();
            Console.WriteLine ();

            Console.Write ( "The system has detected " ) ;
            Console.Write ( Device.Devices.Count );
            Console.WriteLine ( " device(s)." );
            Console.WriteLine ();

            for ( int i = 0 ; i < Device.Devices.Count ; i++ )
            {
                Console.Write ( "Device " ) ;
                Console.Write ( i );
                Console.Write ( " has been detected as " );
                Console.Write ( Device.Devices[i].Type.ToString() );
                Console.WriteLine( "." );
            }

            Console.WriteLine ();
            Console.WriteLine ();

            Console.Write ( "The are " );
            Console.Write ( VFSManager.Filesystems.Count );
            Console.WriteLine ( " filesystem(s) on this system." );
            Console.WriteLine ();

            for ( int i = 0 ; i < VFSManager.Filesystems.Count ; i++ )
            {
                Console.Write ( "Filesystem " );
                Console.Write ( i );
                Console.Write ( " has a block size of " );
                Console.Write ( VFSManager.Filesystems[i].BlockSize.ToString() );
                Console.WriteLine ( "." );
            }

            //string [] tmp = Directory.GetFiles ( "/0/" );
            string [] tmp = Directory.GetFileSystemEntries( "/1/" );

            Console.WriteLine ( tmp.Length.ToString () );

            for ( int i = 0 ; i < tmp.Length ; i++ )
            {
                Console.WriteLine ( tmp [ i ] );
            }

            while ( true )
                ;
        }
    }
}
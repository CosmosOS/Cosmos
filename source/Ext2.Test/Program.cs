using System;
using Cosmos.Compiler.Builder;
using Cosmos.Debug;

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
            xboot.Execute (  );

            Console.WriteLine ( "Welcome! You just booted C# code. Please edit Program.cs to fit your needs" );
            while ( true )
                ;
        }
    }
}
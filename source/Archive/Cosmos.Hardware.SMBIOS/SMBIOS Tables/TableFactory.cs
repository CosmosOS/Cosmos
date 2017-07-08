/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/

using Cosmos.Hardware.SMBIOS.Table; 

namespace Cosmos.Hardware.SMBIOS
{
    public static class TableFactory
    {

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="bt">The bt.</param>
        /// <returns></returns>
        public static BaseInfo CreateTable( BaseTable bt )
        {
            switch( (TableTypes)(int)bt.TableType )
            {

                //BIOS Information (Type 0) 
                case TableTypes.BIOSInformation:
                    return new BIOSTable( bt ); 
                    
                //Processor Information (Type 4) 
                case TableTypes.ProcessorInformation:
                    return new ProcessorInformation( bt ); 

                default:
                    return null;
            }
        }
    }
}
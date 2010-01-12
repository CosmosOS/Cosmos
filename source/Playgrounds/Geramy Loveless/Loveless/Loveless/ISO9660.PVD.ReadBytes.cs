using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.FileSystem.ISO9660
{
    /// <summary>
    /// Introduction to ISO 9660
    ///• Access to ISO 9660 volumes performed by system extensions.
    ///• Layout of folders and files on desktop is created when an ISO 9660 directory is
    ///opened.
    ///• ISO 9660 disc must include Apple extensions to run Macintosh applications from
    ///the ISO volume.
    ///• ISO 9660 file identifiers include the version number.
    ///- UNIX
    ///• Most UNIX type systems support Level 2 Interchange. Volumes that must be
    ///usable under MS-DOS, as well as UNIX, must be restricted to Level 1
    ///Interchange.
    ///• Access to ISO 9660 volumes is usually incorporated into the operating system.
    ///• There is considerable variation between UNIX type implementations as to how the
    ///File Identifiers appear to the user.
    ///• Some systems have options to convert File Identifiers to lower case, and remove
    ///the File Version Number, so the same volume can appear different, even on the
    ///same machine.
    ///• Extensions to ISO 9660
    ///- Apple ISO 9660 provides the Macintosh system with additional data needed to
    ///launch applications from an ISO 9660 volume.
    ///- The Rock Ridge Proposals provide a more UNIX like environment for
    ///distributing data to a variety of UNIX like platforms.
    ///- Updatable ISO 9660 provides a simple way to add more data to a previously
    ///recorded CD-WO.
    ///- The Frankfurt Group Proposal, ECMA 168, provides a way to append data to a
    ///CD-WO, and provide a more UNIX like environment for distributing data to a
    ///variety of UNIX like platforms.
    ///May 22, 1995 Page 38
    ///Introduction to ISO 9660

    /// </summary>
    public partial class ISO9660
    {
        ASCIIEncoding ascii = new ASCIIEncoding();
        byte[] Calculated_Bytes;
        public string Volume_Descriptor_Type_1(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 0; i < 1; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Standard_Identifier_1To6(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 2; i < 6; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Descriptor_Version(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            Calculated_Bytes[7] += Byte[7];
            return ascii.GetString(Calculated_Bytes);
        }
        public string Unused_Field(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            Calculated_Bytes[8] += Byte[8];
            return ascii.GetString(Calculated_Bytes);
        }
        public string System_Identifier_a_characters(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 9; i < 40; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string System_Identifier_d_characters(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 41; i < 72; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Space_Size(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 81; i < 82; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Set_Size_The_assigned(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 121; i < 124; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Sequence_Number(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 125; i < 128; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Logical_Block_Size(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 129; i < 132; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Path_Table_Size_Length(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 133; i < 140; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Type_L_Path_Table_Logical_Block_Number(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 141; i < 144; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Location_of_Optional_Type_L_Path_Table(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 145; i < 148; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Type_M_Path_Table_Logical_Block_Number(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 149; i < 152; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Optional_Type_M_Path_Table(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 153; i < 156; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Directory_record_for_Root_Directory(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 157; i < 190; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Set_Identifier_Name(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 191; i < 318; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Publisher_Identifier(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 319; i < 446; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Data_Preparer_Identifier(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 319; i < 446; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Application_Identifier(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 575; i < 702; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Copyright_File_Identifier(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 703; i < 739; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Abstract_File_Identifier(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 740; i < 776; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Bibliographic_File_Identifier(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 777; i < 813; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        /// <summary>
        /// Represented by seven bytes:
        ///1: Number of years since 1900
        ///2: Month of the year from 1 to 12
        ///3: Day of the Month from 1 to 31
        ///4: Hour of the day from 0 to 23
         ///5: Minute of the hour from 0 to 59
         ///6: second of the minute from 0 to 59
         ///7: Offset from Greenwich Mean Time in
         ///number of 15 minute intervals from
        ///-48(West) to +52(East)
        /// </summary>
        /// <param name="Byte"></param>
        /// <returns></returns>
        public string Volume_Creation_Date_and_Time(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 814; i < 830; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Modification_Date_and_Time(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 831; i < 847; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Expiration_Date_and_Time_Date(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 848; i < 864; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string Volume_Effective_Date_and_Time(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 865; i < 881; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        public string File_Structure_Version_1(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            Calculated_Bytes[882] += Byte[882];
            return ascii.GetString(Calculated_Bytes);
        }
        public string Reserved_for_future_standardization_1(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            Calculated_Bytes[883] += Byte[883];
            return ascii.GetString(Calculated_Bytes);
        }
        /// <summary>
        /// Application Use This field is reserved for application use. Its content is not
        /// specified by ISO-9660.
        /// </summary>
        /// <param name="Byte"></param>
        /// <returns></returns>
        public string Application_Use(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 884; i < 1395; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
        /// <summary>
        /// must be set to (00).
        /// </summary>
        /// <param name="Byte"></param>
        /// <returns></returns>
        public string Reserved_for_future_standardization_2(byte[] Byte)
        {
            Calculated_Bytes = new byte[0];
            for (int i = 1396; i < 2048; i++)
            {
                Calculated_Bytes[i] += Byte[i];
            }
            return ascii.GetString(Calculated_Bytes);
        }
    }
}
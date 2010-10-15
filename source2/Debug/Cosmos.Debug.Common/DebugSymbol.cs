using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Cosmos.Debug.Common
{
    public delegate void LogTimeDelegate(string message);

    public class DebugSymbol
    {
        public string AssemblyFileName
        {
            get;
            set;
        }

        public int MethodMetaDataToken
        {
            get;
            set;
        }

        public int InstructionOffset
        {
            get;
            set;
        }

        public string LabelName
        {
            get;
            set;
        }
    }

    public class MLDebugSymbol
    {
        private static FbConnection OpenCPDB(string aPathname, bool aCreate)
        {
            var xCSB = new FbConnectionStringBuilder();
            xCSB.ServerType = FbServerType.Embedded;
            xCSB.Database = aPathname;
            xCSB.UserID = "sysdba";
            xCSB.Password = "masterkey";
            xCSB.Pooling = false;

            // Ugh - The curr dir is the actual .cosmos dir. But we dont want to
            // copy the FB Embedded DLLs everywhere, and we don't want them in system
            // or path as they might conflict with other apps.
            // However the FB .NET provider doesnt let us set the path, so we hack it
            // by changing the current dir right before the first load (create or open).
            // We set it back after.
            string xCurrDir = Directory.GetCurrentDirectory();
            using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Cosmos", false))
            {
                string xCosmosDir = (string)xKey.GetValue("");
                Directory.SetCurrentDirectory(Path.Combine(xCosmosDir, @"Build\VSIP"));
            }

            if (aCreate)
            {
                File.Delete(aPathname);
                FbConnection.CreateDatabase(xCSB.ToString(), 16384, false, true); // Specifying false to forcedwrites will improve database speed.
            }

            FbConnection DBConn = new FbConnection(xCSB.ToString());
            DBConn.Open();

            // Set the current directory back to the original
            Directory.SetCurrentDirectory(xCurrDir);

            return DBConn;
        }

        private static FbConnection CreateCPDB(string aPathname)
        {
            FbConnection DBConn = OpenCPDB(aPathname, true);
            var xExec = new FbBatchExecution(DBConn);

            xExec.SqlStatements.Add(
                "CREATE TABLE SYMBOL ("
                + "   LABELNAME   VARCHAR(255)  NOT NULL"
                + " , ADDRESS     BIGINT        NOT NULL"
                + " , STACKDIFF   INT           NOT NULL"
                + " , ILASMFILE   VARCHAR(255)  NOT NULL"
                + " , TYPETOKEN   INT           NOT NULL"
                + " , METHODTOKEN INT           NOT NULL"
                + " , ILOFFSET    INT           NOT NULL"
                + " , METHODNAME  VARCHAR(255)  NOT NULL"
                + ");"
            );

            xExec.Execute();
            // Batch execution closes the connection, so we have to reopen it
            DBConn.Open();

            return DBConn;
        }

        public static void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols, string aFile)
        {
            // Use a temporary file for the database and then move the newly created database to the wanted location afterwards.
            // We do this so the user can gain compilation speed when using a faster disk for instance a mem-disk for the temporary files.
            string tmpdbname = Path.GetTempFileName();
            string dbname = Path.ChangeExtension(aFile, ".cpdb");

            using (FbConnection DBConn = CreateCPDB(tmpdbname))
            {
                var xDS = new SymbolsDS();

                using (FbTransaction transaction = DBConn.BeginTransaction())
                {
                    string sqlstmt = "INSERT INTO SYMBOL (LABELNAME, ADDRESS, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME)" +
                                     " VALUES (@LABELNAME, @ADDRESS, @STACKDIFF, @ILASMFILE, @TYPETOKEN, @METHODTOKEN, @ILOFFSET, @METHODNAME)";

                    // Is a real DB now, but we still store all in RAM. We dont need to. Need to change to query DB as needed instead.
                    foreach (var xItem in aSymbols)
                    {
                        var x = xDS.Entry.NewEntryRow();
                        x.LabelName = xItem.LabelName;
                        x.Address = xItem.Address;
                        x.StackDiff = xItem.StackDifference;
                        x.ILAsmFile = xItem.AssemblyFile;
                        x.TypeToken = xItem.TypeToken;
                        x.MethodToken = xItem.MethodToken;
                        x.ILOffset = xItem.ILOffset;
                        x.MethodName = xItem.MethodName;
                        xDS.Entry.AddEntryRow(x);

                        var xCmd = new FbCommand(sqlstmt, DBConn, transaction);
                        xCmd.Parameters.Add("@LABELNAME", xItem.LabelName);
                        xCmd.Parameters.Add("@ADDRESS", xItem.Address);
                        xCmd.Parameters.Add("@STACKDIFF", xItem.StackDifference);
                        xCmd.Parameters.Add("@ILASMFILE", xItem.AssemblyFile);
                        xCmd.Parameters.Add("@TYPETOKEN", xItem.TypeToken);
                        xCmd.Parameters.Add("@METHODTOKEN", xItem.MethodToken);
                        xCmd.Parameters.Add("@ILOFFSET", xItem.ILOffset);
                        xCmd.Parameters.Add("@METHODNAME", xItem.MethodName);
                        xCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                xDS.WriteXml(aFile);
            }

            if (File.Exists(dbname))
                File.Delete(dbname);

            File.Move(tmpdbname, dbname);
        }

        public static void ReadSymbolsListFromFile(List<MLDebugSymbol> aSymbols, string aFile)
        {
            //OpenCPDB(Path.ChangeExtension(aFile, ".cpdb"), false);
            var xDS = new SymbolsDS();
            xDS.ReadXml(aFile);
            foreach (SymbolsDS.EntryRow x in xDS.Entry.Rows)
            {
                aSymbols.Add(new MLDebugSymbol
                {
                    LabelName = x.LabelName,
                    Address = x.Address,
                    StackDifference = x.StackDiff,
                    AssemblyFile = x.ILAsmFile,
                    TypeToken = x.TypeToken,
                    MethodToken = x.MethodToken,
                    ILOffset = x.ILOffset,
                    MethodName = x.MethodName
                });
            }
        }

        public string LabelName
        {
            get;
            set;
        }

        public uint Address
        {
            get;
            set;
        }

        public int StackDifference
        {
            get;
            set;
        }

        public string AssemblyFile
        {
            get;
            set;
        }
        public int TypeToken
        {
            get;
            set;
        }
        public int MethodToken
        {
            get;
            set;
        }
        public int ILOffset
        {
            get;
            set;
        }

        public string MethodName
        {
            get;
            set;
        }
    }
}

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
        public static FbConnection OpenOrCreateCPDB(string aPathName)
        {
            if (File.Exists(aPathName))
            {
                return OpenCPDB(aPathName, false);
            }
            else
            {
                return CreateCPDB(aPathName);
            }
        }

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
                "CREATE TABLE MLSYMBOL ("
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
            
            xExec.SqlStatements.Add(
                "CREATE TABLE ADDRESSLABELMAPPING ("
                + "  LABELNAME VARCHAR(255) NOT NULL"
                + ", ADDRESS   BIGINT NOT NULL"
                + ");");

            xExec.Execute();
            // Batch execution closes the connection, so we have to reopen it
            DBConn.Open();

            return DBConn;
        }

        public static void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols, string aFile)
        {
            using (FbConnection DBConn = OpenOrCreateCPDB(aFile))
            {
                using (FbTransaction transaction = DBConn.BeginTransaction())
                {
                    using (var xCmd = DBConn.CreateCommand())
                    {
                        xCmd.Transaction = transaction;
                        xCmd.CommandText = "INSERT INTO MLSYMBOL (LABELNAME, ADDRESS, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME)" +
                                     " VALUES (@LABELNAME, @ADDRESS, @STACKDIFF, @ILASMFILE, @TYPETOKEN, @METHODTOKEN, @ILOFFSET, @METHODNAME)";

                        xCmd.Parameters.Add("@LABELNAME", FbDbType.VarChar);
                        xCmd.Parameters.Add("@ADDRESS", FbDbType.BigInt);
                        xCmd.Parameters.Add("@STACKDIFF", FbDbType.Integer);
                        xCmd.Parameters.Add("@ILASMFILE", FbDbType.VarChar);
                        xCmd.Parameters.Add("@TYPETOKEN", FbDbType.Integer);
                        xCmd.Parameters.Add("@METHODTOKEN", FbDbType.Integer);
                        xCmd.Parameters.Add("@ILOFFSET", FbDbType.Integer);
                        xCmd.Parameters.Add("@METHODNAME", FbDbType.VarChar);
                        xCmd.Prepare();

                        // Is a real DB now, but we still store all in RAM. We dont need to. Need to change to query DB as needed instead.
                        foreach (var xItem in aSymbols)
                        {
                            xCmd.Parameters[0].Value = xItem.LabelName;
                            xCmd.Parameters[1].Value = xItem.Address;
                            xCmd.Parameters[2].Value = xItem.StackDifference;
                            xCmd.Parameters[3].Value = xItem.AssemblyFile;
                            xCmd.Parameters[4].Value = xItem.TypeToken;
                            xCmd.Parameters[5].Value = xItem.MethodToken;
                            xCmd.Parameters[6].Value = xItem.ILOffset;
                            xCmd.Parameters[7].Value = xItem.MethodName;
                            xCmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public static void ReadSymbolsListFromFile(List<MLDebugSymbol> aSymbols, string aFile)
        {
            using (var xConn = OpenCPDB(aFile, false))
            {
                using (var xCmd = xConn.CreateCommand())
                {
                    xCmd.CommandText = "select LABELNAME, ADDRESS, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME from MLSYMBOL";
                    using (var xReader = xCmd.ExecuteReader())
                    {
                        while (xReader.Read())
                        {
                            aSymbols.Add(new MLDebugSymbol
                            {
                                LabelName=xReader.GetString(0),
                                Address = (uint)xReader.GetInt64(1),
                                StackDifference = xReader.GetInt32(2),
                                AssemblyFile = xReader.GetString(3),
                                TypeToken = xReader.GetInt32(4),
                                MethodToken = xReader.GetInt32(5),
                                ILOffset = xReader.GetInt32(6),
                                MethodName = xReader.GetString(7)
                            });
                        }
                    }
                }
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

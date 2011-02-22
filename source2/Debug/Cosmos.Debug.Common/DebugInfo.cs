using System;
using System.Collections.Generic;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using Microsoft.Win32;
using FirebirdSql.Data.Isql;

namespace Cosmos.Debug.Common
{
    public class DebugInfo: IDisposable
    {
        public class MLDebugSymbol
        {
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
        public class Local_Argument_Info{
            public bool IsArgument{
                get;
                set;
            }

            public string MethodLabelName{
                get;
                set;
            }

            public int Index{
                get;
                set;
            }

            public int Offset{
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }
        }
        private FbConnection mConnection;

        public DebugInfo(string file)
        {
            var xCreate = !File.Exists(file);
            if (xCreate)
            {
                mConnection = CreateCPDB(file);
            }
            else
            {
                mConnection = OpenCPDB(file, false);
            }
        }
        #region connection utilities
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

            xExec.SqlStatements.Add(
                "CREATE TABLE LOCAL_ARGUMENT_INFO ("
                + "  METHODLABELNAME VARCHAR(255) NOT NULL"
                + ", ISARGUMENT      SMALLINT          NOT NULL"
                + ", INDEXINMETHOD   INT          NOT NULL"
                + ", OFFSET          INT          NOT NULL"
                + ", NAME            VARCHAR(255) NOT NULL"
                + ", TYPENAME        VARCHAR(255) NOT NULL"
                + ");"
                );

            xExec.Execute();
            // Batch execution closes the connection, so we have to reopen it
            DBConn.Open();

            return DBConn;
        }
        #endregion

        #region MLDebugSymbol code
        public void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols)
        {
            using (FbTransaction transaction = mConnection.BeginTransaction())
            {
                using (var xCmd = mConnection.CreateCommand())
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
        public void ReadSymbolsList(List<MLDebugSymbol> aSymbols)
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select LABELNAME, ADDRESS, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME from MLSYMBOL";
                using (var xReader = xCmd.ExecuteReader())
                {
                    while (xReader.Read())
                    {
                        aSymbols.Add(new MLDebugSymbol
                        {
                            LabelName = xReader.GetString(0),
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

        public MLDebugSymbol ReadSymbolByLabelName(string labelName)
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select LABELNAME, ADDRESS, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME from MLSYMBOL "
                    + "WHERE LABELNAME = @LABELNAME";
                xCmd.Parameters.Add("@LABELNAME", labelName);
                using (var xReader = xCmd.ExecuteReader())
                {
                    if (xReader.Read())
                    {
                        return new MLDebugSymbol
                        {
                            LabelName = xReader.GetString(0),
                            Address = (uint)xReader.GetInt64(1),
                            StackDifference = xReader.GetInt32(2),
                            AssemblyFile = xReader.GetString(3),
                            TypeToken = xReader.GetInt32(4),
                            MethodToken = xReader.GetInt32(5),
                            ILOffset = xReader.GetInt32(6),
                            MethodName = xReader.GetString(7)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        #endregion

        #region Param/Local locations

        // tuple format: MethodLabel, IsArgument, Index, Offset
        public void WriteAllLocalsArgumentsInfos(IEnumerable<Local_Argument_Info> infos)
        {
            using (var xTrans = mConnection.BeginTransaction())
            {
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = xTrans;
                    xCmd.CommandText = "insert into LOCAL_ARGUMENT_INFO (METHODLABELNAME, ISARGUMENT, INDEXINMETHOD, OFFSET, NAME, TYPENAME) values (@METHODLABELNAME, @ISARGUMENT, @INDEXINMETHOD, @OFFSET, @NAME, @TYPENAME)";
                    xCmd.Parameters.Add("@METHODLABELNAME", FbDbType.VarChar);
                    xCmd.Parameters.Add("@ISARGUMENT", FbDbType.SmallInt);
                    xCmd.Parameters.Add("@INDEXINMETHOD", FbDbType.Integer);
                    xCmd.Parameters.Add("@OFFSET", FbDbType.Integer);
                    xCmd.Parameters.Add("@NAME", FbDbType.VarChar);
                    xCmd.Parameters.Add("@TYPENAME", FbDbType.VarChar);
                    xCmd.Prepare();
                    foreach (var xInfo in infos)
                    {
                        xCmd.Parameters[0].Value = xInfo.MethodLabelName;
                        xCmd.Parameters[1].Value = xInfo.IsArgument ? 1 : 0;
                        xCmd.Parameters[2].Value = xInfo.Index;
                        xCmd.Parameters[3].Value = xInfo.Offset;
                        xCmd.Parameters[4].Value = xInfo.Name;
                        xCmd.Parameters[5].Value = xInfo.Type;
                        xCmd.ExecuteNonQuery();
                    }
                    xTrans.Commit();
                }
            }
        }

        public IList<Local_Argument_Info> ReadAllLocalsArgumentsInfos()
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select METHODLABELNAME, ISARGUMENT, INDEXINMETHOD, OFFSET, NAME, TYPENAME from LOCAL_ARGUMENT_INFO";
                using (var xReader = xCmd.ExecuteReader())
                {
                    var xResult = new List<Local_Argument_Info>(xReader.RecordsAffected);
                    while (xReader.Read())
                    {
                        xResult.Add(new Local_Argument_Info
                        {
                            MethodLabelName = xReader.GetString(0),
                            IsArgument = xReader.GetInt16(1) == 1,
                            Index = xReader.GetInt32(2),
                            Offset = xReader.GetInt32(3),
                            Name=xReader.GetString(4),
                            Type = xReader.GetString(5)
                        });
                    }
                    return xResult;
                }
            }
        }

        public IList<Local_Argument_Info> ReadAllLocalsArgumentsInfosByMethodLabelName(string methodLabelName)
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select METHODLABELNAME, ISARGUMENT, INDEXINMETHOD, OFFSET, NAME, TYPENAME from LOCAL_ARGUMENT_INFO" 
                    + " WHERE METHODLABELNAME = @METHODLABELNAME";
                xCmd.Parameters.Add("@METHODLABELNAME", methodLabelName);
                using (var xReader = xCmd.ExecuteReader())
                {
                    var xResult = new List<Local_Argument_Info>(xReader.RecordsAffected);
                    while (xReader.Read())
                    {
                        xResult.Add(new Local_Argument_Info
                        {
                            MethodLabelName = xReader.GetString(0),
                            IsArgument = xReader.GetInt16(1) == 1,
                            Index = xReader.GetInt32(2),
                            Offset = xReader.GetInt32(3),
                            Name = xReader.GetString(4),
                            Type=xReader.GetString(5)
                        });
                    }
                    return xResult;
                }
            }
        }

        #endregion

        #region address-label mappings
        public void ReadAddressLabelMappings(out IDictionary<uint, string> oAddressLabelMappings, out IDictionary<string, uint> oLabelAddressMappings)
        {
            oAddressLabelMappings = new Dictionary<uint, string>();
            oLabelAddressMappings = new Dictionary<string, uint>();
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select LABELNAME, ADDRESS from ADDRESSLABELMAPPING";
                using (var xReader = xCmd.ExecuteReader())
                {
                    while (xReader.Read())
                    {
                        oAddressLabelMappings.Add((uint)xReader.GetInt64(1), xReader.GetString(0));
                        oLabelAddressMappings.Add(xReader.GetString(0), (uint)xReader.GetInt64(1));
                    }
                }
            }
        }

        public void WriteAddressLabelMappings(SortedList<uint, String> aMap)
        {
            using (var xTrans = mConnection.BeginTransaction())
            {
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = xTrans;
                    xCmd.CommandText = "insert into ADDRESSLABELMAPPING (LABELNAME, ADDRESS) values (@LABELNAME, @ADDRESS)";
                    xCmd.Parameters.Add("@LABELNAME", FbDbType.VarChar);
                    xCmd.Parameters.Add("@ADDRESS", FbDbType.BigInt);
                    xCmd.Prepare();
                    foreach (var xItem in aMap)
                    {
                        xCmd.Parameters[0].Value = xItem.Value;
                        xCmd.Parameters[1].Value = xItem.Key;
                        xCmd.ExecuteNonQuery();
                    }
                    xTrans.Commit();
                }
            }
        } 
        #endregion

        public void Dispose()
        {
            if (mConnection != null)
            {
                var xCon = mConnection;
                mConnection = null;
                xCon.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
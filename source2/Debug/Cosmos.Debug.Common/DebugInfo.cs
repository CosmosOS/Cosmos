using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

namespace Cosmos.Debug.Common
{
    public class DebugInfo : IDisposable
    {
        /// <summary>
        /// Please beware this field,
        /// it may cause issues if used incorrectly.
        /// </summary>
        public static DebugInfo CurrentInstance { get; private set; }
        public class Field_Info
        {
            public string Type { get; set; }
            public int Offset { get; set; }
            public string Name { get; set; }
        }

        public class Field_Map
        {
            public string TypeName { get; set; }
            public List<string> FieldNames = new List<string>();
        }

        public class MLDebugSymbol
        {
            public string LabelName { get; set; }
            public int StackDifference { get; set; }
            public string AssemblyFile { get; set; }
            public int TypeToken { get; set; }
            public int MethodToken { get; set; }
            public int ILOffset { get; set; }
            public string MethodName { get; set; }
        }

        public class Local_Argument_Info
        {
            public bool IsArgument { get; set; }
            public bool IsArrayElement { get; set; }
            public string MethodLabelName { get; set; }
            public int Index { get; set; }
            public int Offset { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        private FbConnection mConnection;

        public DebugInfo()
        {
            CurrentInstance = this;
        }

        public void OpenCPDB(string aPathname)
        {
            OpenCPDB(aPathname, false);
        }

        private void OpenCPDB(string aPathname, bool aCreate)
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

            mConnection = new FbConnection(xCSB.ToString());
            mConnection.Open();

            // Set the current directory back to the original
            Directory.SetCurrentDirectory(xCurrDir);
        }

        public void CreateCPDB(string aPathname)
        {
            OpenCPDB(aPathname, true);
            var xExec = new FbBatchExecution(mConnection);

            xExec.SqlStatements.Add(
                "CREATE TABLE Method ("
                + "    MethodId      INT            NOT NULL PRIMARY KEY"
                + "  , LabelPrefix   VARCHAR(255)   NOT NULL"
                + ");");

            xExec.SqlStatements.Add(
                "CREATE TABLE MLSYMBOL ("
                + "   LABELNAME   VARCHAR(255)  NOT NULL"
                + " , STACKDIFF   INT           NOT NULL"
                + " , ILASMFILE   VARCHAR(255)  NOT NULL"
                + " , TYPETOKEN   INT           NOT NULL"
                + " , METHODTOKEN INT           NOT NULL"
                + " , ILOFFSET    INT           NOT NULL"
                + " , METHODNAME  VARCHAR(255)  NOT NULL"
                + ");"
            );

            xExec.SqlStatements.Add(
                "CREATE TABLE FIELD_INFO ("
                + "    TYPE      VARCHAR(4000)     NOT NULL"
                + " ,  OFFSET    INT               NOT NULL"
                + " ,  NAME      VARCHAR(4000)     NOT NULL PRIMARY KEY"
                + ");"
                );

            xExec.SqlStatements.Add(
                "CREATE TABLE FIELD_MAPPING ("
                + "    TYPE_NAME        VARCHAR(4000)            NOT NULL PRIMARY KEY"
                + " ,  FIELD_COUNT      INT                      NOT NULL"
                + " ,  FIELD_NAMES      VARCHAR(4000)[0:255]     NOT NULL"
                + ");"
                );

            xExec.SqlStatements.Add(
                "CREATE TABLE Label ("
                + "  LABELNAME VARCHAR(4000)  NOT NULL"
                + ", ADDRESS   BIGINT        NOT NULL"
                + ");");

            xExec.SqlStatements.Add(
                "CREATE TABLE LOCAL_ARGUMENT_INFO ("
                + "  METHODLABELNAME VARCHAR(255)      NOT NULL"
                + ", ISARGUMENT      SMALLINT          NOT NULL"
                + ", INDEXINMETHOD   INT               NOT NULL"
                + ", OFFSET          INT               NOT NULL"
                + ", NAME            VARCHAR(255)      NOT NULL"
                + ", TYPENAME        VARCHAR(4000)     NOT NULL"
                + ");"
                );

            xExec.Execute();
            // Batch execution closes the connection, so we have to reopen it
            mConnection.Open();
        }

        private List<string> local_MappingTypeNames = new List<string>();
        public void WriteFieldMappingToFile(IEnumerable<Field_Map> aMapping)
        {
            using (FbTransaction transaction = mConnection.BeginTransaction())
            {
                IEnumerable<Field_Map> xMaps = aMapping.Where(delegate(Field_Map mp)
                {
                    if (local_MappingTypeNames.Contains(mp.TypeName))
                    {
                        return false;
                    }
                    else
                    {
                        local_MappingTypeNames.Add(mp.TypeName);
                        return true;
                    }
                });
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = transaction;
                    xCmd.CommandText = "INSERT INTO FIELD_MAPPING (TYPE_NAME, FIELD_COUNT, FIELD_NAMES)" +
                                       " VALUES (@TYPE_NAME, @FIELD_COUNT, @FIELD_NAMES)";

                    xCmd.Parameters.Add("@TYPE_NAME", FbDbType.VarChar);
                    xCmd.Parameters.Add("@FIELD_COUNT", FbDbType.Integer);
                    xCmd.Parameters.Add("@FIELD_NAMES", FbDbType.Array);
                    xCmd.Prepare();

                    // Is a real DB now, but we still store all in RAM. We don't need to. Need to change to query DB as needed instead.
                    foreach (var xItem in xMaps)
                    {
                        xCmd.Parameters[0].Value = xItem.TypeName;
                        xCmd.Parameters[1].Value = xItem.FieldNames.Count;
                        if (xItem.FieldNames.Count > 255)
                        {
                            throw new Exception("To many fields! There are '" + xItem.FieldNames.Count + "' fields.");
                        }
                        xCmd.Parameters[2].Value = xItem.FieldNames.ToArray();
                        xCmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        public Field_Map GetFieldMap(string name)
        {
            Field_Map mp = new Field_Map();
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select TYPE_NAME, FIELD_COUNT, FIELD_NAMES from FIELD_MAPPING where(TYPE_NAME='" + name + "')";
                using (var xReader = xCmd.ExecuteReader())
                {
                    if (xReader.Read())
                    {
                        mp.TypeName = xReader.GetString(0);
                        int i = xReader.GetInt32(1);
                        mp.FieldNames.AddRange(((string[])xReader.GetValue(2)).Take(i));
                    }
                    else
                    {
                        mp.TypeName = "UNKNOWN!";
                    }
                }
            }
            return mp;
        }

        public void ReadFieldMappingList(List<Field_Map> aSymbols)
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select TYPE_NAME, FIELD_COUNT, FIELD_NAMES from FIELD_MAPPING";
                using (var xReader = xCmd.ExecuteReader())
                {
                    while (xReader.Read())
                    {
                        Field_Map mp = new Field_Map();
                        mp.TypeName = xReader.GetString(0);
                        int i = xReader.GetInt32(1);
                        mp.FieldNames.AddRange(((string[])xReader.GetValue(2)).Take(i));
                        aSymbols.Add(mp);
                    }
                }
            }
        }


        private List<string> local_FieldInfoNames = new List<string>();
        public void WriteFieldInfoToFile(IEnumerable<Field_Info> aFields)
        {
            using (FbTransaction transaction = mConnection.BeginTransaction())
            {
                IEnumerable<Field_Info> xFields = aFields.Where(delegate(Field_Info mp)
                {
                    if (local_FieldInfoNames.Contains(mp.Name))
                    {
                        return false;
                    }
                    else
                    {
                        local_FieldInfoNames.Add(mp.Name);
                        return true;
                    }
                });
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = transaction;
                    xCmd.CommandText = "INSERT INTO FIELD_INFO (TYPE, OFFSET, NAME)" +
                                       " VALUES (@TYPE, @OFFSET, @NAME)";

                    xCmd.Parameters.Add("@TYPE", FbDbType.VarChar);
                    xCmd.Parameters.Add("@OFFSET", FbDbType.Integer);
                    xCmd.Parameters.Add("@NAME", FbDbType.VarChar);
                    xCmd.Prepare();

                    // Is a real DB now, but we still store all in RAM. We don't need to. Need to change to query DB as needed instead.
                    foreach (var xItem in xFields)
                    {
                        xCmd.Parameters[0].Value = xItem.Type;
                        xCmd.Parameters[1].Value = xItem.Offset;
                        xCmd.Parameters[2].Value = xItem.Name;
                        xCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        public Field_Info GetFieldInfo(string name)
        {
            Field_Info inf = new Field_Info();
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select TYPE, OFFSET, NAME from FIELD_INFO where(NAME='" + name + "')";
                using (var xReader = xCmd.ExecuteReader())
                {
                    xReader.Read();
                    inf.Type = xReader.GetString(0);
                    inf.Offset = xReader.GetInt32(1);
                    inf.Name = xReader.GetString(2);
                }
            }
            return inf;
        }

        public void ReadFieldInfoList(List<Field_Info> aSymbols)
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select TYPE, OFFSET, NAME from FIELD_INFO";
                using (var xReader = xCmd.ExecuteReader())
                {
                    while (xReader.Read())
                    {
                        aSymbols.Add(new Field_Info
                        {
                            Type = xReader.GetString(0),
                            Offset = xReader.GetInt32(1),
                            Name = xReader.GetString(2),
                        });
                    }
                }
            }
        }


        public void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols)
        {
            using (FbTransaction transaction = mConnection.BeginTransaction())
            {
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = transaction;
                    xCmd.CommandText = "INSERT INTO MLSYMBOL (LABELNAME, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME)" +
                                 " VALUES (@LABELNAME, @STACKDIFF, @ILASMFILE, @TYPETOKEN, @METHODTOKEN, @ILOFFSET, @METHODNAME)";

                    xCmd.Parameters.Add("@LABELNAME", FbDbType.VarChar);
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
                        xCmd.Parameters[1].Value = xItem.StackDifference;
                        xCmd.Parameters[2].Value = xItem.AssemblyFile;
                        xCmd.Parameters[3].Value = xItem.TypeToken;
                        xCmd.Parameters[4].Value = xItem.MethodToken;
                        xCmd.Parameters[5].Value = xItem.ILOffset;
                        xCmd.Parameters[6].Value = xItem.MethodName;
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
                xCmd.CommandText = "select LABELNAME, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME from MLSYMBOL";
                using (var xReader = xCmd.ExecuteReader())
                {
                    while (xReader.Read())
                    {
                        aSymbols.Add(new MLDebugSymbol
                        {
                            LabelName = xReader.GetString(0),
                            StackDifference = xReader.GetInt32(1),
                            AssemblyFile = xReader.GetString(2),
                            TypeToken = xReader.GetInt32(3),
                            MethodToken = xReader.GetInt32(4),
                            ILOffset = xReader.GetInt32(5),
                            MethodName = xReader.GetString(6)
                        });
                    }
                }
            }
        }

        public MLDebugSymbol ReadSymbolByLabelName(string labelName)
        {
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select LABELNAME, STACKDIFF, ILASMFILE, TYPETOKEN, METHODTOKEN, ILOFFSET, METHODNAME from MLSYMBOL"
                    + " WHERE LABELNAME = @LABELNAME";
                xCmd.Parameters.Add("@LABELNAME", labelName);
                using (var xReader = xCmd.ExecuteReader())
                {
                    if (xReader.Read())
                    {
                        return new MLDebugSymbol
                        {
                            LabelName = xReader.GetString(0),
                            StackDifference = xReader.GetInt32(1),
                            AssemblyFile = xReader.GetString(2),
                            TypeToken = xReader.GetInt32(3),
                            MethodToken = xReader.GetInt32(4),
                            ILOffset = xReader.GetInt32(5),
                            MethodName = xReader.GetString(6)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

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
                            Name = xReader.GetString(4),
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
                            Type = xReader.GetString(5)
                        });
                    }
                    return xResult;
                }
            }
        }

        public void ReadLabels(out List<KeyValuePair<uint, string>> oLabels, out IDictionary<string, uint> oLabelAddressMappings)
        {
            oLabels = new List<KeyValuePair<uint, string>>();
            oLabelAddressMappings = new Dictionary<string, uint>();
            using (var xCmd = mConnection.CreateCommand())
            {
                xCmd.CommandText = "select LABELNAME, ADDRESS from Label";
                using (var xReader = xCmd.ExecuteReader())
                {
                    while (xReader.Read())
                    {
                        oLabels.Add(new KeyValuePair<uint, string>((uint)xReader.GetInt64(1), xReader.GetString(0)));
                        oLabelAddressMappings.Add(xReader.GetString(0), (uint)xReader.GetInt64(1));
                    }
                }
            }
        }

        // This is a heck of a lot easier than using sequences
        protected int mMethodId = 0;
        public int AddMethod(string aLabelPrefix)
        {
            mMethodId++;
            using (var xTrans = mConnection.BeginTransaction())
            {
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = xTrans;
                    xCmd.CommandText = "INSERT INTO Method (MethodId, LabelPrefix) values (@MethodId, @LabelPrefix)";

                    xCmd.Parameters.Add("@MethodId", FbDbType.Integer);
                    xCmd.Parameters.Add("@LabelPrefix", FbDbType.VarChar);
                    xCmd.Prepare();

                    xCmd.Parameters[0].Value = mMethodId;
                    xCmd.Parameters[1].Value = aLabelPrefix;
                    xCmd.ExecuteNonQuery();

                    xTrans.Commit();
                }
            }
            return mMethodId;
        }

        public void WriteLabels(List<KeyValuePair<uint, string>> aMap)
        {
            using (var xTrans = mConnection.BeginTransaction())
            {
                using (var xCmd = mConnection.CreateCommand())
                {
                    xCmd.Transaction = xTrans;
                    xCmd.CommandText = "insert into Label (LABELNAME, ADDRESS) values (@LABELNAME, @ADDRESS)";
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
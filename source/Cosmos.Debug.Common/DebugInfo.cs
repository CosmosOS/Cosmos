using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Microsoft.Samples.Debugging.CorSymbolStore;
using SQLinq;
using SQLinq.Dapper;

namespace Cosmos.Debug.Common
{
    public class DebugInfo : IDisposable
    {
        /// <summary>
        /// Current id of the generation.
        /// </summary>
        private static long mLastGuid = 0;

        /// <summary>
        /// Range for the id generation process.
        /// </summary>
        private static long mPrefix = 0;

        /// <summary>
        /// Specifies range which is used by the assembler during compilation phase.
        /// </summary>
        public const long AssemblerDebugSymbolsRange = 0x0L;

        /// <summary>
        /// Specifies range which is used by the Elf map extraction process.
        /// </summary>
        public const long ElfFileMapExtractionRange = 0x1000000000000000L;

        /// <summary>
        /// Specifies range which is used by the Nasm map extraction process.
        /// </summary>
        public const long NAsmMapExtractionRange = 0x4000000000000000L;

        // Please beware this field, it may cause issues if used incorrectly.
        public static DebugInfo CurrentInstance { get; private set; }

        public class Field_Map
        {
            public string TypeName { get; set; }
            public List<string> FieldNames = new List<string>();
        }

        protected SQLiteConnection mConnection;
        protected string mDbName;
        // Dont use DbConnectionStringBuilder class, it doesnt work with LocalDB properly.
        //protected mDataSouce = @".\SQLEXPRESS";
        protected string mConnStr;

        public void DeleteDB(string aDbName, string aPathname)
        {
            File.Delete(aDbName);
        }

        public DebugInfo(string aPathname, bool aCreate = false, bool aCreateIndexes = false)
        {
            CurrentInstance = this;

            if (aCreate)
            {
                File.Delete(aPathname);
            }
            aCreate = !File.Exists(aPathname);

            // Manually register the data provider. Do not remove this otherwise the data provider doesn't register properly.
            mConnStr = String.Format("data source={0};journal mode=Memory;synchronous=Off;foreign keys=True;BinaryGuid=false", aPathname);
            // Use the SQLiteConnectionFactory as the default database connection
            // Do not open mConnection before mEntities.CreateDatabase
            mConnection = new SQLiteConnection(mConnStr);

            DapperExtensions.DapperExtensions.DefaultMapper = typeof(PluralizedAutoClassMapper<>);
            DapperExtensions.DapperExtensions.SqlDialect = new SqliteDialect();
            if (aCreate)
            {
                // DatabaseExists checks if the DBName exists, not physical files.
                if (aCreate)
                {
                    mConnection.Open();
                    var xSQL = new SQL(mConnection);
                    xSQL.CreateDB();

                    // Be careful with indexes, they slow down inserts. So on tables that we have a
                    // lot of inserts, but limited look ups, dont add them.
                    //
                    if (aCreateIndexes)
                    {
                        this.CreateIndexes();
                    }
                }
            }
            if (mConnection.State == ConnectionState.Closed)
            {
                mConnection.Open();
            }
        }

        public IDbConnection Connection
        {
            get
            {
                return mConnection;
            }
        }

        /// <summary>
        /// Create indexes inside the database.
        /// </summary>
        public void CreateIndexes()
        {
            var xSQL = new SQL(mConnection);

            xSQL.MakeIndex("Labels", "Address", false);
            xSQL.MakeIndex("Labels", "Name", true);
            xSQL.MakeIndex("Methods", "DocumentID", false);

            xSQL.ExecuteAssemblyResource("SQLiteIndexes.sql");
        }

        // The GUIDs etc are populated by the MSBuild task, so they wont be loaded when the debugger runs.
        // Because of this, we also allow manual loading.
        public void LoadLookups()
        {
            foreach (var xDoc in Connection.Query<Document>(new SQLinq<Document>().ToSQL().ToQuery()))
            {
                DocumentGUIDs.Add(xDoc.Pathname.ToLower(), xDoc.ID);
            }
        }

        public UInt32 AddressOfLabel(string aLabel)
        {
            var xRow = Connection.Query<Label>(new SQLinq<Label>().Where(i => i.Name == aLabel)).FirstOrDefault();

            if (xRow == null)
            {
                return 0;
            }
            return (UInt32)xRow.Address;
        }

        public string[] GetLabels(UInt32 aAddress)
        {
            var xLabels = Connection.Query<Label>(new SQLinq<Label>().Where(i => i.Address == aAddress)).Select(i => i.Name).ToArray();
            return xLabels;
        }

        protected List<string> local_MappingTypeNames = new List<string>();
        public void WriteFieldMappingToFile(IEnumerable<Field_Map> aMapping)
        {
            var xMaps = aMapping.Where(delegate(Field_Map mp)
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

            // Is a real DB now, but we still store all in RAM. We don't need to. Need to change to query DB as needed instead.
            var xItemsToAdd = new List<FIELD_MAPPING>(1024);
            foreach (var xItem in xMaps)
            {
                foreach (var xFieldName in xItem.FieldNames)
                {
                    var xRow = new FIELD_MAPPING();
                    xRow.ID = CreateId();
                    xRow.TYPE_NAME = xItem.TypeName;
                    xRow.FIELD_NAME = xFieldName;
                    xItemsToAdd.Add(xRow);
                }
            }
            BulkInsert<FIELD_MAPPING>("FIELD_MAPPINGS", xItemsToAdd);
        }

        public Field_Map GetFieldMap(string aName)
        {
            var xMap = new Field_Map();
            xMap.TypeName = aName;
            var xRows = mConnection.Query<FIELD_MAPPING>(new SQLinq<FIELD_MAPPING>().Where(i => i.TYPE_NAME == aName));
            foreach (var xFieldName in xRows)
            {
                xMap.FieldNames.Add(xFieldName.FIELD_NAME);
            }
            return xMap;
        }

        public void ReadFieldMappingList(List<Field_Map> aSymbols)
        {
            var xMap = new Field_Map();
            foreach (var xRow in mConnection.GetList<FIELD_MAPPING>())
            {
                string xTypeName = xRow.TYPE_NAME;
                if (xTypeName != xMap.TypeName)
                {
                    if (xMap.FieldNames.Count > 0)
                    {
                        aSymbols.Add(xMap);
                    }
                    xMap = new Field_Map();
                    xMap.TypeName = xTypeName;
                }
                xMap.FieldNames.Add(xRow.FIELD_NAME);
            }
            aSymbols.Add(xMap);
        }

        protected List<string> mLocalFieldInfoNames = new List<string>();
        public void WriteFieldInfoToFile(IList<FIELD_INFO> aFields)
        {
            var itemsToAdd = new List<FIELD_INFO>(aFields.Count);
            foreach (var xItem in aFields)
            {
                if (!mLocalFieldInfoNames.Contains(xItem.NAME))
                {
                    xItem.ID = CreateId();
                    mLocalFieldInfoNames.Add(xItem.NAME);
                    itemsToAdd.Add(xItem);
                }
            }
            BulkInsert<FIELD_INFO>("FIELD_INFOS", itemsToAdd, 2500, true);
        }

        public class SequencePoint
        {
            public int Offset;
            public string Document;
            public int LineStart;
            public int ColStart;
            public Int64 LineColStart
            {
                get { return ((Int64)LineStart << 32) + ColStart; }
            }
            public int LineEnd;
            public int ColEnd;
            public Int64 LineColEnd
            {
                get { return ((Int64)LineEnd << 32) + ColEnd; }
            }
        }

        // This gets the Sequence Points.
        // Sequence Points are spots that identify what the compiler/debugger says is a spot
        // that a breakpoint can occur one. Essentially, an atomic source line in C#
        public SequencePoint[] GetSequencePoints(MethodBase aMethod, bool aFilterHiddenLines = false)
        {
            return GetSequencePoints(aMethod.DeclaringType.Assembly.Location, aMethod.MetadataToken, aFilterHiddenLines);
        }

        public SequencePoint[] GetSequencePoints(string aAsmPathname, int aMethodToken, bool aFilterHiddenLines = false)
        {
            var xReader = Microsoft.Samples.Debugging.CorSymbolStore.SymbolAccess.GetReaderForFile(aAsmPathname);
            if (xReader == null)
            {
                return new SequencePoint[0];
            }

            var xSymbols = xReader.GetMethod(new SymbolToken(aMethodToken));
            if (xSymbols == null)
            {
                return new SequencePoint[0];
            }

            int xCount = xSymbols.SequencePointCount;
            var xOffsets = new int[xCount];
            var xDocuments = new ISymbolDocument[xCount];
            var xStartLines = new int[xCount];
            var xStartCols = new int[xCount];
            var xEndLines = new int[xCount];
            var xEndCols = new int[xCount];

            xSymbols.GetSequencePoints(xOffsets, xDocuments, xStartLines, xStartCols, xEndLines, xEndCols);

            var xResult = new SequencePoint[xCount];
            for (int i = 0; i < xCount; i++)
            {
                var xSP = new SequencePoint();
                xResult[i] = xSP;
                xSP.Offset = xOffsets[i];
                xSP.Document = xDocuments[i].URL;
                xSP.LineStart = xStartLines[i];
                xSP.ColStart = xStartCols[i];
                xSP.LineEnd = xEndLines[i];
                xSP.ColEnd = xEndCols[i];
            }

            if (aFilterHiddenLines)
            {
                return xResult.Where(q => q.LineStart != 0xFEEFEE).ToArray();
            }
            return xResult;
        }

        protected List<Method> mMethods = new List<Method>();
        public void AddMethod(Method aMethod, bool aFlush = false)
        {
            if (aMethod != null)
            {
                mMethods.Add(aMethod);
            }
            BulkInsert("Methods", mMethods, 2500, aFlush);
        }

        // Quick look up of assemblies so we dont have to go to the database and compare by fullname.
        // This and other GUID lists contain only a few members, and save us from issuing a lot of selects to SQL.
        public Dictionary<Assembly, long> AssemblyGUIDs = new Dictionary<Assembly, long>();
        List<Cosmos.Debug.Common.AssemblyFile> xAssemblies = new List<Cosmos.Debug.Common.AssemblyFile>();
        public void AddAssemblies(List<Assembly> aAssemblies, bool aFlush = false)
        {
            if (aAssemblies != null)
            {

                foreach (var xAsm in aAssemblies)
                {
                    var xRow = new Cosmos.Debug.Common.AssemblyFile()
                    {
                        ID = CreateId(),
                        Pathname = xAsm.Location
                    };
                    xAssemblies.Add(xRow);

                    AssemblyGUIDs.Add(xAsm, xRow.ID);
                }
            }
            BulkInsert("AssemblyFiles", xAssemblies, 2500, aFlush);
        }

        public Dictionary<string, long> DocumentGUIDs = new Dictionary<string, long>();
        List<Document> xDocuments = new List<Document>(1);
        public void AddDocument(string aPathname, bool aFlush = false)
        {
            if (aPathname != null)
            {
                aPathname = aPathname.ToLower();

                if (!DocumentGUIDs.ContainsKey(aPathname))
                {
                    var xRow = new Document()
                    {
                        ID = CreateId(),
                        Pathname = aPathname
                    };
                    DocumentGUIDs.Add(aPathname, xRow.ID);
                    // Even though we are inserting only one row, Bulk already has a connection
                    // open so its probably faster than using EF, and its about the same amount of code.
                    // Need to insert right away so RI will be ok when dependents are inserted.
                    xDocuments.Add(xRow);
                    BulkInsert("Documents", xDocuments, 2500, aFlush);
                }
            }
            else
            {
                BulkInsert("Documents", xDocuments, 2500, aFlush);
            }
        }

        public void AddSymbols(IList<MethodIlOp> aSymbols, bool aFlush = false)
        {
            foreach (var x in aSymbols)
            {
                x.ID = CreateId();
            }
            BulkInsert("MethodIlOps", aSymbols, 2500, aFlush);
        }

        public void WriteAllLocalsArgumentsInfos(IList<LOCAL_ARGUMENT_INFO> aInfos)
        {
            foreach (var x in aInfos)
            {
                x.ID = CreateId();
            }
            BulkInsert("LOCAL_ARGUMENT_INFOS", aInfos, aFlush: true);
        }

      private static int DataDumpIndex;

        // EF is slow on bulk operations. But we want to retain explicit bindings to the model to avoid unbound mistakes.
        // SqlBulk operations are on average 15x faster. So we use a hybrid approach by using the entities as containers
        // and EntityDataReader to bridge the gap to SqlBulk.
        //
        // We dont want to issue individual inserts to SQL as this is very slow.
        // But accumulating too many records in RAM also is a problem. For example
        // at time of writing the full structure would take up 11 MB of RAM just for this structure.
        // This is not a huge amount, but as we compile in more and more this figure will grow.
        // So as a compromise, we collect 2500 records then bulk insert.
        public void BulkInsert<T>(string aTableName, IList<T> aList, int aFlushSize = 0, bool aFlush = false) where T : class
        {
            if (aList.Count >= aFlushSize || aFlush)
            {
                if (aList.Count > 0)
                {
                    using (var xBulkCopy = new SqliteBulkCopy(mConnection))
                    {
                        xBulkCopy.DestinationTableName = aTableName;
                        #region debug
                        // for now dump to disk:
                        //using (var reader = new ObjectReader<T>(aList.ToArray()))
                        //{
                        //  var dumpIdx = Interlocked.Increment(ref DataDumpIndex);
                        //  using (var writer = new StreamWriter(@"e:\Temp\sqls\" + dumpIdx.ToString("D8") + ".dmp"))
                        //  {
                        //    writer.WriteLine(typeof(T).FullName);
                        //    writer.WriteLine("Flush = {0}, flush-size = {1}", aFlush, aFlushSize);
                        //    bool first = true;
                        //    while (reader.Read())
                        //    {
                        //      if (first)
                        //      {
                        //        first = false;
                        //        for (int i = 0; i < reader.FieldCount; i++)
                        //        {
                        //          writer.Write(reader.GetName(i));
                        //          if (i < (reader.FieldCount - 1))
                        //          {
                        //            writer.Write("\t");
                        //          }
                        //        }
                        //        writer.WriteLine();
                        //      }
                        //      for (int i = 0; i < reader.FieldCount; i++)
                        //      {
                        //        writer.Write(reader.GetValue(i));
                        //        if (i < (reader.FieldCount - 1))
                        //        {
                        //          writer.Write("\t");
                        //        }
                        //      }
                        //      writer.WriteLine();
                        //    }
                        //  }
                        //}
                        #endregion region debug
                        //using (var db = DB())
                        //{
                        //    db.Set(typeof(T)).AddRange(aList);
                        //    db.SaveChanges();
                        //}
                        //using (var trans = mConnection.BeginTransaction())
                        //{
                        //    try
                        //    {
                        //        mConnection.Insert<T>(aList);
                        //        trans.Commit();
                        //    }
                        //    catch(Exception E)
                        //    {
                        //        trans.Rollback();
                        //    }
                        //}
                        using (var reader = new ObjectReader<T>(aList))
                        {
                            xBulkCopy.WriteToServer(reader);
                        }
                    }
                    aList.Clear();
                }
            }
        }

        public void AddLabels(IList<Label> aLabels, bool aFlush = false)
        {
            // GUIDs inserted by caller
            BulkInsert("Labels", aLabels, 2500, aFlush);
        }

        public void AddINT3Labels(IList<INT3Label> aLabels, bool aFlush = false)
        {
            BulkInsert("INT3Labels", aLabels, 2500, aFlush);
        }

        public void Dispose()
        {
            if (mConnection != null)
            {
                AddAssemblies(null, true);
                AddDocument(null, true);
                AddMethod(null, true);
                var xConn = mConnection;
                xConn.Close();
                xConn = null;
                mConnection = null;
                // Dont set to null... causes problems because of bad code :(
                // Need to fix the whole class, but its here for now.
                //CurrentInstance = null;
            }
        }

        public Label GetMethodHeaderLabel(UInt32 aAddress)
        {
            var xAddress = (long)aAddress;
            var xLabels = mConnection.Query<Label>(new SQLinq<Label>().Where(i => i.Address <= xAddress).OrderByDescending(i => i.Address)).ToArray();

            Label methodHeaderLabel = null;
            //The first label we find searching upwards with "GUID_" at the start will be the very start of the method header
            foreach (var xLabel in xLabels)
            {
                if (xLabel.Name.StartsWith("GUID_"))
                {
                    methodHeaderLabel = xLabel;
                    break;
                }
            }
            return methodHeaderLabel;
        }

        public Label[] GetMethodLabels(uint address)
        {
            var method = GetMethod(address);
            var first = mConnection.Query<Label>(new SQLinq<Label>().Where(i => i.ID == method.LabelStartID)).Single();
            var last = mConnection.Query<Label>(new SQLinq<Label>().Where(i => i.ID == method.LabelEndID)).Single();
            var temp = mConnection.Query<Label>(new SQLinq<Label>().Where(i => i.Address >= first.Address && i.Address <= last.Address)).ToArray();
            var result = new List<Label>(temp.Length);
            //There are always two END__OF__METHOD_EXCEPTION__2 labels at the end of the method footer.
            int endOfMethodException2LabelsFound = 0;
            foreach (var label in temp)
            {
                if (label.Name.IndexOf("END__OF__METHOD_EXCEPTION__2", StringComparison.Ordinal) > -1)
                {
                    endOfMethodException2LabelsFound++;
                }
                result.Add(label);
                if (endOfMethodException2LabelsFound >= 2)
                {
                    break;
                }
            }

            return result.ToArray();
        }

        public Method GetMethod(UInt32 aAddress)
        {
            var method = mConnection.Query<Method>(
                "select Methods.* from methods " +
                "inner join Labels LStart on LStart.ID = methods.LabelStartID " +
                "inner join Labels LEnd on LEnd.ID = Methods.LabelEndID " +
                "where LStart.Address <= @Address and LEnd.Address > @Address;", new { Address = aAddress }).Single();
            return method;
        }

        // Gets MLSymbols for a method, given an address within the method.
        public MethodIlOp[] GetSymbols(Method aMethod)
        {
            var xSymbols = mConnection.Query<MethodIlOp>(new SQLinq<MethodIlOp>().Where(i => i.MethodID == aMethod.ID).OrderBy(i => i.IlOffset)).ToArray();
            return xSymbols;
        }

        public SourceInfos GetSourceInfos(UInt32 aAddress)
        {
            var xResult = new SourceInfos();
            try
            {
                var xMethod = GetMethod(aAddress);
                if (xMethod != null)
                {
                    var xSymbols = GetSymbols(xMethod);
                    var xAssemblyFile = mConnection.Get<AssemblyFile>(xMethod.AssemblyFileID);
                    var xSymbolReader = SymbolAccess.GetReaderForFile(xAssemblyFile.Pathname);
                    var xMethodSymbol = xSymbolReader.GetMethod(new SymbolToken(xMethod.MethodToken));

                    int xSeqCount = xMethodSymbol.SequencePointCount;
                    var xCodeOffsets = new int[xSeqCount];
                    var xCodeDocuments = new ISymbolDocument[xSeqCount];
                    var xCodeLines = new int[xSeqCount];
                    var xCodeColumns = new int[xSeqCount];
                    var xCodeEndLines = new int[xSeqCount];
                    var xCodeEndColumns = new int[xSeqCount];
                    xMethodSymbol.GetSequencePoints(xCodeOffsets, xCodeDocuments, xCodeLines, xCodeColumns, xCodeEndLines, xCodeEndColumns);
                    if (xSymbols.Length == 0 && xSeqCount > 0)
                    {
                        var xSourceInfo = new SourceInfo()
                        {
                            SourceFile = xCodeDocuments[0].URL,
                            Line = xCodeLines[0],
                            LineEnd = xCodeEndLines[0],
                            Column = xCodeColumns[0],
                            ColumnEnd = xCodeEndColumns[0],
                            MethodName = xMethod.LabelCall
                        };
                        xResult.Add(aAddress, xSourceInfo);
                    }
                    else
                    {
                        foreach (var xSymbol in xSymbols)
                        {
                            var xRow = mConnection.Query<Label>(new SQLinq<Label>().Where(i => i.Name == xSymbol.LabelName)).FirstOrDefault();
                            if (xRow != null)
                            {
                                UInt32 xAddress = (UInt32) xRow.Address;
                                // Each address could have mult labels, but this wont matter for SourceInfo, its not tied to label.
                                // So we just ignore duplicate addresses.
                                if (!xResult.ContainsKey(xAddress))
                                {
                                    int xIdx = SourceInfo.GetIndexClosestSmallerMatch(xCodeOffsets, xSymbol.IlOffset);
                                    var xSourceInfo = new SourceInfo()
                                    {
                                        SourceFile = xCodeDocuments[xIdx].URL,
                                        Line = xCodeLines[xIdx],
                                        LineEnd = xCodeEndLines[xIdx],
                                        Column = xCodeColumns[xIdx],
                                        ColumnEnd = xCodeEndColumns[xIdx],
                                        MethodName = xMethod.LabelCall
                                    };
                                    xResult.Add(xAddress, xSourceInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return xResult;
        }

        public List<KeyValuePair<uint, string>> GetAllINT3AddressesForMethod(Method aMethod, bool filterPermanentINT3s)
        {
            var INT3Labels = Connection.Query<INT3Label>(new SQLinq<INT3Label>().Where(i => i.MethodID == aMethod.ID));
            if (filterPermanentINT3s)
            {
                INT3Labels = INT3Labels.Where(x => !x.LeaveAsINT3);
            }
            return INT3Labels.Select(x => new KeyValuePair<uint, string>(AddressOfLabel(x.LabelName), x.LabelName)).ToList();
        }
        public UInt32 GetClosestCSharpBPAddress(UInt32 aAddress)
        {
            // Get the method this address belongs to
            var xMethod = GetMethod(aAddress);

            // Get the assembly file this method belongs to
            var asm = Connection.Get<AssemblyFile>(xMethod.AssemblyFileID);
            // Get the Sequence Points for this method
            var xSPs = GetSequencePoints(asm.Pathname, xMethod.MethodToken);
            // Get the IL Offsets for these sequence points
            var xSPOffsets = xSPs.Select(x => x.Offset);

            // Get all the MethodILOps for this method
            // Then filter to get only the MethodILOps that also have sequence points
            // Then get the MethodILOp with the highest address <= to aAddress

            // Get all ILOps for current method
            // Filter out ones that don't have sequence points associated with them
            // Oorder by increasing address (this will happen by order by method ID because of how label names are constructed)
            var xOps = Connection.Query(new SQLinq<MethodIlOp>().Where(q => q.MethodID == xMethod.ID)).Where(delegate(MethodIlOp x)
            {
                return xSPOffsets.Contains(x.IlOffset);
            }).OrderBy(x => x.MethodID);

            //Search for first one with address > aAddress then use the previous
            UInt32 address = 0;
            foreach (var op in xOps)
            {
                UInt32 addr = AddressOfLabel(op.LabelName);
                if (addr > aAddress)
                {
                    break;
                }
                else
                {
                    address = addr;
                }
            }

            return address;
        }

        /// <summary>
        /// Sets the range in which id would be generated.
        /// </summary>
        /// <param name="idRange">Number which specified id range.</param>
        public static void SetRange(long idRange)
        {
            mPrefix = idRange;
            mLastGuid = idRange;
        }

        /// <summary>
        /// Gets count of ids generated during last session.
        /// </summary>
        /// <returns>Count of generated ids.</returns>
        public static long GeneratedIdsCount()
        {
            return mLastGuid - mPrefix;
        }

        /// <summary>
        /// Generates new id for the symbol.
        /// </summary>
        /// <returns>New value for the id.</returns>
        public static long CreateId()
        {
            mLastGuid++;
            return mLastGuid;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.EntityClient;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Text;
using Microsoft.Win32;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Diagnostics.SymbolStore;
using System.Threading;
using System.Data.SQLite;

namespace Cosmos.Debug.Common {
  public class DebugInfo : IDisposable {

    // Please beware this field, it may cause issues if used incorrectly.
    public static DebugInfo CurrentInstance { get; private set; }

    public class Field_Map {
      public string TypeName { get; set; }
      public List<string> FieldNames = new List<string>();
    }

    protected SQLiteConnection mConnection;
    protected string mDbName;
    // Dont use DbConnectionStringBuilder class, it doesnt work with LocalDB properly.
    //protected mDataSouce = @".\SQLEXPRESS";
    protected string mConnStr;
    protected System.Data.Metadata.Edm.MetadataWorkspace mWorkspace;

    public void DeleteDB(string aDbName, string aPathname) {
      File.Delete(aDbName);
    }

    public DebugInfo(string aPathname, bool aCreate = false) {
      CurrentInstance = this;

      if (aCreate)
      {
        File.Delete(aPathname);
      }
      aCreate = !File.Exists(aPathname);

      mConnStr = String.Format("data source={0};journal mode=Memory;synchronous=Off;foreign keys=True;", aPathname);

      // Initial Catalog is necessary for EDM
      mWorkspace = new System.Data.Metadata.Edm.MetadataWorkspace(
        new string[] { "res://*/" }, new Assembly[] { Assembly.GetExecutingAssembly() });
      // Do not open mConnection before mEntities.CreateDatabase
      mConnection = new SQLiteConnection(mConnStr);
      if (aCreate) {
        using (var xEntities = DB()) {
          // DatabaseExists checks if the DBName exists, not physical files.
          if (aCreate) {
            mConnection.Open();
            var xSQL = new SQL(mConnection);
            xSQL.CreateDB();

            // Be careful with indexes, they slow down inserts. So on tables that we have a 
            // lot of inserts, but limited look ups, dont add them.
            //
            xSQL.MakeIndex("Labels", "Address", false);
            xSQL.MakeIndex("Labels", "Name", true);
            xSQL.MakeIndex("Methods", "DocumentID", false);
          }
        }
      }
      if (mConnection.State == ConnectionState.Closed) {
        mConnection.Open();
      }
    }

    // The GUIDs etc are populated by the MSBuild task, so they wont be loaded when the debugger runs.
    // Because of this, we also allow manual loading.
    public void LoadLookups() {
      using (var xDB = DB()) {
        foreach (var xDoc in xDB.Documents) {
          DocumentGUIDs.Add(xDoc.Pathname, xDoc.ID);
        }
      }
    }

    public UInt32 AddressOfLabel(string aLabel) {
      using (var xDB = DB()) {
        var xRow = xDB.Labels.SingleOrDefault(q => q.Name == aLabel);
        if (xRow == null) {
          return 0;
        }
        return (UInt32)xRow.Address;
      }
    }

    public string[] GetLabels(UInt32 aAddress) {
      using (var xDB = DB()) {
        var xLabels = from x in xDB.Labels
                      where x.Address == aAddress
                      select x.Name;
        return xLabels.ToArray();
      }
    }

    protected List<string> local_MappingTypeNames = new List<string>();
    public void WriteFieldMappingToFile(IEnumerable<Field_Map> aMapping) {
      var xMaps = aMapping.Where(delegate(Field_Map mp) {
        if (local_MappingTypeNames.Contains(mp.TypeName)) {
          return false;
        } else {
          local_MappingTypeNames.Add(mp.TypeName);
          return true;
        }
      });

      // Is a real DB now, but we still store all in RAM. We don't need to. Need to change to query DB as needed instead.
      using (var xDB = DB()) {
        foreach (var xItem in xMaps) {
          foreach (var xFieldName in xItem.FieldNames) {
            var xRow = new FIELD_MAPPING();
            xRow.ID = NewGuid();
            xRow.TYPE_NAME = xItem.TypeName;
            xRow.FIELD_NAME = xFieldName;
            xDB.FIELD_MAPPING.AddObject(xRow);
          }
        }
        xDB.SaveChanges();
      }
    }

    public Field_Map GetFieldMap(string aName) {
      var xMap = new Field_Map();
      xMap.TypeName = aName;
      using (var xDB = DB()) {
        var xRows = from x in xDB.FIELD_MAPPING
                    where x.TYPE_NAME == aName
                    select x.FIELD_NAME;
        foreach (var xFieldName in xRows) {
          xMap.FieldNames.Add(xFieldName);
        }
      }
      return xMap;
    }

    public void ReadFieldMappingList(List<Field_Map> aSymbols) {
      using (var xDB = DB()) {
        var xMap = new Field_Map();
        foreach (var xRow in xDB.FIELD_MAPPING) {
          string xTypeName = xRow.TYPE_NAME;
          if (xTypeName != xMap.TypeName) {
            if (xMap.FieldNames.Count > 0) {
              aSymbols.Add(xMap);
            }
            xMap = new Field_Map();
            xMap.TypeName = xTypeName;
          }
          xMap.FieldNames.Add(xRow.FIELD_NAME);
        }
        aSymbols.Add(xMap);
      }
    }

    private static Guid NewGuid()
    {
      return Guid.NewGuid();
    }

    protected List<string> mLocalFieldInfoNames = new List<string>();
    public void WriteFieldInfoToFile(IEnumerable<FIELD_INFO> aFields) {
      using (var xDB = DB()) {
        foreach (var xItem in aFields) {
          if (!mLocalFieldInfoNames.Contains(xItem.NAME)) {
            xItem.ID = NewGuid();
            mLocalFieldInfoNames.Add(xItem.NAME);
            xDB.FIELD_INFO.AddObject(xItem);
          }
        }
        xDB.SaveChanges();
      }
    }

    public Entities DB() {
      // We have to create a new connection each time because threads can call this
      // function and it causes issues for different threads to share the same connection, 
      // even if they have different Entity (context) instances.
      var xEntConn = new EntityConnection(mWorkspace, new SQLiteConnection(mConnStr));
      return new Entities(xEntConn);
    }

    public class SequencePoint {
      public int Offset;
      public string Document;
      public int LineStart;
      public int ColStart;
      public Int64 LineColStart {
        get { return ((Int64)LineStart << 32) + ColStart; }
      }
      public int LineEnd;
      public int ColEnd;
      public Int64 LineColEnd {
        get { return ((Int64)LineEnd << 32) + ColEnd; }
      }
    }

    // This gets the Sequence Points.
    // Sequence Points are spots that identify what the compiler/debugger says is a spot
    // that a breakpoint can occur one. Essentially, an atomic source line in C#
    public SequencePoint[] GetSequencePoints(MethodBase aMethod, bool aFilterHiddenLines = false) {
      return GetSequencePoints(aMethod.DeclaringType.Assembly.Location, aMethod.MetadataToken, aFilterHiddenLines);
    }

    public SequencePoint[] GetSequencePoints(string aAsmPathname, int aMethodToken, bool aFilterHiddenLines = false) {
      var xReader = Microsoft.Samples.Debugging.CorSymbolStore.SymbolAccess.GetReaderForFile(aAsmPathname);
      if (xReader == null) {
        return new SequencePoint[0];
      }

      var xSymbols = xReader.GetMethod(new SymbolToken(aMethodToken));
      if (xSymbols == null) {
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
      for (int i = 0; i < xCount; i++) {
        var xSP = new SequencePoint();
        xResult[i] = xSP;
        xSP.Offset = xOffsets[i];
        xSP.Document = xDocuments[i].URL;
        xSP.LineStart = xStartLines[i];
        xSP.ColStart = xStartCols[i];
        xSP.LineEnd = xEndLines[i];
        xSP.ColEnd = xEndCols[i];
      }

      if (aFilterHiddenLines) {
        return xResult.Where(q => q.LineStart != 0xFEEFEE).ToArray();
      }
      return xResult;
    }

    protected List<Method> mMethods = new List<Method>();
    public void AddMethod(Method aMethod, bool aFlush = false) {
      if (aMethod != null) {
        mMethods.Add(aMethod);
      }
      BulkInsert("Methods", mMethods, 2500, aFlush);
    }

    // Quick look up of assemblies so we dont have to go to the database and compare by fullname.
    // This and other GUID lists contain only a few members, and save us from issuing a lot of selects to SQL.
    public Dictionary<Assembly, Guid> AssemblyGUIDs = new Dictionary<Assembly, Guid>();
    List<Cosmos.Debug.Common.AssemblyFile> xAssemblies = new List<Cosmos.Debug.Common.AssemblyFile>();
    public void AddAssemblies(List<Assembly> aAssemblies, bool aFlush = false) {
        if (aAssemblies != null)
        {
            
            foreach (var xAsm in aAssemblies)
            {
                var xRow = new Cosmos.Debug.Common.AssemblyFile()
                {
                    ID = Guid.NewGuid(),
                    Pathname = xAsm.Location
                };
                xAssemblies.Add(xRow);

                AssemblyGUIDs.Add(xAsm, xRow.ID);
            }
        }
        BulkInsert("AssemblyFiles", xAssemblies, 2500, aFlush);
    }

    public Dictionary<string, Guid> DocumentGUIDs = new Dictionary<string, Guid>();
    List<Document> xDocuments = new List<Document>(1);
    public void AddDocument(string aPathname, bool aFlush = false)
    {
        if (aPathname != null)
        {
            if (!DocumentGUIDs.ContainsKey(aPathname))
            {
                var xRow = new Document()
                {
                    ID = Guid.NewGuid(),
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

    public void AddSymbols(IList<MethodIlOp> aSymbols, bool aFlush = false) {
      foreach (var x in aSymbols) {
        x.ID = Guid.NewGuid();
      }
      BulkInsert("MethodIlOps", aSymbols, 2500, aFlush);
    }

    public void WriteAllLocalsArgumentsInfos(IList<LOCAL_ARGUMENT_INFO> aInfos) {
      foreach (var x in aInfos) {
        x.ID = Guid.NewGuid();
      }
      BulkInsert("LOCAL_ARGUMENT_INFO", aInfos);
    }

    // EF is slow on bulk operations. But we want to retain explicit bindings to the model to avoid unbound mistakes.
    // SqlBulk operations are on average 15x faster. So we use a hybrid approach by using the entities as containers
    // and EntityDataReader to bridge the gap to SqlBulk.
    //
    // We dont want to issue individual inserts to SQL as this is very slow.
    // But accumulating too many records in RAM also is a problem. For example 
    // at time of writing the full structure would take up 11 MB of RAM just for this structure.
    // This is not a huge amount, but as we compile in more and more this figure will grow.
    // So as a compromise, we collect 2500 records then bulk insert.
    public void BulkInsert<T>(string aTableName, IList<T> aList, int aFlushSize = 0, bool aFlush = false) {
      if (aList.Count >= aFlushSize /*|| aFlush*/) {
        if (aList.Count > 0) {
          using (var xBulkCopy = new SqliteBulkCopy(mConnection)) {
            xBulkCopy.DestinationTableName = aTableName;
            // for now dump to disk:
            //using (var reader = aList.AsDataReader())
            //{
            //  var dumpIdx = Interlocked.Increment(ref DataDumpIndex);
            //  using (var writer = new StreamWriter(@"c:\temp\dataout\" + dumpIdx + ".dmp"))
            //  {
            //    writer.WriteLine(typeof(T).FullName);
            //    writer.WriteLine("Flush = {0}", aFlush);
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
            xBulkCopy.WriteToServer(aList.AsDataReader());
          }
          aList.Clear();
        }
      }
    }

    private static int DataDumpIndex = 0;

    public void AddLabels(IList<Label> aLabels, bool aFlush = false) {
      // GUIDs inserted by caller
      BulkInsert("Labels", aLabels, 2500, aFlush);
    }

    public void Dispose() {
      if (mConnection != null) {
        var xConn = mConnection;
        xConn.Close();
        xConn = null;
        mConnection = null;
        // Dont set to null... causes problems because of bad code :(
        // Need to fix the whole class, but its here for now.
        //CurrentInstance = null;
      }
    }

    public Method GetMethod(Entities aDB, UInt32 aAddress) {
        // The address we have is somewhere in the method, but we need to find 
        // one that is also in MLSymbol. Asm labels for example wont be found.
        // So we find ones that match or are before, and we walk till we fine one
        // in MLSymbol.
        var xLabels = from x in aDB.Labels
                      where x.Address <= aAddress
                      orderby x.Address descending
                      select x.Name;

        // Search till we find a matching label.
        MethodIlOp xSymbol = null;
        foreach (var xLabel in xLabels) {
          xSymbol = aDB.MethodIlOps.SingleOrDefault(q => q.LabelName == xLabel);
          if (xSymbol != null) {
            break;
          }
        }

        if (xSymbol == null) {
          throw new Exception("Label not found.");
        }
        return xSymbol.Method;
    }

    // Gets MLSymbols for a method, given an address within the method.
    public IEnumerable<MethodIlOp> GetSymbols(Entities aDB, Method aMethod) {
        var xSymbols = from x in aDB.MethodIlOps
                       where x.MethodID == aMethod.ID
                       orderby x.IlOffset
                       select x;
        return xSymbols;
    }

    public SourceInfos GetSourceInfos(UInt32 aAddress) {
      var xResult = new SourceInfos();
      using (var xDB = DB()) {
        var xMethod = GetMethod(xDB, aAddress);
        var xSymbols = GetSymbols(xDB, xMethod);
        var xSymbolReader = SymbolAccess.GetReaderForFile(xMethod.AssemblyFile.Pathname);
        var xMethodSymbol = xSymbolReader.GetMethod(new SymbolToken(xMethod.MethodToken));

        int xSeqCount = xMethodSymbol.SequencePointCount;
        var xCodeOffsets = new int[xSeqCount];
        var xCodeDocuments = new ISymbolDocument[xSeqCount];
        var xCodeLines = new int[xSeqCount];
        var xCodeColumns = new int[xSeqCount];
        var xCodeEndLines = new int[xSeqCount];
        var xCodeEndColumns = new int[xSeqCount];
        xMethodSymbol.GetSequencePoints(xCodeOffsets, xCodeDocuments, xCodeLines, xCodeColumns, xCodeEndLines, xCodeEndColumns);

        foreach (var xSymbol in xSymbols) {
          var xRow = xDB.Labels.SingleOrDefault(q => q.Name == xSymbol.LabelName);
          if (xRow != null) {
            UInt32 xAddress = (UInt32)xRow.Address;
            // Each address could have mult labels, but this wont matter for SourceInfo, its not tied to label.
            // So we just ignore duplicate addresses.
            if (!xResult.ContainsKey(xAddress)) {
              int xIdx = SourceInfo.GetIndexClosestSmallerMatch(xCodeOffsets, xSymbol.IlOffset);
              var xSourceInfo = new SourceInfo() {
                SourceFile = xCodeDocuments[xIdx].URL,
                Line = xCodeLines[xIdx],
                LineEnd = xCodeEndLines[xIdx],
                Column = xCodeColumns[xIdx],
                ColumnEnd = xCodeEndColumns[xIdx],
                MethodName = xSymbol.Method.LabelCall
              };
              xResult.Add(xAddress, xSourceInfo);
            }
          }
        }
      }
      return xResult;
    }

  }

}
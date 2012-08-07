using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Diagnostics.SymbolStore;

namespace Cosmos.Debug.Common {
  public class DebugInfo : IDisposable {

    // Please beware this field, it may cause issues if used incorrectly.
    public static DebugInfo CurrentInstance { get; private set; }

    public class Field_Map {
      public string TypeName { get; set; }
      public List<string> FieldNames = new List<string>();
    }

    protected SqlConnection mConnection;
    protected string mDbName;
    // Dont use DbConnectionStringBuilder class, it doesnt work with LocalDB properly.
    protected string mDataSouce = @"(LocalDB)\v11.0";
    //protected mDataSouce = @".\SQLEXPRESS";
    protected string mConnStrBase;
    protected string mConnStr;
    protected System.Data.Metadata.Edm.MetadataWorkspace mWorkspace;

    public void DeleteDB() {
      using (var xConn = new SqlConnection(mConnStrBase)) {
        xConn.Open();

        bool xExists = false;
        using (var xCmd = xConn.CreateCommand()) {
          xCmd.CommandText = "select * from sys.databases where name = '" + mDbName + "'";
          using (var xReader = xCmd.ExecuteReader()) {
            xExists = xReader.Read();
          }
        }

        if (xExists) {
          // Necessary to because of SQL pooled connections etc, even if all our connections are closed.
          using (var xCmd = xConn.CreateCommand()) {
            xCmd.CommandText = "ALTER DATABASE " + mDbName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            xCmd.ExecuteNonQuery();
          }
          // Yes this throws an exception if the database doesnt exist, so we have to
          // run it only if we know it exists.
          // This will detach and also delete the physica files.
          using (var xCmd = xConn.CreateCommand()) {
            xCmd.CommandText = "DROP DATABASE " + mDbName;
            xCmd.ExecuteNonQuery();
          }
        }
      }
    }

    public DebugInfo(string aPathname, bool aCreate = false) {
      CurrentInstance = this;

      mDbName = Path.GetFileNameWithoutExtension(aPathname);
      // SQL doesnt like - in db names.
      mDbName = mDbName.Replace("-", ""); ;

      mConnStrBase = @"Data Source=" + mDataSouce + ";Integrated Security=True;MultipleActiveResultSets=True;";

      if (aCreate) {
        DeleteDB();
      }

      // Initial Catalog is necessary for EDM
      mConnStr = mConnStrBase + "Initial Catalog=" + mDbName + ";AttachDbFilename=" + aPathname + ";";
      mWorkspace = new System.Data.Metadata.Edm.MetadataWorkspace(
        new string[] { "res://*/" }, new Assembly[] { Assembly.GetExecutingAssembly() });
      // Do not open mConnection before mEntities.CreateDatabase
      mConnection = new SqlConnection(mConnStr);
      if (aCreate) {
        using (var xEntities = DB()) {
          // DatabaseExists checks if the DBName exists, not physical files.
          if (!xEntities.DatabaseExists()) {
            xEntities.CreateDatabase();
            mConnection.Open();
            var xSQL = new SQL(mConnection);

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

    protected List<string> mLocalFieldInfoNames = new List<string>();
    public void WriteFieldInfoToFile(IEnumerable<FIELD_INFO> aFields) {
      using (var xDB = DB()) {
        foreach (var xItem in aFields) {
          if (!mLocalFieldInfoNames.Contains(xItem.NAME)) {
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
      var xEntConn = new EntityConnection(mWorkspace, new SqlConnection(mConnStr));
      return new Entities(xEntConn);
    }

    public class SequencePoint {
      public int Offset;
      public string Document;
      public int LineStart;
      public int ColStart;
      public int LineEnd;
      public int ColEnd;
    }
    // This gets the Sequence Points.
    // Sequence Points are spots that identify what the compiler/debugger says is a spot
    // that a breakpoint can occur one. Essentially, an atomic source line in C#
    public SequencePoint[] GetSequencePoints(MethodBase aMethod, bool aFilterHiddenLines = false) {
      var xReader = Microsoft.Samples.Debugging.CorSymbolStore.SymbolAccess.GetReaderForFile(aMethod.DeclaringType.Assembly.Location);
      if (xReader == null) {
        return new SequencePoint[0];
      }

      var xSymbols = xReader.GetMethod(new SymbolToken(aMethod.MetadataToken));
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
        aMethod.ID = Guid.NewGuid();
        mMethods.Add(aMethod);
      }
      BulkInsert("Methods", mMethods, 2500, aFlush);
    }

    // Quick look up of assemblies so we dont have to go to the database and compare by fullname.
    // This and other GUID lists contain only a few members, and save us from issuing a lot of selects to SQL.
    public Dictionary<Assembly, Guid> AssemblyGUIDs = new Dictionary<Assembly, Guid>();
    public void AddAssemblies(List<Assembly> aAssemblies) {
      var xAssemblies = new List<Cosmos.Debug.Common.AssemblyFile>();
      foreach (var xAsm in aAssemblies) {
        var xRow = new Cosmos.Debug.Common.AssemblyFile() {
          ID = Guid.NewGuid(),
          Pathname = xAsm.Location
        };
        xAssemblies.Add(xRow);

        AssemblyGUIDs.Add(xAsm, xRow.ID);
      }
      BulkInsert("AssemblyFiles", xAssemblies, 0, true);
    }

    public Dictionary<string, Guid> DocumentGUIDs = new Dictionary<string, Guid>();
    public void AddDocument(string aPathname) {
      if (!DocumentGUIDs.ContainsKey(aPathname)) {
        var xRow = new Document() {
          ID = Guid.NewGuid(),
          Pathname = aPathname
        };
        DocumentGUIDs.Add(aPathname, xRow.ID);

        // Even though we are inserting only one row, Bulk already has a connection
        // open so its probably faster than using EF, and its about the same amount of code.
        // Need to insert right away so RI will be ok when dependents are inserted.
        var xDocuments = new List<Document>(1);
        xDocuments.Add(xRow);
        BulkInsert("Documents", xDocuments, 0, true);
      }
    }

    public void WriteSymbols(IList<MethodIlOp> aSymbols, bool aFlush = false) {
      foreach (var x in aSymbols) {
        x.ID = Guid.NewGuid();
      }
      BulkInsert("MLSYMBOLs", aSymbols, 2500, aFlush);
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
    public void BulkInsert<T>(string aTableName, IList<T> aList, int aFlushSize = 0, bool aFlush = true) {
      if (aList.Count >= aFlushSize || aFlush) {
        if (aList.Count > 0) {
          using (var xBulkCopy = new SqlBulkCopy(mConnection)) {
            xBulkCopy.DestinationTableName = aTableName;
            xBulkCopy.WriteToServer(aList.AsDataReader());
          }
          aList.Clear();
        }
      }
    }

    public void AddLabels(IList<Label> aLabels, bool aFlush = false) {
      // GUIDs inserted by caller
      BulkInsert("Labels", aLabels, 2500, aFlush);
    }

    public void Dispose() {
      if (mConnection != null) {
        var xConn = mConnection;
        mConnection = null;
        xConn.Close();
        // Dont set to null... causes problems because of bad code :(
        // Need to fix the whole class, but its here for now.
        //CurrentInstance = null;
      }
    }

    // Gets MLSymbols for a method, given an address within the method.
    public MethodIlOp[] GetSymbols(UInt32 aAddress) {
      using (var xDB = DB()) {
        // The address we have is somewhere in the method, but we need to find 
        // one that is also in MLSymbol. Asm labels for example wont be found.
        // So we find ones that match or are before, and we walk till we fine one
        // in MLSymbol.
        var xLabels = from x in xDB.Labels
                      where x.Address <= aAddress
                      orderby x.Address descending
                      select x.Name;

        // Search till we find a matching label.
        MethodIlOp xSymbol = null;
        foreach (var xLabel in xLabels) {
          xSymbol = xDB.MethodIlOps.SingleOrDefault(q => q.LabelName == xLabel);
          if (xSymbol != null) {
            break;
          }
        }
        if (xSymbol == null) {
          throw new Exception("Label not found.");
        }

        // Now get all MLSymbols for the method.
        var xSymbols = from x in xDB.MethodIlOps
                       where
                         x.METHODTOKEN == xSymbol.METHODTOKEN
                         && x.ILASMFILE == xSymbol.ILASMFILE
                       orderby x.IlOffset
                       select x;
        return xSymbols.ToArray();
      }
    }

    public SourceInfos GetSourceInfos(UInt32 aAddress) {
      var xResult = new SourceInfos();
      var xSymbols = GetSymbols(aAddress);
      var xSymbolReader = SymbolAccess.GetReaderForFile(xSymbols[0].ILASMFILE);
      var xMethodSymbol = xSymbolReader.GetMethod(new SymbolToken(xSymbols[0].METHODTOKEN));

      int xSeqCount = xMethodSymbol.SequencePointCount;
      var xCodeOffsets = new int[xSeqCount];
      var xCodeDocuments = new ISymbolDocument[xSeqCount];
      var xCodeLines = new int[xSeqCount];
      var xCodeColumns = new int[xSeqCount];
      var xCodeEndLines = new int[xSeqCount];
      var xCodeEndColumns = new int[xSeqCount];
      xMethodSymbol.GetSequencePoints(xCodeOffsets, xCodeDocuments, xCodeLines, xCodeColumns, xCodeEndLines, xCodeEndColumns);

      using (var xDB = DB()) {
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
                MethodName = xSymbol.METHODNAME
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
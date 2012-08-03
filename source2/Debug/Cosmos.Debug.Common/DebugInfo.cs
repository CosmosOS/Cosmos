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

            // Labels
            // Labels is a big table. Avoid indexes when possible, because we need inserts to be fast.
            // -ADDRESS - Dont index - We dont look up on it very much
            // -LABELNAME - We do lookup a lot on this, but will change to asm line as key prob
            xSQL.MakeIndex("Labels", "LABELNAME", true);
          }
        }
      }
      if (mConnection.State == ConnectionState.Closed) {
        mConnection.Open();
      }
    }

    public UInt32 AddressOfLabel(string aLabel) {
      using (var xDB = DB()) {
        var xRow = xDB.Labels.SingleOrDefault(q => q.LABELNAME == aLabel);
        if (xRow == null) {
          return 0;
        }
        return (UInt32)xRow.ADDRESS;
      } 
    }

    public string[] GetLabels(UInt32 aAddress) {
      using (var xDB = DB()) {
        var xLabels = from x in xDB.Labels
                      where x.ADDRESS == aAddress
                      select x.LABELNAME;
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

    public void WriteSymbolsListToFile(IList<MLSYMBOL> aSymbols) {
      foreach (var x in aSymbols) {
        x.ID = Guid.NewGuid();
      }
      BulkInsert("MLSYMBOLs", aSymbols);
    }

    // tuple format: MethodLabel, IsArgument, Index, Offset
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

    public void WriteLabels(IList<Label> aLabels, bool aFlush = false) {
      foreach (var x in aLabels) {
        x.ID = Guid.NewGuid();
      }
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
  }

}
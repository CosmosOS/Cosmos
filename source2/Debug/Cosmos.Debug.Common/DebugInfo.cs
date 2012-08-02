using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.Objects;
using System.Reflection;
using Microsoft.Win32;

namespace Cosmos.Debug.Common {
  public class DebugInfo : IDisposable {

    // Please beware this field, it may cause issues if used incorrectly.
    public static DebugInfo CurrentInstance { get; private set; }
    public class Field_Info {
      public string Type { get; set; }
      public int Offset { get; set; }
      public string Name { get; set; }
    }

    public class Field_Map {
      public string TypeName { get; set; }
      public List<string> FieldNames = new List<string>();
    }

    public class MLDebugSymbol {
      public string LabelName { get; set; }
      public int StackDifference { get; set; }
      public string AssemblyFile { get; set; }
      public int TypeToken { get; set; }
      public int MethodToken { get; set; }
      public int ILOffset { get; set; }
      public string MethodName { get; set; }
    }

    public class Local_Argument_Info {
      public bool IsArgument { get; set; }
      public bool IsArrayElement { get; set; }
      public string MethodLabelName { get; set; }
      public int Index { get; set; }
      public int Offset { get; set; }
      public string Name { get; set; }
      public string Type { get; set; }
    }

    protected SqlConnection mConnection;
    protected string mDbName;
    // Dont use DbConnectionStringBuilder class, it doesnt work with LocalDB properly.
    protected string mDataSouce = @"(LocalDB)\v11.0";
    //protected mDataSouce = @".\SQLEXPRESS";
    protected string mConnStrBase;
    protected EntityConnection mEntConn;

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
      string xConnStr = mConnStrBase + "Initial Catalog=" + mDbName + ";AttachDbFilename=" + aPathname + ";";

      var xWorkspace = new System.Data.Metadata.Edm.MetadataWorkspace(
        new string[] { "res://*/" }, new Assembly[] { Assembly.GetExecutingAssembly() });
      mEntConn = new EntityConnection(xWorkspace, new SqlConnection(xConnStr));
      // Do not open mConnection before mEntities.CreateDatabase
      if (aCreate) {
        using (var xEntities = new Entities(mEntConn)) {
          // DatabaseExists checks if the DBName exists, not physical files.
          if (!xEntities.DatabaseExists()) {
            xEntities.CreateDatabase();
          }
        }
      }

      mConnection = new SqlConnection(xConnStr);
      mConnection.Open();
    }

    protected List<string> local_MappingTypeNames = new List<string>();
    public void WriteFieldMappingToFile(IEnumerable<Field_Map> aMapping) {
      IEnumerable<Field_Map> xMaps = aMapping.Where(delegate(Field_Map mp) {
        if (local_MappingTypeNames.Contains(mp.TypeName)) {
          return false;
        } else {
          local_MappingTypeNames.Add(mp.TypeName);
          return true;
        }
      });

      // Is a real DB now, but we still store all in RAM. We don't need to. Need to change to query DB as needed instead.
      using (var xDB = new Entities(mEntConn)) {
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
      using (var xDB = new Entities(mEntConn)) {
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
      using (var xDB = new Entities(mEntConn)) {
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
    public void WriteFieldInfoToFile(IEnumerable<Field_Info> aFields) {
      IEnumerable<Field_Info> xFields = aFields.Where(delegate(Field_Info mp) {
        if (mLocalFieldInfoNames.Contains(mp.Name)) {
          return false;
        } else {
          mLocalFieldInfoNames.Add(mp.Name);
          return true;
        }
      });

      // Is a real DB now, but we still store all in RAM. We don't need to. Need to change to query DB as needed instead.
      using (var xDB = new Entities(mEntConn)) {
        foreach (var xItem in xFields) {
          var xRow = new FIELD_INFO();
          xRow.TYPE = xItem.Type;
          xRow.OFFSET = xItem.Offset;
          xRow.NAME = xItem.Name;
          xDB.FIELD_INFO.AddObject(xRow);
        }
        xDB.SaveChanges();
      }
    }

    public Field_Info GetFieldInfo(string aName) {
      var xInf = new Field_Info();
      using (var xDB = new Entities(mEntConn)) {
        var xRow = xDB.FIELD_INFO.Where(q => q.NAME == aName).First();
        xInf.Type = xRow.TYPE;
        xInf.Offset = xRow.OFFSET;
        xInf.Name = xRow.NAME;
      }
      return xInf;
    }

    public void ReadFieldInfoList(List<Field_Info> aSymbols) {
      using (var xDB = new Entities(mEntConn)) {
        foreach (var xRow in xDB.FIELD_INFO) {
          aSymbols.Add(new Field_Info {
            Type = xRow.TYPE,
            Offset = xRow.OFFSET,
            Name = xRow.NAME,
          });
        }
      }
    }

    public List<MLDebugSymbol> ReadSymbolsList() {
      var xResult = new List<MLDebugSymbol>();
      using (var xDB = new Entities(mEntConn)) {
        foreach (var xRow in xDB.MLSYMBOLs) {
          xResult.Add(new MLDebugSymbol {
            LabelName = xRow.LABELNAME,
            StackDifference = xRow.STACKDIFF,
            AssemblyFile = xRow.ILASMFILE,
            TypeToken = xRow.TYPETOKEN,
            MethodToken = xRow.METHODTOKEN,
            ILOffset = xRow.ILOFFSET,
            MethodName = xRow.METHODNAME
          });
        }
      }
      return xResult;
    }

    public MLDebugSymbol ReadSymbolByLabelName(string aLabelName) {
      using (var xDB = new Entities(mEntConn)) {
        var xRow = xDB.MLSYMBOLs.Where(q => q.LABELNAME == aLabelName).FirstOrDefault();
        if (xRow == null) {
          return null;
        }
        return new MLDebugSymbol {
          LabelName = xRow.LABELNAME,
          StackDifference = xRow.STACKDIFF,
          AssemblyFile = xRow.ILASMFILE,
          TypeToken = xRow.TYPETOKEN,
          MethodToken = xRow.METHODTOKEN,
          ILOffset = xRow.ILOFFSET,
          MethodName = xRow.METHODNAME
        };
      }
    }

    public IList<Local_Argument_Info> ReadLocalArgumentsInfos(string aMethodLabelName) {
      var xResult = new List<Local_Argument_Info>();
      using (var xDB = new Entities(mEntConn)) {
        foreach (var xRow in xDB.LOCAL_ARGUMENT_INFO) {
          xResult.Add(new Local_Argument_Info {
            MethodLabelName = xRow.METHODLABELNAME,
            IsArgument = xRow.ISARGUMENT == 1,
            Index = xRow.INDEXINMETHOD,
            Offset = xRow.OFFSET,
            Name = xRow.NAME,
            Type = xRow.TYPENAME
          });
        }
      }
      return xResult;
    }

    public void ReadLabels(out List<KeyValuePair<uint, string>> oLabels, out IDictionary<string, uint> oLabelAddressMappings) {
      oLabels = new List<KeyValuePair<uint, string>>();
      oLabelAddressMappings = new Dictionary<string, uint>();
      using (var xDB = new Entities(mEntConn)) {
        foreach (var xRow in xDB.Labels) {
          oLabels.Add(new KeyValuePair<uint, string>((uint)xRow.ADDRESS, xRow.LABELNAME));
          oLabelAddressMappings.Add(xRow.LABELNAME, (uint)xRow.ADDRESS);
        }
      }
    }

    public void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols) {
      // Is a real DB now, but we still store all in RAM. We dont need to. Need to change to query DB as needed instead.
        var xSymbols = new List<MLSYMBOL>();
        foreach (var xItem in aSymbols) {
          var xRow = new MLSYMBOL();
          xRow.ID = Guid.NewGuid();
          xRow.LABELNAME = xItem.LabelName;
          xRow.STACKDIFF = xItem.StackDifference;
          xRow.ILASMFILE = xItem.AssemblyFile;
          xRow.TYPETOKEN = xItem.TypeToken;
          xRow.METHODTOKEN = xItem.MethodToken;
          xRow.ILOFFSET = xItem.ILOffset;
          xRow.METHODNAME = xItem.MethodName;
          xSymbols.Add(xRow);
        }
        BulkInsert("MLSYMBOLs", xSymbols.AsDataReader());
    }

    // tuple format: MethodLabel, IsArgument, Index, Offset
    public void WriteAllLocalsArgumentsInfos(IEnumerable<Local_Argument_Info> aInfos) {
      using (var xDB = new Entities(mEntConn)) {
        foreach (var xInfo in aInfos) {
          var xRow = new LOCAL_ARGUMENT_INFO();
          xRow.METHODLABELNAME = xInfo.MethodLabelName;
          xRow.ISARGUMENT = (short)(xInfo.IsArgument ? 1 : 0);
          xRow.INDEXINMETHOD = xInfo.Index;
          xRow.OFFSET = xInfo.Offset;
          xRow.NAME = xInfo.Name;
          xRow.TYPENAME = xInfo.Type;
          xDB.LOCAL_ARGUMENT_INFO.AddObject(xRow);
        }
        xDB.SaveChanges();
      }
    }

    // This is a heck of a lot easier than using sequences.
    // Can prob change to the new GUIDs we use.
    protected int mMethodId = 0;
    public int AddMethod(string aLabelPrefix) {
      mMethodId++;
      using (var xDB = new Entities(mEntConn)) {
       var xRow = new Method();
        xRow.MethodId = mMethodId;
        xRow.LabelPrefix = aLabelPrefix;
        xDB.Methods.AddObject(xRow);
        xDB.SaveChanges();
      }
      return mMethodId;
    }

    // EF is slow on bulk operations. But we want to retain explicit bindings to the model to avoid unbound mistakes.
    // SqlBulk operations are on average 15x faster. So we use a hybrid approach by using the entities as containers
    // and EntityDataReader to bridge the gap to SqlBulk.
    public void BulkInsert(string aTableName, IDataReader aReader) {
      using (var xBulkCopy = new SqlBulkCopy(mConnection)) {
        xBulkCopy.DestinationTableName = aTableName;
        xBulkCopy.WriteToServer(aReader);
      }
    }

    public void WriteLabels(List<Label> aLabels) {
      BulkInsert("Labels", aLabels.AsDataReader());
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
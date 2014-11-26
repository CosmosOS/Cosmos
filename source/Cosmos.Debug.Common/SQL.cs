using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;

namespace Cosmos.Debug.Common {
  public class SQL {
    public readonly SQLiteConnection Connection;

    public SQL(SQLiteConnection aConnection)
    {
      Connection = aConnection;
    }

    public int? SelectOneInt(string aSql) {
      var xQry = Connection.CreateCommand();
      xQry.CommandText = aSql;
      return (int)xQry.ExecuteScalar();
    }

    public void Exec(string aSql) {
      var xQry = Connection.CreateCommand();
      xQry.CommandText = aSql;
      xQry.ExecuteNonQuery();
    }

    public void MakeUniqueInt(string aTable, string aCol, bool aAllowNulls) {
      string xIdxName = "Uq_" + aTable + "_" + aCol;
      if (aAllowNulls) {
        //http://sqlservercodebook.blogspot.com/2008/04/multiple-null-values-in-unique-index-in.html
        Exec("ALTER TABLE " + aTable + " ADD " + xIdxName + " AS (CASE WHEN " + aCol + " IS NULL THEN Id * -1 ELSE " + aCol + " END);");
        // We create index seperately instead of using unique keyword on previous SQL
        // so we can control name of the index for better identification
        Exec("CREATE UNIQUE INDEX Idx" + xIdxName + " ON " + aTable + "(" + xIdxName + ");");
      } else {
        Exec("CREATE UNIQUE INDEX Idx" + xIdxName + " ON " + aTable + "(" + aCol + ");");
      }
    }

    public void MakeIndex(string aTable, string aCol, bool aUnique) {
      Exec("CREATE " + (aUnique ? "UNIQUE " : "") + "INDEX Idx_" + aTable + "_" + aCol + " ON " + aTable + "(" + aCol + ");");
    }

    public void MakeUniqueString(string aTable, string aCol, bool aAllowNulls) {
      string xIdxName = "Uq_" + aTable + "_" + aCol;
      if (aAllowNulls) {
        Exec("ALTER TABLE " + aTable + " ADD " + xIdxName + " AS (CASE WHEN " + aCol + " IS NULL THEN 'key-' + CAST([Id] as varchar(10)) ELSE UPPER(RTRIM(LTRIM(" + aCol + "))) END);");
        Exec("CREATE UNIQUE INDEX Idx" + xIdxName + " ON " + aTable + "(" + xIdxName + ");");
      } else {
        Exec("CREATE UNIQUE INDEX Idx" + xIdxName + " ON " + aTable + "(" + aCol + ");");
      }
    }    

    internal void CreateDB()
    {
      using (var strm = typeof(SQL).Assembly.GetManifestResourceStream(typeof(SQL), "SQLite.sql"))
      {
        if (strm == null)
        {
          throw new Exception("Sql resource not found!");
        }
        using (var reader = new StreamReader(strm))
        {
          Exec(reader.ReadToEnd());
        }
      }          
    }
  }
}

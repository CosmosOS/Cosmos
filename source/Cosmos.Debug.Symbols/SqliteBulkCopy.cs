using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace Cosmos.Debug.Symbols
{
  public class SqliteBulkCopy: IDisposable
  {
    private bool mDisposed = false;
    public void Dispose()
    {
      if (mDisposed)
      {
        return;
      }
      mDisposed = true;
      GC.SuppressFinalize(this);
    }

    private readonly SqliteConnection mConnection;
    public SqliteBulkCopy(SqliteConnection connection)
    {
      mConnection = connection;
    }

    public string DestinationTableName
    {
      get;
      set;
    }

    public void WriteToServer(IDataReader reader)
    {
      if (reader.Read())
      {
        // initialize bulk copy

        using (var trans = mConnection.BeginTransaction())
        {
          using (var command = mConnection.CreateCommand())
          {
            var fieldNames ="";
            var paramNames = "";
            SqliteParameter[] parms = new SqliteParameter[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
              fieldNames += "\"" + reader.GetName(i) + "\",";
              paramNames += "?,";
              parms[i] = new SqliteParameter();
              command.Parameters.Add(parms[i]);
            }
            fieldNames = fieldNames.TrimEnd(',');
            paramNames = paramNames.TrimEnd(',');

            command.Transaction = trans;
            command.CommandText = String.Format("insert into \"{0}\" ({1}) values ({2})", DestinationTableName, fieldNames, paramNames);
            command.Prepare();
            do
            {
              for (int i = 0; i < reader.FieldCount; i++)
              {
                var parm=parms[i];
                if (parm != null)
                {
                  parm.Value = reader.GetValue(i);
                }
              }
              command.ExecuteNonQuery();
            } while (reader.Read());
          }
          trans.Commit();
        }
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.Common;
using System.Data.SQLite;

namespace Cosmos.Debug.Common
{

    /// <summary>
    /// Class to provide an IDbConnectionFactory for SQLite.
    /// </summary>
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            var xResult = new SQLiteConnection();
            xResult.ConnectionString = nameOrConnectionString;
            return xResult;
        }
    }
}

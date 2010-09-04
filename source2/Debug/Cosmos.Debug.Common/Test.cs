using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Common {
    public static class Test {
        static public void Execute() {
            // http://www.firebirdsql.org/dotnetfirebird/create-a-new-database-from-an-sql-script.html
            var xCSB = new FbConnectionStringBuilder();
            xCSB.ServerType = FbServerType.Embedded;
            xCSB.Database = @"m:\temp\Cosmos.cpdb";

            FbConnection.CreateDatabase(xCSB.ToString());

            using (var xConn = new FbConnection(xCSB.ToString())) {
                var xExec = new FbBatchExecution(xConn);
                //foreach (string cmd in script.Results) {
                //    xExec.SqlStatements.Add(cmd);
                //}
                //fbe.Execute();
            }
        }
    }
}

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Common {
    public static class Test {
        static public void Execute() {
            System.IO.File.Delete(@"m:\temp\Cosmos.cpdb");

            // http://www.firebirdsql.org/dotnetfirebird/create-a-new-database-from-an-sql-script.html
            var xCSB = new FbConnectionStringBuilder();
            xCSB.ServerType = FbServerType.Embedded;
            xCSB.Database = @"m:\temp\Cosmos.cpdb";
            xCSB.UserID = "sysdba";
            xCSB.Password = "masterkey";

            FbConnection.CreateDatabase(xCSB.ToString());

            using (var xConn = new FbConnection(xCSB.ToString())) {
                var xExec = new FbBatchExecution(xConn);

                xExec.SqlStatements.Add(
                    "CREATE TABLE SYMBOL ("
                    + "   LABELNAME   VARCHAR(255)  NOT NULL"
                    + " , ADDRESS     BIGINT        NOT NULL"
                    + " , STACKDIFF   INT           NOT NULL"
                    + " , ILASMFILE   VARCHAR(255)  NOT NULL"
                    + " , TYPETOKEN   INT           NOT NULL"
                    + " , METHODTOKEN INT           NOT NULL"
                    + " , ILOFFSET    INT           NOT NULL"
                    + " , METHODNAME  VARCHAR(255)  NOT NULL"
                    + ");"
                );
                
                xExec.Execute();
            }
        }
    }
}

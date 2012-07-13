using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Cosmos.Debug.Common
{
    public delegate void LogTimeDelegate(string message);

    public class DebugSymbol
    {
        public string AssemblyFileName
        {
            get;
            set;
        }

        public int MethodMetaDataToken
        {
            get;
            set;
        }

        public int InstructionOffset
        {
            get;
            set;
        }

        public string LabelName
        {
            get;
            set;
        }
    }

    public class MLDebugSymbol_Old
    {
        public static FbConnection OpenOrCreateCPDB(string aPathName)
        {
            if (File.Exists(aPathName))
            {
                return OpenCPDB(aPathName, false);
            }
            else
            {
                return CreateCPDB(aPathName);
            }
        }

        private static FbConnection OpenCPDB(string aPathname, bool aCreate)
        {
            var xCSB = new FbConnectionStringBuilder();
            xCSB.ServerType = FbServerType.Embedded;
            xCSB.Database = aPathname;
            xCSB.UserID = "sysdba";
            xCSB.Password = "masterkey";
            xCSB.Pooling = false;

            // Ugh - The curr dir is the actual .cosmos dir. But we dont want to
            // copy the FB Embedded DLLs everywhere, and we don't want them in system
            // or path as they might conflict with other apps.
            // However the FB .NET provider doesnt let us set the path, so we hack it
            // by changing the current dir right before the first load (create or open).
            // We set it back after.
            string xCurrDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Cosmos.Build.Common.CosmosPaths.Vsip);

            if (aCreate)
            {
                File.Delete(aPathname);
                FbConnection.CreateDatabase(xCSB.ToString(), 16384, false, true); // Specifying false to forcedwrites will improve database speed.
            }

            FbConnection DBConn = new FbConnection(xCSB.ToString());
            DBConn.Open();

            // Set the current directory back to the original
            Directory.SetCurrentDirectory(xCurrDir);

            return DBConn;
        }

        private static FbConnection CreateCPDB(string aPathname)
        {
            FbConnection DBConn = OpenCPDB(aPathname, true);
            var xExec = new FbBatchExecution(DBConn);

            xExec.SqlStatements.Add(
                "CREATE TABLE MLSYMBOL ("
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
            
            xExec.SqlStatements.Add(
                "CREATE TABLE ADDRESSLABELMAPPING ("
                + "  LABELNAME VARCHAR(255) NOT NULL"
                + ", ADDRESS   BIGINT NOT NULL"
                + ");");

            xExec.Execute();
            // Batch execution closes the connection, so we have to reopen it
            DBConn.Open();

            return DBConn;
        }

        public string LabelName
        {
            get;
            set;
        }

        public uint Address
        {
            get;
            set;
        }

        public int StackDifference
        {
            get;
            set;
        }

        public string AssemblyFile
        {
            get;
            set;
        }
        public int TypeToken
        {
            get;
            set;
        }
        public int MethodToken
        {
            get;
            set;
        }
        public int ILOffset
        {
            get;
            set;
        }

        public string MethodName
        {
            get;
            set;
        }
    }
}

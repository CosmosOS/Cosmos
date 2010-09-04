using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Cosmos.Debug.Common.CDebugger
{
	public class DebugSymbol {
		public string AssemblyFileName {
			get;
			set;
		}

		public int MethodMetaDataToken {
			get;
			set;
		}

		public int InstructionOffset {
			get;
			set;
		}

		public string LabelName {
			get;
			set;
		}
	}

	public class MLDebugSymbol {

        protected static void CreateCPDB(string aPathname) {
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

		public static void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols, string aFile) {
            CreateCPDB(Path.ChangeExtension(aFile, ".cpdb"));

            var xDS = new SymbolsDS();
            // This is a dataset and XML currently.
            // Later change it to a real DB as we don't need it in RAM all at once but we need it to be fast.
            // Probably embedded Firebird
            foreach(var xItem in aSymbols){
                var x = xDS.Entry.NewEntryRow();
                x.LabelName = xItem.LabelName;
                x.Address = xItem.Address;
                x.StackDiff = xItem.StackDifference;
                x.ILAsmFile = xItem.AssemblyFile;
                x.TypeToken = xItem.TypeToken;
                x.MethodToken = xItem.MethodToken;
                x.ILOffset = xItem.ILOffset;
                x.MethodName = xItem.MethodName;
                xDS.Entry.AddEntryRow(x);
            }
            xDS.WriteXml(aFile);
		}

		public static void ReadSymbolsListFromFile(List<MLDebugSymbol> aSymbols, string aFile) {
            var xDS = new SymbolsDS();
            xDS.ReadXml(aFile);
            foreach (SymbolsDS.EntryRow x in xDS.Entry.Rows) {
                aSymbols.Add(new MLDebugSymbol {
                    LabelName = x.LabelName,
                    Address = x.Address,
                    StackDifference = x.StackDiff,
                    AssemblyFile = x.ILAsmFile,
                    TypeToken = x.TypeToken,
                    MethodToken = x.MethodToken,
                    ILOffset = x.ILOffset,
                    MethodName = x.MethodName
                });
            }
		}

		public string LabelName {
			get;
			set;
		}

		public uint Address {
			get;
			set;
		}

		public int StackDifference {
			get;
			set;
		}

		public string AssemblyFile {
			get;
			set;
		}
		public int TypeToken {
			get;
			set;
		}
		public int MethodToken {
			get;
			set;
		}
		public int ILOffset {
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
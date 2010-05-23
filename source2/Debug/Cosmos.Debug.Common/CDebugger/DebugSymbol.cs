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
		public static void WriteSymbolsListToFile(IEnumerable<MLDebugSymbol> aSymbols, string aFile) {
            using (var xWriter = new BinaryWriter(new FileStream(aFile, FileMode.Create), Encoding.ASCII))
            {
                foreach(var xItem in aSymbols){
                    xWriter.Write(xItem.LabelName);
                    xWriter.Write(xItem.Address);
                    xWriter.Write(xItem.StackDifference);
                    xWriter.Write(xItem.AssemblyFile);
                    xWriter.Write(xItem.TypeToken);
                    xWriter.Write(xItem.MethodToken);
                    xWriter.Write(xItem.ILOffset);
                    xWriter.Write(xItem.MethodName);
                }
            }
		}

		public static void ReadSymbolsListFromFile(List<MLDebugSymbol> aSymbols, string aFile) {
            using (var xReader = new BinaryReader(new FileStream(aFile, FileMode.Open), Encoding.ASCII))
            {
                while (xReader.BaseStream.Position < xReader.BaseStream.Length)
                {
                    aSymbols.Add(new MLDebugSymbol
                    {
                        LabelName = xReader.ReadString(),
                        Address = xReader.ReadUInt32(),
                        StackDifference=xReader.ReadInt32(),
                        AssemblyFile = xReader.ReadString(),
                        TypeToken = xReader.ReadInt32(),
                        MethodToken = xReader.ReadInt32(),
                        ILOffset = xReader.ReadInt32(),
                        MethodName = xReader.ReadString()
                    });
                }
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
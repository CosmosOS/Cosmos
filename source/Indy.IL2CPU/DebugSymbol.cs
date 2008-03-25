using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Indy.IL2CPU {
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
			using (XmlWriter xWriter = XmlWriter.Create(aFile)) {
				xWriter.WriteStartDocument();
				{
					xWriter.WriteStartElement("MSILInstructions");
					{
						foreach (var item in aSymbols) {
							xWriter.WriteStartElement("Instruction");
							{
								xWriter.WriteAttributeString("Label", item.LabelName);
								xWriter.WriteAttributeString("Address", item.Address.ToString());
								xWriter.WriteAttributeString("StackDifference", item.StackDifference.ToString());
								xWriter.WriteAttributeString("AssemblyFile", item.AssemblyFile);
								xWriter.WriteAttributeString("TypeToken", item.TypeToken.ToString());
								xWriter.WriteAttributeString("MethodToken", item.MethodToken.ToString());
								xWriter.WriteAttributeString("ILOffset", item.ILOffset.ToString());
							}
							xWriter.WriteEndElement();
						}
					}
					xWriter.WriteEndElement();
				}
				xWriter.WriteEndDocument();
			}
		}

		public static void ReadSymbolsListFromFile(List<MLDebugSymbol> aSymbols, string aFile) {
			var xDoc = XDocument.Load(aFile);
			aSymbols.Clear();
			aSymbols.AddRange((from item in xDoc.Descendants("Instruction")
							   select new MLDebugSymbol() {
								   LabelName = item.Attribute("Label").Value,
								   Address = (uint)item.Attribute("Address"),
								   StackDifference = (int)item.Attribute("StackDifference"),
								   AssemblyFile = (string)item.Attribute("AssemblyFile"),
								   TypeToken = (int)item.Attribute("TypeToken"),
								   MethodToken = (int)item.Attribute("MethodToken"),
								   ILOffset = (int)item.Attribute("ILOffset"),
							   }));
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
	}
}
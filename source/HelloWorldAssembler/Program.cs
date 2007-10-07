using System;
using System.IO;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Assembler=Indy.IL2CPU.Assembler.X86.Win32.Assembler;

namespace HelloWorldAssembler {
	public class Program {
		public static void Main(string[] args) {
//			using (StreamWriter xSW = new StreamWriter("out.asm")) {
//				using (Indy.IL2CPU.Assembler.X86.Assembler a = new Win32.Assembler(xSW)) {
//					a.OutputType = Assembler.OutputTypeEnum.GUI;
//					a.Includes.Add("win32w.inc");
//					a.DataMembers.Add(new DataMember("_class", "TCHAR", "'FASMWIN32',0"));
//					a.DataMembers.Add(new DataMember("_title", "TCHAR", "'Win32 program template',0"));
//					a.DataMembers.Add(new DataMember("_error", "TCHAR", "'Startup failed.',0"));
//					a.DataMembers.Add(new DataMember("wc", "WNDCLASS", "0,WindowProc,0,0,NULL,NULL,NULL,COLOR_BTNFACE+1,NULL,_class"));
//					a.DataMembers.Add(new DataMember("msg", "MSG", ""));
//					DummyOp xDO = new DummyOp(null, null, null);
//					xDO.Assembler = a;
//					xDO.Assemble();
//					//					a.ImportMembers.Add(new ImportMember("library kernel32,'KERNEL32.DLL',\\"));
//					//					a.ImportMembers.Add(new ImportMember("\tuser32,'USER32.DLL'"));
//					//					a.ImportMembers.Add(new ImportMember("include 'api\\kernel32.inc'"));
//					//					a.ImportMembers.Add(new ImportMember("include 'api\\user32.inc'"));
//					a.Flush();
//				}
//			}
			Console.WriteLine("[Not Implemented] Done.");
			Console.ReadLine();
		}
	}
}
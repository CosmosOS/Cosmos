using System;
using System.IO;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;

namespace HelloWorldAssembler {
	public class Program {
		public static void Main(string[] args) {
			using (StreamWriter xSW = new StreamWriter("out.asm")) {
				using (Assembler a = new Assembler(xSW)) {
					a.OutputType = Assembler.OutputTypeEnum.Console;
					a.Includes.Add("win32w.inc");
					a.DataMembers.Add(new DataMember("_class", "TCHAR", "'FASMWIN32'"));
					a.DataMembers.Add(new DataMember("_title", "TCHAR", "'Win32 program template'"));
					a.DataMembers.Add(new DataMember("_error", "TCHAR", "'Startup failed.'"));
					a.DataMembers.Add(new DataMember("wc", "WNDCLASS", "0,WindowProc,0,0,NULL,NULL,NULL,COLOR_BTNFACE+1,NULL,_class"));
					new Invoke("GetModuleHandle", 0);
					new Move("[wc.hInstance]", "eax");
					new Invoke("LoadIcon", 0, "IDI_APPLICATION");
					new Move("[wc.hIcon]", "eax");
					new Invoke("LoadCursor", 0, "IDC_ARROW");
					new Move("[wc.hCursor]", "eax");
					new Invoke("RegisterClass", "wc");
					new Test("eax", "eax");
					new JumpIfZero("error");
					new Invoke("CreateWindowEx", 0, "_class", "_title", "WS_VISIBLE+WS_DLGFRAME+WS_SYSMENU", 128, 128, 256, 192, "NULL", "NULL", "[wc.hInstance]", "NULL");
					new Test("eax", "eax");
					new JumpIfZero("error");
					new Label("msg_loop");
					new Invoke("GetMessage", "msg", "NULL", 0, 0);
					new Compare("eax", "1");

					a.Flush();
				}
			}
			Console.WriteLine("Done.");
			Console.ReadLine();
		}
	}
}
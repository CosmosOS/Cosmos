using System;
using System.IO;
using System.Linq;
using Indy.IL2CPU.Assembler;
using X86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86;

namespace HelloWorldAssembler {
	public class DummyOp: Indy.IL2CPU.IL.X86.Op {
		public override void Assemble(Mono.Cecil.Cil.Instruction aInstruction) {
			Invoke("GetModuleHandle", 0);
			Move("[wc.hInstance]", "eax");
			Invoke("LoadIcon", 0, "IDI_APPLICATION");
			Move("[wc.hIcon]", "eax");
			Invoke("LoadCursor", 0, "IDC_ARROW");
			Move("[wc.hCursor]", "eax");
			Invoke("RegisterClass", "wc");
			Test("eax", "eax");
			JumpIfZero("error");
			Invoke("CreateWindowEx", 0, "_class", "_title", "WS_VISIBLE+WS_DLGFRAME+WS_SYSMENU", 128, 128, 256, 192, "NULL", "NULL", "[wc.hInstance]", "NULL");
			Test("eax", "eax");
			JumpIfZero("error");
			Label("msg_loop");
			Invoke("GetMessage", "msg", "NULL", 0, 0);
			Compare("eax", "1");
			JumpIfGreater("end_loop");
			JumpIfNotZero("msg_loop");
			Invoke("TranslateMessage", "msg");
			Invoke("DispatchMessage", "msg");
			JumpAlways("msg_loop");
			Label("error");
			Invoke("MessageBox", "NULL", "_error", "NULL", "MB_ICONERROR+MB_OK");
			Label("end_loop");
			Invoke("ExitProcess", "[msg.wParam]");
			Literal("proc WindowProc hwnd,wmsg,wparam,lparam");
			Push("ebx", "esi", "edi");
			Compare("[wmsg]", "WM_DESTROY");
			JumpIfEquals(".wmdestroy");
			Label(".defwndproc");
			Invoke("DefWindowProc", "[hwnd]", "[wmsg]", "[wparam]", "[lparam]");
			JumpAlways(".finish");
			Label(".wmdestroy");
			Invoke("PostQuitMessage", "0");
			Xor("eax", "eax");
			Label(".finish");
			Pop("edi", "esi", "ebx");
			Ret();
			Literal("endp");
		}
	}
	public class Program {
		public static void Main(string[] args) {
			using (StreamWriter xSW = new StreamWriter("out.asm")) {
				using (X86.Assembler a = new X86.Assembler(xSW)) {
					a.OutputType = Indy.IL2CPU.Assembler.Assembler.OutputTypeEnum.GUI;
					a.Includes.Add("win32w.inc");
					a.DataMembers.Add(new DataMember("_class", "TCHAR", "'FASMWIN32',0"));
					a.DataMembers.Add(new DataMember("_title", "TCHAR", "'Win32 program template',0"));
					a.DataMembers.Add(new DataMember("_error", "TCHAR", "'Startup failed.',0"));
					a.DataMembers.Add(new DataMember("wc", "WNDCLASS", "0,WindowProc,0,0,NULL,NULL,NULL,COLOR_BTNFACE+1,NULL,_class"));
					a.DataMembers.Add(new DataMember("msg", "MSG", ""));
					DummyOp xDO = new DummyOp();
					xDO.Assembler = a;
					xDO.Assemble(null);
					a.ImportMembers.Add(new ImportMember("library kernel32,'KERNEL32.DLL',\\"));
					a.ImportMembers.Add(new ImportMember("\tuser32,'USER32.DLL'"));
					a.ImportMembers.Add(new ImportMember("include 'api\\kernel32.inc'"));
					a.ImportMembers.Add(new ImportMember("include 'api\\user32.inc'"));
					a.Flush();
				}
			}
			Console.WriteLine("[Not Implemented] Done.");
			Console.ReadLine();
		}
	}
}
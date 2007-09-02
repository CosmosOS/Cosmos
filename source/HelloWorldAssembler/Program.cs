using System;
using System.IO;
using System.Linq;
using Indy.IL2CPU.Assembler;
using X86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86;

namespace HelloWorldAssembler {
	public class Program {
		public static void Main(string[] args) {
//			using (StreamWriter xSW = new StreamWriter("out.asm")) {
//				using (X86.Assembler a = new X86.Assembler(xSW)) {
//                    a.OutputType = Indy.IL2CPU.Assembler.Assembler.OutputTypeEnum.GUI;
//					a.Includes.Add("win32w.inc");
//                    a.DataMembers.Add(new DataMember("_class", "TCHAR", "'FASMWIN32',0"));
//                    a.DataMembers.Add(new DataMember("_title", "TCHAR", "'Win32 program template',0"));
//                    a.DataMembers.Add(new DataMember("_error", "TCHAR", "'Startup failed.',0"));
//                    a.DataMembers.Add(new DataMember("wc", "WNDCLASS", "0,WindowProc,0,0,NULL,NULL,NULL,COLOR_BTNFACE+1,NULL,_class"));
//                    a.DataMembers.Add(new DataMember("msg", "MSG", ""));
//					new Invoke("GetModuleHandle", 0);
//					new Move("[wc.hInstance]", "eax");
//                    new Invoke("LoadIcon", 0, "IDI_APPLICATION");
//                    new Move("[wc.hIcon]", "eax");
//                    new Invoke("LoadCursor", 0, "IDC_ARROW");
//                    new Move("[wc.hCursor]", "eax");
//                    new Invoke("RegisterClass", "wc");
//                    new Test("eax", "eax");
//                    new JumpIfZero("error");
//                    new Invoke("CreateWindowEx", 0, "_class", "_title", "WS_VISIBLE+WS_DLGFRAME+WS_SYSMENU", 128, 128, 256, 192, "NULL", "NULL", "[wc.hInstance]", "NULL");
//                    new Test("eax", "eax");
//                    new JumpIfZero("error");
//                    new Label("msg_loop");
//                    new Invoke("GetMessage", "msg", "NULL", 0, 0);
//                    new Compare("eax", "1");
//                    new JumpIfGreater("end_loop");
//                    new JumpIfNotZero("msg_loop");
//                    new Invoke("TranslateMessage", "msg");
//                    new Invoke("DispatchMessage", "msg");
//                    new JumpAlways("msg_loop");
//                    new Label("error");
//                    new Invoke("MessageBox", "NULL", "_error", "NULL", "MB_ICONERROR+MB_OK");
//                    new Label("end_loop");
//                    new Invoke("ExitProcess", "[msg.wParam]");
//                    new Literal("proc WindowProc hwnd,wmsg,wparam,lparam");
//                    new Push("ebx", "esi", "edi");
//                    new Compare("[wmsg]", "WM_DESTROY");
//                    new JumpIfEquals(".wmdestroy");
//                    new Label(".defwndproc");
//                    new Invoke("DefWindowProc", "[hwnd]", "[wmsg]", "[wparam]", "[lparam]");
//                    new JumpAlways(".finish");
//                    new Label(".wmdestroy");
//                    new Invoke("PostQuitMessage", "0");
//                    new Xor("eax", "eax");
//                    new Label(".finish");
//                    new Pop("edi", "esi", "ebx");
//                    new Ret();
//                    new Literal("endp");
//					a.ImportMembers.Add(new ImportMember("library kernel32,'KERNEL32.DLL',\\"));
//                    a.ImportMembers.Add(new ImportMember("\tuser32,'USER32.DLL'"));
//                    a.ImportMembers.Add(new ImportMember("include 'api\\kernel32.inc'"));
//                    a.ImportMembers.Add(new ImportMember("include 'api\\user32.inc'"));
//					a.Flush();
//				}
//			}
#warning Not implemented
			Console.WriteLine("[Not Implemented] Done.");
			Console.ReadLine();
		}
	}
}
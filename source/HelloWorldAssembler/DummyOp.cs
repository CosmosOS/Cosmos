using System;
using System.Linq;
using Indy.IL2CPU.IL;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace HelloWorldAssembler {
	public class DummyOp: Indy.IL2CPU.IL.X86.Op {
		public override void Assemble(Instruction aInstruction, MethodInformation aMethodInfo) {
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
}
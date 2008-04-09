using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lost.JIT.AMD64;
using System.IO;
using System.Diagnostics;
//using Cosmos.Build.Windows;

namespace Lost
{
    class LostTest
    {
        [STAThread]
        static void Main(string[] args)
        {
			//using (var source = new StreamReader(args[0]))
			//{
			//    string srcLine;
			//    while ((srcLine = source.ReadLine()) != null)
			//    {
			//        var op = ProcessorInstruction.Parse(srcLine);
			//        Console.WriteLine(op);
			//    }
			//}
			CompilationTest();

			if (File.Exists(tempFile)) File.Delete(tempFile);
			Console.ReadKey();
        }

		static string RunFasm(string filename, string dest)
		{
			const string fasm = @"C:\Programs\Devel\asm\fasmw16726\fasm.exe";

			var par = new ProcessStartInfo(fasm, string.Format("\"{0}\" \"{1}\"", filename, dest));

			par.CreateNoWindow = true;
			par.RedirectStandardOutput = true;
			par.RedirectStandardError = true;
			par.UseShellExecute = false;

			string result;

			using (var process = Process.Start(par))
			using(var output = process.StandardOutput)
			using(var errorStream =process.StandardError)
			{
				result = output.ReadToEnd();
				var error = errorStream.ReadToEnd().Trim();
				process.WaitForExit();
				if (!string.IsNullOrEmpty(error))
					throw new InvalidProgramException(error);
			}
			return result;
		}

		static readonly string tempFile = Path.GetTempFileName();

		static void CompilationTest()
		{
			#region ALU
			#region ADC
			Test("adc	al, 1", new AddWithCarry(GeneralPurposeRegister.AL, (byte)1));
			Test("adc	rcx, 1", new AddWithCarry(GeneralPurposeRegister.RCX, (byte)1));
			Test("adc	rcx, 0xFFF", new AddWithCarry(GeneralPurposeRegister.RCX, 0xFFF));
			Test("adc	[rip + 1], rax",
				new AddWithCarry(new MemoryOperand()
					{
						RipBased = true,
						Displacement = 1,
					}, GeneralPurposeRegister.RAX));
			Test("adc	[rax], rax", new AddWithCarry(new MemoryOperand() {
				Base = GeneralPurposeRegister.RAX,
			}, GeneralPurposeRegister.RAX));

			Test("adc	[rax + 3], r11",
				new AddWithCarry(new MemoryOperand() {
					Displacement = 3,
					Base = GeneralPurposeRegister.RAX,
				}, GeneralPurposeRegister.R11));

			Test("adc	[rsp + 0xFFF], eax",
				new AddWithCarry(new MemoryOperand() {
					Displacement = 0xFFF,
					Base = GeneralPurposeRegister.SP,
				}, GeneralPurposeRegister.EAX));

			Test("adc	al, [rax*2 + r11]",
				new AddWithCarry(GeneralPurposeRegister.AL,
					new MemoryOperand() {
						Base = GeneralPurposeRegister.R11,
						Index = GeneralPurposeRegister.RAX,
						Scale = 2,
					}));
			#endregion
			#endregion

			#region Stack
			#region POP
			Test("pop	rax", new Pop(GeneralPurposeRegister.RAX));
			Test("pop	r12", new Pop(GeneralPurposeRegister.R12));

			Test("pop	qword [rax]", new Pop(new MemoryOperand() {
				Base = GeneralPurposeRegister.RAX,
			}));
			Test("pop	qword [rip + 1]", new Pop(new MemoryOperand() {
				Displacement = 1,
				RipBased = true,
			}));
			Test("pop	qword [rax*2 + r11]",
				new Pop(
					new MemoryOperand() {
						Base = GeneralPurposeRegister.R11,
						Index = GeneralPurposeRegister.RAX,
						Scale = 2,
					}));
			Test("pop	qword [rsp + 0xFFF]",
				new Pop(
					new MemoryOperand() {
						Displacement = 0xFFF,
						Base = GeneralPurposeRegister.SP,
					}));
			Test("pop	qword [r12*8 + r11]",
				new Pop(
					new MemoryOperand() {
						Base = GeneralPurposeRegister.R11,
						Index = GeneralPurposeRegister.R12,
						Scale = 8,
					}));
			#endregion

			#region PUSH
			Test("push	1", new Push(1));
			Test("push	qword [r12*8 + r11]",
				new Push(
					new MemoryOperand() {
						Base = GeneralPurposeRegister.R11,
						Index = GeneralPurposeRegister.R12,
						Scale = 8,
					}));
			#endregion
			#endregion
		}

		static void Test(string fasm_code, ProcessorInstruction my_code)
		{
		    using (var writer = new StreamWriter(tempFile))
			{
				writer.WriteLine("use64");
				writer.WriteLine(fasm_code);
				writer.Close();
			}
			var asm_out = RunFasm(tempFile, "fasm.out");

			byte[] fasm_out;
			using (var stream = new FileStream("fasm.out", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				fasm_out = new byte[stream.Length];
				stream.Read(fasm_out, 0, fasm_out.Length);
			}

			byte[] my_out;
			using (var stream = new MemoryStream())
			{
				my_code.Compile(stream);
				my_out = stream.ToArray();
			}


			if (fasm_out.Length != my_out.Length) throw new InvalidProgramException();
			for (int i = 0; i < fasm_out.Length; i++)
				if (fasm_out[i] != my_out[i]) throw new InvalidProgramException(i.ToString());
			Console.WriteLine("{0}: passed", fasm_code);
		}
	}
}

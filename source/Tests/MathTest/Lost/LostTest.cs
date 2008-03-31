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
			par.UseShellExecute = false;

			string result;

			using (var process = Process.Start(par))
			using(var output = process.StandardOutput)
			{
				result = output.ReadToEnd();
				process.WaitForExit();
			}
			return result;
		}

		static readonly string tempFile = Path.GetTempFileName();

		static void CompilationTest()
		{
			Test("adc	al, 1\nadc	rcx, 1\nadc	rcx, 0xFFF\n",
				new List<ProcessorInstruction>(){
					new AddWithCarry(new GeneralPurposeRegister(Registers.AX, 1), new ImmediateOperand((byte)1)),
					new AddWithCarry(new GeneralPurposeRegister(Registers.CX, 8), new ImmediateOperand((byte)1)),
					new AddWithCarry(new GeneralPurposeRegister(Registers.CX, 8), new ImmediateOperand(0xFFF)),});
		}

		static void Test(string fasm_code, IEnumerable<ProcessorInstruction> my_code)
		{
		    using (var writer = new StreamWriter(tempFile))
			{
				writer.WriteLine("use64");
				writer.WriteLine(fasm_code);
				writer.Close();
			}
			RunFasm(tempFile, "fasm.out");

			byte[] fasm_out;
			using (var stream = new FileStream("fasm.out", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				fasm_out = new byte[stream.Length];
				stream.Read(fasm_out, 0, fasm_out.Length);
			}

			byte[] my_out;
			using (var stream = new MemoryStream())
			{
				foreach(var instr in my_code) instr.Compile(stream);
				my_out = stream.ToArray();
			}


			if (fasm_out.Length != my_out.Length) throw new InvalidProgramException();
			for (int i = 0; i < fasm_out.Length; i++)
				if (fasm_out[i] != my_out[i]) throw new InvalidProgramException(i.ToString());
			Console.WriteLine("{0}: passed", fasm_code);
		}
	}
}

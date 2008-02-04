using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands {
	public class MatthijsCommand: CommandBase {
		public override string Name {
			get {
				return "matthijs";
			}
		}

		public override string Summary {
			get {
				return "Executes tests Matthijs is working on.";
			}
		}

		public override void Execute(string param) {
			//Kernel.FileSystem.TestsMatthijs.TestNewATA();
			//System.Diagnostics.Debugger.Break();
			//try {
			//    try {
			//        if (param != null && param.Length == 0) {
			//            throw new ArgumentException("ArgumentError!", "paramname");
			//        } else {
			//            throw new Exception("Generic Error!");
			//        }
			//    } finally {
			//        System.Console.WriteLine("Finally Clause Called!");
			//    }
			//} catch (ArgumentException AE) {
			//    System.Console.WriteLine("Argument Exception Occurred:");
			//    System.Console.Write("  Param Name: ");
			//    System.Console.WriteLine(AE.ParamName);
			//} catch (Exception E) {
			//    System.Console.WriteLine("Error Occurred while executing Command!");
			//    System.Console.Write("Details: ");
			//    System.Console.WriteLine(E.Message);
			//}
		}

		public override void Help() {
			System.Console.WriteLine(Summary);
		}
	}
}

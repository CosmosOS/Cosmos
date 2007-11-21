using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Client.CLI {
	class Program {
		static void Main(string[] args) {
			string xBasePath = Path.Combine(Path.GetDirectoryName(typeof (Program).Assembly.Location), @"..\..\..\..\Build\Cosmos\ISO\");
			ProcessStartInfo xProcessInfo = new ProcessStartInfo();
			xProcessInfo.FileName = Path.Combine(xBasePath, "Build.bat");
			xProcessInfo.CreateNoWindow = true;
			xProcessInfo.WorkingDirectory = xBasePath;
			Process.Start(xProcessInfo).WaitForExit();
		}
	}
}

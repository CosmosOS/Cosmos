using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.Diagnostics;

namespace Cosmos.Build.MSBuild
{
	public abstract class BaseToolTask : Task
	{
		protected bool ExecuteTool(string workingDir, string filename, string arguments, string name)
		{
			var xProcessStartInfo = new ProcessStartInfo();
			xProcessStartInfo.WorkingDirectory = workingDir;
			xProcessStartInfo.FileName = filename;
			xProcessStartInfo.Arguments = arguments;
			xProcessStartInfo.UseShellExecute = false;
			xProcessStartInfo.RedirectStandardOutput = true;
			xProcessStartInfo.RedirectStandardError = true;
			xProcessStartInfo.CreateNoWindow = true;
			using (var xProcess = new Process())
			{
				xProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
				{
					if (e.Data != null)
						mErrors.Add(e.Data);
				};
				xProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
				{
					if (e.Data != null)
						mOutput.Add(e.Data);
				};
				xProcess.StartInfo = xProcessStartInfo;
				mErrors = new List<string>();
				mOutput = new List<string>();
				xProcess.Start();
				xProcess.BeginErrorReadLine();
				xProcess.BeginOutputReadLine();
				xProcess.WaitForExit(15 * 60 * 1000); // wait 15 minutes
				if (xProcess.ExitCode != 0)
				{
					if (!xProcess.HasExited)
					{
						xProcess.Kill();
						Log.LogError("{0} timed out.", name);
					}
					else
					{
						Log.LogError("Error occurred while invoking {0}", name);
					}
					foreach (var xError in mErrors)
					{
						Log.LogError(xError);
					}
					foreach (var xOutput in mOutput)
					{
						Log.LogError(xOutput);
					}
					return false;
				}
				else
				{
					foreach (var xOutput in mOutput)
					{
						Log.LogMessage(xOutput);
					}
				}
			}
			return true;
		}

		private List<string> mErrors;
		private List<string> mOutput;
	}
}
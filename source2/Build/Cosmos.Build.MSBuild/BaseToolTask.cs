using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.Diagnostics;

namespace Cosmos.Build.MSBuild
{
	public enum WriteType
	{
		Warning,
		Error,
		Info
	}

	public abstract class BaseToolTask : AppDomainIsolatedTask
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
						Log.LogError("Error occurred while invoking {0}.", name);
					}
					
					return false;
				}
				WriteType typ;
				foreach (var xError in mErrors)
				{
					string error = xError;
					if(ExtendLineError(xProcess.ExitCode, ref error, out typ))
					{
						Logs(typ, error);
					}
				}
				foreach (var xOutput in mOutput)
				{
					string output = xOutput;
					if (ExtendLineError(xProcess.ExitCode, ref output, out typ))
					{
						Logs(typ, output);
					}
				}
			}
			return true;
		}

		private List<string> mErrors;
		private List<string> mOutput;

		public virtual bool ExtendLineError(int exitCode, ref string errorMessage, out WriteType typ)
		{
			typ = WriteType.Error;
			if (exitCode == 0)
				return false;
			return true;
		}

		public virtual bool ExtendLineOutput(int exitCode, ref string errorMessage, out WriteType typ)
		{
			typ = WriteType.Info;
			return true;
		}

		public void Logs(WriteType typ, string message)
		{
			//TODO remove
			Log.LogCommandLine(message);
			switch (typ)
			{
				case WriteType.Warning:
					Log.LogWarning(message);
					break;
				case WriteType.Error:
					Log.LogError(message);
					break;
				case WriteType.Info:
					Log.LogMessage(message);
					break;
				default:
					Log.LogError(message);
					break;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.Diagnostics;
using MessageImportance = Microsoft.Build.Framework.MessageImportance;
using Microsoft.Build.Framework;

namespace Cosmos.Build.MSBuild
{
	  public enum WriteType
	  {
		    Warning,
		    Error,
		    Message, // only issued on console
		    Info
	  }

    public class LogInfo
    {
        /// <summary>
        /// Specifies if warning, error, info or message.
        /// </summary>
        public WriteType logType;

        /// <summary>
        /// Description of the type (can be null).
        /// </summary>
        public string subcategory;

        /// <summary>
        /// Message, Warning or Error code (can be null)
        /// </summary>
        public string code;

        /// <summary>
        /// The help keyword for the host IDE (can be null).
        /// </summary>
        public string helpKeyword;

        /// <summary>
        /// The path to the file causing the message (can be null).
        /// </summary>
        public string file;

        /// <summary>
        /// The line in the file causing the message (set to zero if not available).
        /// </summary>
        public int lineNumber;

        /// <summary>
        /// The column in the file causing the message (set to zero if not available).
        /// </summary>
        public int columnNumber;

        /// <summary>
        /// The last line of a range of lines in the file causing the message (set to zero if not available).
        /// </summary>
        public int endLineNumber;

        /// <summary>
        /// The last column of a range of columns in the file causing the message (set to zero if not available).
        /// </summary>
        public int endColumnNumber;

        /// <summary>
        /// Importance of the message. (default is High)
        /// </summary>
        public MessageImportance importance;

        /// <summary>
        /// The message string.
        /// </summary>
        public string message;

        /// <summary>
        /// Optional arguments for formatting the message string.
        /// </summary>
        public object[] messageArgs;//TODO check if null is allowed, if yes document it here
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

			Log.LogCommandLine(string.Format("Executing command line \"{0}\" {1}", filename, arguments));
			Log.LogCommandLine(string.Format("Working directory = '{0}'", workingDir));

			using (var xProcess = new Process())
			{
        xProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
        {
          if (e.Data != null)
          {
            mErrors.Add(e.Data);
          }
        };
        xProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
        {
          if (e.Data != null)
          {
            mOutput.Add(e.Data);
          }
        };
				xProcess.StartInfo = xProcessStartInfo;
				mErrors = new List<string>();
				mOutput = new List<string>();
			  xProcess.Start();
        xProcess.BeginErrorReadLine();
        xProcess.BeginOutputReadLine();
        xProcess.WaitForExit(15 * 60 * 1000); // wait 15 minutes
			  if (!xProcess.HasExited) {
					xProcess.Kill();
					Log.LogError("{0} timed out.", name);
				}
				else {
					if (xProcess.ExitCode != 0)
					{
					  Log.LogError("Error occurred while invoking {0}.", name);
					}
				}

        LogInfo logContent;
			  for (int xIndex = 0; xIndex < mErrors.Count; xIndex++)
			  {
			    var xError = mErrors[xIndex];
			    if (ExtendLineError(xProcess.ExitCode, xError, out logContent))
			    {
			      Logs(logContent);
			    }
			  }

			  for (int xIndex = 0; xIndex < mOutput.Count; xIndex++)
			  {
			    var xOutput = mOutput[xIndex];
			    if (ExtendLineOutput(xProcess.ExitCode, xOutput, out logContent))
			    {
			      Logs(logContent);
			    }
			  }

			  return xProcess.ExitCode == 0;
			}
		}

		private List<string> mErrors;
		private List<string> mOutput;

	  public virtual bool ExtendLineError(int exitCode, string errorMessage, out LogInfo log)
	  {
	    log = new LogInfo();
	    log.logType = WriteType.Error;
	    log.message = errorMessage;
	    if (exitCode == 0)
	      return false;
	    return true;
	  }

	  public virtual bool ExtendLineOutput(int exitCode, string errorMessage, out LogInfo log)
	  {
	    log = new LogInfo();
	    log.logType = WriteType.Info;
	    log.message = errorMessage;
	    return true;
	  }

	  public void Logs(LogInfo logInfo)// string message, string category, string filename, string lineNumber = 0, string columnNumber = 0)
		{
            switch (logInfo.logType)
			{
				case WriteType.Warning:
                    //Log.LogWarning(category, string.Empty, string.Empty, filename, lineNumber, columnNumber, lineNumber, columnNumber, message);
                    Log.LogWarning(logInfo.subcategory, logInfo.code, logInfo.helpKeyword, logInfo.file, logInfo.lineNumber, logInfo.columnNumber, logInfo.endLineNumber, logInfo.endColumnNumber, logInfo.message, logInfo.messageArgs);
					break;
				case WriteType.Message:
                    Log.LogMessage(logInfo.subcategory, logInfo.code, logInfo.helpKeyword, logInfo.file, logInfo.lineNumber, logInfo.columnNumber, logInfo.endLineNumber, logInfo.endColumnNumber, logInfo.message, logInfo.messageArgs);
					break;
				case WriteType.Info:
					// changed IDEBuildLogger.cs for this behavior of add to ErrorList Messages
                    Log.LogMessage(logInfo.subcategory, logInfo.code, logInfo.helpKeyword, logInfo.file, logInfo.lineNumber, logInfo.columnNumber, logInfo.endLineNumber, logInfo.endColumnNumber, logInfo.importance, logInfo.message, logInfo.messageArgs);
					break;
                case WriteType.Error:
				default:
			        if (UseConsoleForLog)
			        {
			            LogError(logInfo.message, logInfo.messageArgs);
			        }
			        else
			        {
			            Log.LogError(logInfo.subcategory, logInfo.code, logInfo.helpKeyword, logInfo.file, logInfo.lineNumber, logInfo.columnNumber, logInfo.endLineNumber, logInfo.endColumnNumber, logInfo.message??"", logInfo.messageArgs);
			        }
			        break;
			}
		}

        protected void LogError(string message, params object[] args)
        {
            if (UseConsoleForLog)
            {
                if (message == null)
                {
                    return;
                }
                Console.WriteLine("Error: " + String.Format(message, args));
            }
            else
            {
                Log.LogError(message, args);
            }
        }

    public bool UseConsoleForLog { get; set; }
	}
}

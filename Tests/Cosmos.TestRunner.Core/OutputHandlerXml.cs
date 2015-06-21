using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Cosmos.TestRunner.Core
{
    public partial class OutputHandlerXml: OutputHandlerBase
    {
        private readonly string mFilename;
        private XmlDocument mDocument;

        public OutputHandlerXml(string filename)
        {
            mFilename = filename;
        }

        public override void ExecuteKernelStart(string assemblyName)
        {
            var xParent = mCurrentNode.Peek();
            var xItem = mDocument.CreateElement("Kernel");
            xItem.Attributes.Append(NewXmlAttribute("AssemblyName", assemblyName));
            xParent.AppendChild(xItem);
            mCurrentNode.Push(xItem);
            mKernelStopwatch = Stopwatch.StartNew();
        }

        private Stopwatch mKernelStopwatch;

        public override void ExecuteKernelEnd(string assemblyName)
        {
            mKernelStopwatch.Stop();
            var xItem = mCurrentNode.Pop();
            xItem.Attributes.Append(NewXmlAttribute("Duration", mKernelStopwatch.Elapsed.ToString("c")));
        }

        public override void LogMessage(string message)
        {
            var xParent = mCurrentNode.Peek();
            var xItem = mDocument.CreateElement("Message");
            xItem.AppendChild(mDocument.CreateCDataSection(message));
            xParent.AppendChild(xItem);
        }

        public override void LogError(string message)
        {
            var xParent = mCurrentNode.Peek();
            var xItem = mDocument.CreateElement("Error");
            xItem.AppendChild(mDocument.CreateCDataSection(message));
            xParent.AppendChild(xItem);
        }

        public override void ExecutionStart()
        {
            mDocument = new XmlDocument();
            mDocument.LoadXml("<Execution/>");
            mDocument.DocumentElement.Attributes.Append(NewXmlAttribute("DateTime", DateTime.UtcNow.ToString("O")));
            mCurrentNode.Push(mDocument.DocumentElement);
            mExecutionStopwatch = Stopwatch.StartNew();
        }

        private Stopwatch mExecutionStopwatch;

        public override void ExecutionEnd()
        {
            mExecutionStopwatch.Stop();
            mDocument.DocumentElement.Attributes.Append(NewXmlAttribute("Duration", mExecutionStopwatch.Elapsed.ToString("c")));
            mDocument.Save(mFilename);
            mCurrentNode.Pop();
        }

        private Stack<XmlElement> mCurrentNode = new Stack<XmlElement>();
        private Stopwatch mTaskStopwatch;
        private Stopwatch mConfigurationStopwatch;

        public override void UnhandledException(Exception exception)
        {
            var xParent = mCurrentNode.Peek();
            var xItem = mDocument.CreateElement("Exception");
            xItem.AppendChild(mDocument.CreateCDataSection(exception.ToString()));
            xParent.AppendChild(xItem);
        }

        public override void TaskStart(string taskName)
        {
            var xParent = mCurrentNode.Peek();
            var xItem = mDocument.CreateElement("Task");
            xItem.Attributes.Append(NewXmlAttribute("TaskName", taskName));
            xParent.AppendChild(xItem);
            mCurrentNode.Push(xItem);
            mTaskStopwatch = Stopwatch.StartNew();
        }

        public override void TaskEnd(string taskName)
        {
            mTaskStopwatch.Stop();
            var xItem = mCurrentNode.Pop();
            xItem.Attributes.Append(NewXmlAttribute("Duration", mKernelStopwatch.Elapsed.ToString("c")));
        }

        public override void SetKernelTestResult(bool succeeded, string message)
        {
            var xItem = mCurrentNode.ElementAt(1);
            if (succeeded)
            {
                LogMessage(message);
            }
            else
            {
                LogError(message);
            }
            xItem.Attributes.Append(NewXmlAttribute("Succeeded", succeeded.ToString()));
        }

        public override void RunConfigurationStart(RunConfiguration configuration)
        {
            var xParent = mCurrentNode.Peek();
            var xItem = mDocument.CreateElement("Configuration");
            xItem.Attributes.Append(NewXmlAttribute("IsELF", configuration.IsELF.ToString()));
            xParent.AppendChild(xItem);
            mCurrentNode.Push(xItem);
            mConfigurationStopwatch = Stopwatch.StartNew();
        }

        public override void RunConfigurationEnd(RunConfiguration configuration)
        {
            mConfigurationStopwatch.Stop();
            var xItem = mCurrentNode.Pop();
            xItem.Attributes.Append(NewXmlAttribute("Duration", mKernelStopwatch.Elapsed.ToString("c")));
        }
    }
}

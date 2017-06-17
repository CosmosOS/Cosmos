using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Cosmos.TestRunner.Core
{
    public partial class OutputHandlerXml: OutputHandlerBase
    {
        private XmlDocument mDocument;

        private bool mConfigurationSucceeded = false;
        private bool mExecutionSucceeded = false;

        public OutputHandlerXml()
        {
        }

        protected override void OnExecuteKernelStart(string assemblyName)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xItem = mDocument.CreateElement("Kernel");
            xItem.Attributes.Append(NewXmlAttribute("AssemblyName", assemblyName));
            xParent.AppendChild(xItem);
            mCurrentNode.Push(xItem);
            mCurrentKernelNode = xItem;
            mKernelStopwatch = Stopwatch.StartNew();
        }

        private Stopwatch mKernelStopwatch;

        protected override void OnExecuteKernelEnd(string assemblyName)
        {
            mKernelStopwatch.Stop();
            var xItem = mCurrentNode.Pop();
            mCurrentKernelNode = null;
            xItem.Attributes.Append(NewXmlAttribute("Duration", mKernelStopwatch.Elapsed.ToString("c")));
        }

        protected override void OnLogDebugMessage(string message)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xNode = xParent.SelectSingleNode("./DebugMessages");
            if (xNode == null)
            {
                xNode = mDocument.CreateElement("DebugMessages");
                xParent.PrependChild(xNode);
            }
            var xItem = mDocument.CreateElement("Message");
            xItem.InnerText = message;
            xNode.AppendChild(xItem);
        }

        protected override void OnLogMessage(string message)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xNode = xParent.SelectSingleNode("./Messages");
            if (xNode == null)
            {
                xNode = mDocument.CreateElement("Messages");
                xParent.PrependChild(xNode);
            }
            var xItem = mDocument.CreateElement("Message");
            xItem.InnerText = message;
            xParent.AppendChild(xItem);
        }

        protected override void OnLogError(string message)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xItem = mDocument.CreateElement("Error");
            xItem.AppendChild(mDocument.CreateCDataSection(message));
            xParent.AppendChild(xItem);
        }

        protected override void OnExecutionStart()
        {
            mDocument = new XmlDocument();
            mDocument.LoadXml("<Execution/>");
            mDocument.DocumentElement.Attributes.Append(NewXmlAttribute("DateTime", DateTime.UtcNow.ToString("O")));
            mCurrentNode.Push(mDocument.DocumentElement);
            mExecutionStopwatch = Stopwatch.StartNew();
            mExecutionSucceeded = true;
        }

        private Stopwatch mExecutionStopwatch;

        protected override void OnExecutionEnd()
        {
            mExecutionStopwatch.Stop();
            mDocument.DocumentElement.Attributes.Append(NewXmlAttribute("Duration", mExecutionStopwatch.Elapsed.ToString("c")));
            mDocument.DocumentElement.Attributes.Append(NewXmlAttribute("Succeeded", mExecutionSucceeded.ToString()));
            mCurrentNode.Pop();
        }

        public void SaveToFile(string filename)
        {
            mDocument.Save(File.OpenWrite(filename));
        }

        private Stack<XmlElement> mCurrentNode = new Stack<XmlElement>();
        private Stopwatch mTaskStopwatch;
        private Stopwatch mConfigurationStopwatch;
        private XmlElement mCurrentKernelNode;

        protected override void OnUnhandledException(Exception exception)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xItem = mDocument.CreateElement("Exception");
            xItem.AppendChild(mDocument.CreateCDataSection(exception.ToString()));
            xParent.AppendChild(xItem);
        }

        protected override void OnTaskStart(string taskName)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xItem = mDocument.CreateElement("Task");
            xItem.Attributes.Append(NewXmlAttribute("TaskName", taskName));
            xParent.AppendChild(xItem);
            mCurrentNode.Push(xItem);
            mTaskStopwatch = Stopwatch.StartNew();
        }

        protected override void OnTaskEnd(string taskName)
        {
            mTaskStopwatch.Stop();
            var xItem = mCurrentNode.Pop();
            xItem.Attributes.Append(NewXmlAttribute("Duration", mTaskStopwatch.Elapsed.ToString("c")));

        }

        protected override void OnSetKernelTestResult(bool succeeded, string message)
        {
            if (succeeded)
            {
                OnLogMessage(message);
            }
            else
            {
                OnLogError(message);
            }
            mCurrentKernelNode.Attributes.Append(NewXmlAttribute("Succeeded", succeeded.ToString()));
            mConfigurationSucceeded &= succeeded;
            mExecutionSucceeded &= succeeded;
        }

        protected override void OnSetKernelSucceededAssertionsCount(int succeededAssertions)
        {
            mCurrentKernelNode.Attributes.Append(NewXmlAttribute("SucceededAssertionsCount", succeededAssertions.ToString()));
        }

        protected override void OnRunConfigurationStart(RunConfiguration configuration)
        {
            XmlElement xParent = mDocument.DocumentElement;
            if (mCurrentNode.Count > 0)
            {
                xParent = mCurrentNode.Peek();
            }
            var xItem = mDocument.CreateElement("Configuration");
            xItem.Attributes.Append(NewXmlAttribute("IsELF", configuration.IsELF.ToString()));
            xItem.Attributes.Append(NewXmlAttribute("RunTarget", configuration.RunTarget.ToString()));
            xParent.AppendChild(xItem);
            mCurrentNode.Push(xItem);
            mConfigurationSucceeded = true;
            mConfigurationStopwatch = Stopwatch.StartNew();
        }

        protected override void OnRunConfigurationEnd(RunConfiguration configuration)
        {
            mConfigurationStopwatch.Stop();
            var xItem = mCurrentNode.Pop();
            xItem.Attributes.Append(NewXmlAttribute("Duration", mKernelStopwatch.Elapsed.ToString("c")));
            xItem.Attributes.Append(NewXmlAttribute("Succeeded", mConfigurationSucceeded.ToString()));
        }
    }
}

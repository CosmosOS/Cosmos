/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Windows.Forms.Design;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// This class implements an MSBuild logger that output events to VS outputwindow and tasklist.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "IDE")]
    internal class IDEBuildLogger : Logger
    {
        #region fields

        // TODO: Remove these constants when we have a version that suppoerts getting the verbosity using automation.
        private string buildVerbosityRegistryRoot = @"Software\Microsoft\VisualStudio\10.0";
        private const string buildVerbosityRegistrySubKey = @"General";
        private const string buildVerbosityRegistryKey = "MSBuildLoggerVerbosity";

        private int currentIndent;
        private IVsOutputWindowPane outputWindowPane;
        private string errorString = SR.GetString(SR.Error, CultureInfo.CurrentUICulture);
        private string warningString = SR.GetString(SR.Warning, CultureInfo.CurrentUICulture);
        private TaskProvider taskProvider;
        private IVsHierarchy hierarchy;
        private IServiceProvider serviceProvider;
        private Dispatcher dispatcher;
        private bool haveCachedVerbosity = false;

        // Queues to manage Tasks and Error output plus message logging
        private ConcurrentQueue<Func<ErrorTask>> taskQueue;
        private ConcurrentQueue<string> outputQueue;

        #endregion

        #region properties

        public IServiceProvider ServiceProvider
        {
            get { return this.serviceProvider; }
        }

        public string WarningString
        {
            get { return this.warningString; }
            set { this.warningString = value; }
        }

        public string ErrorString
        {
            get { return this.errorString; }
            set { this.errorString = value; }
        }

        /// <summary>
        /// When the build is not a "design time" (background or secondary) build this is True
        /// </summary>
        /// <remarks>
        /// The only known way to detect an interactive build is to check this.outputWindowPane for null.
        /// </remarks>
        protected bool InteractiveBuild
        {
            get { return this.outputWindowPane != null; }
        }

        /// <summary>
        /// When building from within VS, setting this will
        /// enable the logger to retrive the verbosity from
        /// the correct registry hive.
        /// </summary>
        internal string BuildVerbosityRegistryRoot
        {
            get { return this.buildVerbosityRegistryRoot; }
            set 
            {
                this.buildVerbosityRegistryRoot = value;
            }
        }

        /// <summary>
        /// Set to null to avoid writing to the output window
        /// </summary>
        internal IVsOutputWindowPane OutputWindowPane
        {
            get { return this.outputWindowPane; }
            set { this.outputWindowPane = value; }
        }

        #endregion

        #region ctors

        /// <summary>
        /// Constructor.  Inititialize member data.
        /// </summary>
        public IDEBuildLogger(IVsOutputWindowPane output, TaskProvider taskProvider, IVsHierarchy hierarchy)
        {
            if (taskProvider == null)
                throw new ArgumentNullException("taskProvider");
            if (hierarchy == null)
                throw new ArgumentNullException("hierarchy");

            Trace.WriteLineIf(Thread.CurrentThread.GetApartmentState() != ApartmentState.STA, "WARNING: IDEBuildLogger constructor running on the wrong thread.");

            IOleServiceProvider site;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hierarchy.GetSite(out site));

            this.taskProvider = taskProvider;
            this.outputWindowPane = output;
            this.hierarchy = hierarchy;
            this.serviceProvider = new ServiceProvider(site);
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion

        #region overridden methods

        /// <summary>
        /// Overridden from the Logger class.
        /// </summary>
        public override void Initialize(IEventSource eventSource)
        {
            if (null == eventSource)
            {
                throw new ArgumentNullException("eventSource");
            }

            this.taskQueue = new ConcurrentQueue<Func<ErrorTask>>();
            this.outputQueue = new ConcurrentQueue<string>();

            eventSource.BuildStarted += new BuildStartedEventHandler(BuildStartedHandler);
            eventSource.BuildFinished += new BuildFinishedEventHandler(BuildFinishedHandler);
            eventSource.ProjectStarted += new ProjectStartedEventHandler(ProjectStartedHandler);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(ProjectFinishedHandler);
            eventSource.TargetStarted += new TargetStartedEventHandler(TargetStartedHandler);
            eventSource.TargetFinished += new TargetFinishedEventHandler(TargetFinishedHandler);
            eventSource.TaskStarted += new TaskStartedEventHandler(TaskStartedHandler);
            eventSource.TaskFinished += new TaskFinishedEventHandler(TaskFinishedHandler);
            eventSource.CustomEventRaised += new CustomBuildEventHandler(CustomHandler);
            eventSource.ErrorRaised += new BuildErrorEventHandler(ErrorHandler);
            eventSource.WarningRaised += new BuildWarningEventHandler(WarningHandler);
            eventSource.MessageRaised += new BuildMessageEventHandler(MessageHandler);
        }

        #endregion

        #region event delegates

        /// <summary>
        /// This is the delegate for BuildStartedHandler events.
        /// </summary>
        protected virtual void BuildStartedHandler(object sender, BuildStartedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            ClearCachedVerbosity();
            ClearQueuedOutput();
            ClearQueuedTasks();

            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for BuildFinishedHandler events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buildEvent"></param>
        protected virtual void BuildFinishedHandler(object sender, BuildFinishedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            MessageImportance importance = buildEvent.Succeeded ? MessageImportance.Low : MessageImportance.High;
            QueueOutputText(importance, Environment.NewLine);
            QueueOutputEvent(importance, buildEvent);

            // flush output and error queues
            ReportQueuedOutput();
            ReportQueuedTasks();
        }

        /// <summary>
        /// This is the delegate for ProjectStartedHandler events.
        /// </summary>
        protected virtual void ProjectStartedHandler(object sender, ProjectStartedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for ProjectFinishedHandler events.
        /// </summary>
        protected virtual void ProjectFinishedHandler(object sender, ProjectFinishedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(buildEvent.Succeeded ? MessageImportance.Low : MessageImportance.High, buildEvent);
        }

        /// <summary>
        /// This is the delegate for TargetStartedHandler events.
        /// </summary>
        protected virtual void TargetStartedHandler(object sender, TargetStartedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.Low, buildEvent);
            IndentOutput();
        }

        /// <summary>
        /// This is the delegate for TargetFinishedHandler events.
        /// </summary>
        protected virtual void TargetFinishedHandler(object sender, TargetFinishedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            UnindentOutput();
            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for TaskStartedHandler events.
        /// </summary>
        protected virtual void TaskStartedHandler(object sender, TaskStartedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.Low, buildEvent);
            IndentOutput();
        }

        /// <summary>
        /// This is the delegate for TaskFinishedHandler events.
        /// </summary>
        protected virtual void TaskFinishedHandler(object sender, TaskFinishedEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            UnindentOutput();
            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for CustomHandler events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buildEvent"></param>
        protected virtual void CustomHandler(object sender, CustomBuildEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.High, buildEvent);
        }

        /// <summary>
        /// This is the delegate for error events.
        /// </summary>
        protected virtual void ErrorHandler(object sender, BuildErrorEventArgs errorEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputText(GetFormattedErrorMessage(errorEvent.File, errorEvent.LineNumber, errorEvent.ColumnNumber, false, errorEvent.Code, errorEvent.Message));
            QueueTaskEvent(errorEvent);
        }

        /// <summary>
        /// This is the delegate for warning events.
        /// </summary>
        protected virtual void WarningHandler(object sender, BuildWarningEventArgs warningEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputText(MessageImportance.High, GetFormattedErrorMessage(warningEvent.File, warningEvent.LineNumber, warningEvent.ColumnNumber, true, warningEvent.Code, warningEvent.Message));
            QueueTaskEvent(warningEvent);
        }

        /// <summary>
        /// This is the delegate for Message event types
        /// </summary>		
        protected virtual void MessageHandler(object sender, BuildMessageEventArgs messageEvent)
        {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(messageEvent.Importance, messageEvent);
        }

        #endregion

        #region output queue

        protected void QueueOutputEvent(MessageImportance importance, BuildEventArgs buildEvent)
        {
            // NOTE: This may run on a background thread!
            if (LogAtImportance(importance) && !string.IsNullOrEmpty(buildEvent.Message))
            {
                StringBuilder message = new StringBuilder(this.currentIndent + buildEvent.Message.Length);
                if (this.currentIndent > 0)
                {
                    message.Append('\t', this.currentIndent);
                }
                message.AppendLine(buildEvent.Message);

                QueueOutputText(message.ToString());
            }
        }

        protected void QueueOutputText(MessageImportance importance, string text)
        {
            // NOTE: This may run on a background thread!
            if (LogAtImportance(importance))
            {
                QueueOutputText(text);
            }
        }

        protected void QueueOutputText(string text)
        {
            // NOTE: This may run on a background thread!
            if (this.OutputWindowPane != null)
            {
                // Enqueue the output text
                this.outputQueue.Enqueue(text);

                // We want to interactively report the output. But we dont want to dispatch
                // more than one at a time, otherwise we might overflow the main thread's
                // message queue. So, we only report the output if the queue was empty.
                if (this.outputQueue.Count == 1)
                {
                    ReportQueuedOutput();
                }
            }
        }

        private void IndentOutput()
        {
            // NOTE: This may run on a background thread!
            this.currentIndent++;
        }

        private void UnindentOutput()
        {
            // NOTE: This may run on a background thread!
            this.currentIndent--;
        }

        private void ReportQueuedOutput()
        {
            // NOTE: This may run on a background thread!
            // We need to output this on the main thread. We must use BeginInvoke because the main thread may not be pumping events yet.
            BeginInvokeWithErrorMessage(this.serviceProvider, this.dispatcher, () =>
            {
                if (this.OutputWindowPane != null)
                {
                    string outputString;

                    while (this.outputQueue.TryDequeue(out outputString))
                    {
                        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(this.OutputWindowPane.OutputString(outputString));
                    }
                }
            });
        }

        private void ClearQueuedOutput()
        {
            // NOTE: This may run on a background thread!
            this.outputQueue = new ConcurrentQueue<string>();
        }

        #endregion output queue

        #region task queue

        protected void QueueTaskEvent(BuildEventArgs errorEvent)
        {
            this.taskQueue.Enqueue(() =>
            {
                ErrorTask task = new ErrorTask();

                if (errorEvent is BuildErrorEventArgs)
                {
                    BuildErrorEventArgs errorArgs = (BuildErrorEventArgs)errorEvent;
                    task.Document = errorArgs.File;
                    task.ErrorCategory = TaskErrorCategory.Error;
                    task.Line = errorArgs.LineNumber - 1; // The task list does +1 before showing this number.
                    task.Column = errorArgs.ColumnNumber;
                    task.Priority = TaskPriority.High;
                }
                else if (errorEvent is BuildWarningEventArgs)
                {
                    BuildWarningEventArgs warningArgs = (BuildWarningEventArgs)errorEvent;
                    task.Document = warningArgs.File;
                    task.ErrorCategory = TaskErrorCategory.Warning;
                    task.Line = warningArgs.LineNumber - 1; // The task list does +1 before showing this number.
                    task.Column = warningArgs.ColumnNumber;
                    task.Priority = TaskPriority.Normal;
                }

                task.Text = errorEvent.Message;
                task.Category = TaskCategory.BuildCompile;
                task.HierarchyItem = hierarchy;

                return task;
            });

            // NOTE: Unlike output we dont want to interactively report the tasks. So we never queue
            // call ReportQueuedTasks here. We do this when the build finishes.
        }

        private void ReportQueuedTasks()
        {
            // NOTE: This may run on a background thread!
            // We need to output this on the main thread. We must use BeginInvoke because the main thread may not be pumping events yet.
            BeginInvokeWithErrorMessage(this.serviceProvider, this.dispatcher, () =>
            {
                this.taskProvider.SuspendRefresh();
                try
                {
                    Func<ErrorTask> taskFunc;

                    while (this.taskQueue.TryDequeue(out taskFunc))
                    {
                        // Create the error task
                        ErrorTask task = taskFunc();

                        // Log the task
                        this.taskProvider.Tasks.Add(task);
                    }
                }
                finally
                {
                    this.taskProvider.ResumeRefresh();
                }
            });
        }

        private void ClearQueuedTasks()
        {
            // NOTE: This may run on a background thread!
            this.taskQueue = new ConcurrentQueue<Func<ErrorTask>>();

            if (this.InteractiveBuild)
            {
                // We need to clear this on the main thread. We must use BeginInvoke because the main thread may not be pumping events yet.
                BeginInvokeWithErrorMessage(this.serviceProvider, this.dispatcher, () =>
                {
                    this.taskProvider.Tasks.Clear();
                });
            }
        }

        #endregion task queue

        #region helpers

        /// <summary>
        /// This method takes a MessageImportance and returns true if messages
        /// at importance i should be loggeed.  Otherwise return false.
        /// </summary>
        private bool LogAtImportance(MessageImportance importance)
        {
            // If importance is too low for current settings, ignore the event
            bool logIt = false;

            this.SetVerbosity();

            switch (this.Verbosity)
            {
                case LoggerVerbosity.Quiet:
                    logIt = false;
                    break;
                case LoggerVerbosity.Minimal:
                    logIt = (importance == MessageImportance.High);
                    break;
                case LoggerVerbosity.Normal:
                // Falling through...
                case LoggerVerbosity.Detailed:
                    logIt = (importance != MessageImportance.Low);
                    break;
                case LoggerVerbosity.Diagnostic:
                    logIt = true;
                    break;
                default:
                    Debug.Fail("Unknown Verbosity level. Ignoring will cause everything to be logged");
                    break;
            }

            return logIt;
        }

        /// <summary>
        /// Format error messages for the task list
        /// </summary>
        private string GetFormattedErrorMessage(
            string fileName,
            int line,
            int column,
            bool isWarning,
            string errorNumber,
            string errorText)
        {
            string errorCode = isWarning ? this.WarningString : this.ErrorString;

            StringBuilder message = new StringBuilder();
            if (!string.IsNullOrEmpty(fileName))
            {
                message.AppendFormat(CultureInfo.CurrentCulture, "{0}({1},{2}):", fileName, line, column);
            }
            message.AppendFormat(CultureInfo.CurrentCulture, " {0} {1}: {2}", errorCode, errorNumber, errorText);
            message.AppendLine();

            return message.ToString();
        }

        /// <summary>
        /// Sets the verbosity level.
        /// </summary>
        private void SetVerbosity()
        {
            // TODO: This should be replaced when we have a version that supports automation.
            if (!this.haveCachedVerbosity)
            {
                string verbosityKey = String.Format(CultureInfo.InvariantCulture, @"{0}\{1}", BuildVerbosityRegistryRoot, buildVerbosityRegistrySubKey);
                using (RegistryKey subKey = Registry.CurrentUser.OpenSubKey(verbosityKey))
                {
                    if (subKey != null)
                    {
                        object valueAsObject = subKey.GetValue(buildVerbosityRegistryKey);
                        if (valueAsObject != null)
                        {
                            this.Verbosity = (LoggerVerbosity)((int)valueAsObject);
                        }
                    }
                }

                this.haveCachedVerbosity = true;
            }
        }

        /// <summary>
        /// Clear the cached verbosity, so that it will be re-evaluated from the build verbosity registry key.
        /// </summary>
        private void ClearCachedVerbosity()
        {
            this.haveCachedVerbosity = false;
        }

        #endregion helpers

        #region exception handling helpers

        /// <summary>
        /// Call Dispatcher.BeginInvoke, showing an error message if there was a non-critical exception.
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <param name="dispatcher">dispatcher</param>
        /// <param name="action">action to invoke</param>
        private static void BeginInvokeWithErrorMessage(IServiceProvider serviceProvider, Dispatcher dispatcher, Action action)
        {
            dispatcher.BeginInvoke(new Action(() => CallWithErrorMessage(serviceProvider, action)));
        }

        /// <summary>
        /// Show error message if exception is caught when invoking a method
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <param name="action">action to invoke</param>
        private static void CallWithErrorMessage(IServiceProvider serviceProvider, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (Microsoft.VisualStudio.ErrorHandler.IsCriticalException(ex))
                {
                    throw;
                }

                ShowErrorMessage(serviceProvider, ex);
            }
        }

        /// <summary>
        /// Show error window about the exception
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <param name="exception">exception</param>
        private static void ShowErrorMessage(IServiceProvider serviceProvider, Exception exception)
        {
            IUIService UIservice = (IUIService)serviceProvider.GetService(typeof(IUIService));
            if (UIservice != null && exception != null)
            {
                UIservice.ShowError(exception);
            }
        }

        #endregion exception handling helpers
    }
}
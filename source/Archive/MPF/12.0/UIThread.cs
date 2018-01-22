/********************************************************************************************

Copyright (c) Microsoft Corporation 
All rights reserved. 

Microsoft Public License: 

This license governs use of the accompanying software. If you use the software, you 
accept this license. If you do not accept the license, do not use the software. 

1. Definitions 
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the 
same meaning here as under U.S. copyright law. 
A "contribution" is the original software, or any additions or changes to the software. 
A "contributor" is any person that distributes its contribution under this license. 
"Licensed patents" are a contributor's patent claims that read directly on its contribution. 

2. Grant of Rights 
(A) Copyright Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free copyright license to reproduce its contribution, prepare derivative works of 
its contribution, and distribute its contribution or any derivative works that you create. 
(B) Patent Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free license under its licensed patents to make, have made, use, sell, offer for 
sale, import, and/or otherwise dispose of its contribution in the software or derivative 
works of the contribution in the software. 

3. Conditions and Limitations 
(A) No Trademark License- This license does not grant you rights to use any contributors' 
name, logo, or trademarks. 
(B) If you bring a patent claim against any contributor over patents that you claim are 
infringed by the software, your patent license from such contributor to the software ends 
automatically. 
(C) If you distribute any portion of the software, you must retain all copyright, patent, 
trademark, and attribution notices that are present in the software. 
(D) If you distribute any portion of the software in source code form, you may do so only 
under this license by including a complete copy of this license with your distribution. 
If you distribute any portion of the software in compiled or object code form, you may only 
do so under a license that complies with this license. 
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give 
no express warranties, guarantees or conditions. You may have additional consumer rights 
under your local laws which this license cannot change. To the extent permitted under your 
local laws, the contributors exclude the implied warranties of merchantability, fitness for 
a particular purpose and non-infringement.

********************************************************************************************/

namespace Microsoft.VisualStudio.Project
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class UIThread : IDisposable
    {
        private WindowsFormsSynchronizationContext synchronizationContext;
#if DEBUG
        /// <summary>
        /// Stack trace when synchronizationContext was captured
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private StackTrace captureStackTrace;
#endif

        private Thread uithread; 

        /// <summary>
        /// RunSync puts orignal exception stacktrace to Exception.Data by this key if action throws on UI thread
        /// </summary>
        /// WrappedStacktraceKey is a string to keep exception serializable.
        private const string WrappedStacktraceKey = "$$Microsoft.VisualStudio.Package.UIThread.WrappedStacktraceKey$$";

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static volatile UIThread instance = new UIThread();

        internal UIThread()
        {
            this.Initialize();
        }

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static UIThread Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Checks whether this is the UI thread.
        /// </summary>
        public bool IsUIThread
        {
            get { return this.uithread == System.Threading.Thread.CurrentThread; }
        }

        /// <summary>
        /// Gets a value indicating whether unit tests are running.
        /// </summary>
        internal static bool IsUnitTest { get; set; }

        #region IDisposable Members
        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            if (this.synchronizationContext != null)
            {
                this.synchronizationContext.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Initializes unit testing mode for this object
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void InitUnitTestingMode()
        {
            Debug.Assert(this.synchronizationContext == null, "Context has already been captured; too late to InitUnitTestingMode");
            IsUnitTest = true;
        }

        [Conditional("DEBUG")]
        internal void MustBeCalledFromUIThread()
        {
            Debug.Assert(this.uithread == System.Threading.Thread.CurrentThread || IsUnitTest, "This must be called from the GUI thread");
        }

        /// <summary>
        /// Runs an action asynchronously on an associated forms synchronization context.
        /// </summary>
        /// <param name="a">The action to run</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void Run(Action a)
        {
            if (IsUnitTest)
            {
                a();
                return;
            }
            Debug.Assert(this.synchronizationContext != null, "The SynchronizationContext must be captured before calling this method");
#if DEBUG
            StackTrace stackTrace = new StackTrace(true);
#endif
            this.synchronizationContext.Post(delegate(object ignore)
            {
                try
                {
                    this.MustBeCalledFromUIThread();
                    a();
                }
#if DEBUG
                catch (Exception e)
                {
                    // swallow, random exceptions should not kill process
                    Debug.Assert(false, string.Format(CultureInfo.InvariantCulture, "UIThread.Run caught and swallowed exception: {0}\n\noriginally invoked from stack:\n{1}", e.ToString(), stackTrace.ToString()));
                }
#else
                catch (Exception)
                {
                    // swallow, random exceptions should not kill process
                }
#endif
            }, null);

        }

        /// <summary>
        /// Runs an action synchronously on an associated forms synchronization context
        /// </summary>
        /// <param name="a">The action to run.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void RunSync(Action a)
        {
            if (IsUnitTest)
            {
                a();
                return;
            }
            Exception exn = null; ;
            Debug.Assert(this.synchronizationContext != null, "The SynchronizationContext must be captured before calling this method");
            
            // Send on UI thread will execute immediately.
            this.synchronizationContext.Send(ignore =>
            {
                try
                {
                    this.MustBeCalledFromUIThread();
                    a();
                }
                catch (Exception e)
                {
                    exn = e;
                }
            }, null
            );
            if (exn != null)
            {
                // throw exception on calling thread, preserve stacktrace
                if (!exn.Data.Contains(WrappedStacktraceKey)) exn.Data[WrappedStacktraceKey] = exn.StackTrace;
                throw exn;
            }
        }

        /// <summary>
        /// Performs a callback on the UI thread, blocking until the action completes.  Uses the VS mechanism 
        /// of marshalling back to the main STA thread via COM RPC.
        /// </summary>
        internal static T DoOnUIThread<T>(Func<T> callback)
        {
            return ThreadHelper.Generic.Invoke<T>(callback);
        }

        /// <summary>
        /// Performs a callback on the UI thread, blocking until the action completes.  Uses the VS mechanism 
        /// of marshalling back to the main STA thread via COM RPC.
        /// </summary>
        internal static void DoOnUIThread(Action callback)
        {
            ThreadHelper.Generic.Invoke(callback);
        }

        /// <summary>
        /// Initializes this object.
        /// </summary>
        private void Initialize()
        {
            if (IsUnitTest) return;
            this.uithread = System.Threading.Thread.CurrentThread;

            if (this.synchronizationContext == null)
            {
#if DEBUG
                 // This is a handy place to do this, since the product and all interesting unit tests
                 // must go through this code path.
                 AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(delegate(object sender, UnhandledExceptionEventArgs args)
                 {
                     if (args.IsTerminating)
                     {
                         string s = String.Format(CultureInfo.InvariantCulture, "An unhandled exception is about to terminate the process.  Exception info:\n{0}", args.ExceptionObject.ToString());
                         Debug.Assert(false, s);
                     }
                 });

                 this.captureStackTrace = new StackTrace(true);
#endif
                this.synchronizationContext = new WindowsFormsSynchronizationContext();
            }
            else
            {
                 // Make sure we are always capturing the same thread.
                 Debug.Assert(this.uithread == Thread.CurrentThread);
            }
        }       
    }
}

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

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// This object is in charge of reloading nodes that have file monikers that can be listened to changes
    /// </summary>
    internal class FileChangeManager : IVsFileChangeEvents
    {
        #region nested objects
        /// <summary>
        /// Defines a data structure that can link a item moniker to the item and its file change cookie.
        /// </summary>
        private struct ObservedItemInfo
        {
            /// <summary>
            /// Defines the id of the item that is to be reloaded.
            /// </summary>
            private uint itemID;

            /// <summary>
            /// Defines the file change cookie that is returned when listening on file changes on the nested project item.
            /// </summary>
            private uint fileChangeCookie;

            /// <summary>
            /// Defines the nested project item that is to be reloaded.
            /// </summary>
            internal uint ItemID
            {
                get
                {
                    return this.itemID;
                }

                set
                {
                    this.itemID = value;
                }
            }

            /// <summary>
            /// Defines the file change cookie that is returned when listenning on file changes on the nested project item.
            /// </summary>
            internal uint FileChangeCookie
            {
                get
                {
                    return this.fileChangeCookie;
                }

                set
                {
                    this.fileChangeCookie = value;
                }
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Event that is raised when one of the observed file names have changed on disk.
        /// </summary>
        internal event EventHandler<FileChangedOnDiskEventArgs> FileChangedOnDisk;

        /// <summary>
        /// Reference to the FileChange service.
        /// </summary>
        private IVsFileChangeEx fileChangeService;

        /// <summary>
        /// Maps between the observed item identified by its filename (in canonicalized form) and the cookie used for subscribing 
        /// to the events.
        /// </summary>
        private Dictionary<string, ObservedItemInfo> observedItems = new Dictionary<string, ObservedItemInfo>();

        /// <summary>
        /// Has Disposed already been called?
        /// </summary>
        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Overloaded ctor.
        /// </summary>
        /// <param name="nodeParam">An instance of a project item.</param>
        internal FileChangeManager(IServiceProvider serviceProvider)
        {
            #region input validation
            if(serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            #endregion

            this.fileChangeService = (IVsFileChangeEx)serviceProvider.GetService(typeof(SVsFileChangeEx));

            if(this.fileChangeService == null)
            {
                // VS is in bad state, since the SVsFileChangeEx could not be proffered.
                throw new InvalidOperationException();
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            // Don't dispose more than once
            if(this.disposed)
            {
                return;
            }

            this.disposed = true;

            // Unsubscribe from the observed source files.
            foreach(ObservedItemInfo info in this.observedItems.Values)
            {
                ErrorHandler.ThrowOnFailure(this.fileChangeService.UnadviseFileChange(info.FileChangeCookie));
            }

            // Clean the observerItems list
            this.observedItems.Clear();
        }
        #endregion

        #region IVsFileChangeEvents Members
        /// <summary>
        /// Called when one of the file have changed on disk.
        /// </summary>
        /// <param name="numberOfFilesChanged">Number of files changed.</param>
        /// <param name="filesChanged">Array of file names.</param>
        /// <param name="flags">Array of flags indicating the type of changes. See _VSFILECHANGEFLAGS.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        int IVsFileChangeEvents.FilesChanged(uint numberOfFilesChanged, string[] filesChanged, uint[] flags)
        {
            if (filesChanged == null)
            {
                throw new ArgumentNullException("filesChanged");
            }

            if (flags == null)
            {
                throw new ArgumentNullException("flags");
            }
            
            if(this.FileChangedOnDisk != null)
            {
                for(int i = 0; i < numberOfFilesChanged; i++)
                {
                    string fullFileName = Utilities.CanonicalizeFileName(filesChanged[i]);
                    if(this.observedItems.ContainsKey(fullFileName))
                    {
                        ObservedItemInfo info = this.observedItems[fullFileName];
                        this.FileChangedOnDisk(this, new FileChangedOnDiskEventArgs(fullFileName, info.ItemID, (_VSFILECHANGEFLAGS)flags[i]));
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notifies clients of changes made to a directory. 
        /// </summary>
        /// <param name="directory">Name of the directory that had a change.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
        int IVsFileChangeEvents.DirectoryChanged(string directory)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region helpers
        /// <summary>
        /// Observe when the given file is updated on disk. In this case we do not care about the item id that represents the file in the hierarchy.
        /// </summary>
        /// <param name="fileName">File to observe.</param>
        internal void ObserveItem(string fileName)
        {
            this.ObserveItem(fileName, VSConstants.VSITEMID_NIL);
        }

        /// <summary>
        /// Observe when the given file is updated on disk.
        /// </summary>
        /// <param name="fileName">File to observe.</param>
        /// <param name="id">The item id of the item to observe.</param>
        internal void ObserveItem(string fileName, uint id)
        {
            #region Input validation
            if(String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), "fileName");
            }
            #endregion

            string fullFileName = Utilities.CanonicalizeFileName(fileName);
            if(!this.observedItems.ContainsKey(fullFileName))
            {
                // Observe changes to the file
                uint fileChangeCookie;
                ErrorHandler.ThrowOnFailure(this.fileChangeService.AdviseFileChange(fullFileName, (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Del), this, out fileChangeCookie));

                ObservedItemInfo itemInfo = new ObservedItemInfo();
                itemInfo.ItemID = id;
                itemInfo.FileChangeCookie = fileChangeCookie;

                // Remember that we're observing this file (used in FilesChanged event handler)
                this.observedItems.Add(fullFileName, itemInfo);
            }
        }

        /// <summary>
        /// Ignore item file changes for the specified item.
        /// </summary>
        /// <param name="fileName">File to ignore observing.</param>
        /// <param name="ignore">Flag indicating whether or not to ignore changes (1 to ignore, 0 to stop ignoring).</param>
        internal void IgnoreItemChanges(string fileName, bool ignore)
        {
            #region Input validation
            if(String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), "fileName");
            }
            #endregion

            string fullFileName = Utilities.CanonicalizeFileName(fileName);
            if(this.observedItems.ContainsKey(fullFileName))
            {
                // Call ignore file with the flags specified.
                ErrorHandler.ThrowOnFailure(this.fileChangeService.IgnoreFile(0, fileName, ignore ? 1 : 0));
            }
        }

        /// <summary>
        /// Stop observing when the file is updated on disk.
        /// </summary>
        /// <param name="fileName">File to stop observing.</param>
        internal void StopObservingItem(string fileName)
        {
            #region Input validation
            if(String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, CultureInfo.CurrentUICulture), "fileName");
            }
            #endregion

            string fullFileName = Utilities.CanonicalizeFileName(fileName);

            if(this.observedItems.ContainsKey(fullFileName))
            {
                // Get the cookie that was used for this.observedItems to this file.
                ObservedItemInfo itemInfo = this.observedItems[fullFileName];

                // Remove the file from our observed list. It's important that this is done before the call to 
                // UnadviseFileChange, because for some reason, the call to UnadviseFileChange can trigger a 
                // FilesChanged event, and we want to be able to filter that event away.
                this.observedItems.Remove(fullFileName);

                // Stop observing the file
                ErrorHandler.ThrowOnFailure(this.fileChangeService.UnadviseFileChange(itemInfo.FileChangeCookie));
            }
        }
        #endregion
    }
}

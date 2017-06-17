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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio;

namespace Microsoft.VisualStudio.Project
{
    public class ImageHandler : IDisposable
    {
        private ImageList imageList;
        private List<IntPtr> iconHandles;
        private static volatile object Mutex;
        private bool isDisposed;

        /// <summary>
        /// Initializes the <see cref="RDTListener"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ImageHandler()
        {
            Mutex = new object();
        }

        /// <summary>
        /// Builds an empty ImageHandler object.
        /// </summary>
        public ImageHandler()
        {
        }

        /// <summary>
        /// Builds an ImageHandler object from a Stream providing the bitmap that
        /// stores the images for the image list.
        /// </summary>
        public ImageHandler(Stream resourceStream)
        {
            if(null == resourceStream)
            {
                throw new ArgumentNullException("resourceStream");
            }
            imageList = Utilities.GetImageList(resourceStream);
        }

        /// <summary>
        /// Builds an ImageHandler object from an ImageList object.
        /// </summary>
        public ImageHandler(ImageList list)
        {
            if(null == list)
            {
                throw new ArgumentNullException("list");
            }
            imageList = list;
        }

        /// <summary>
        /// Closes the ImageHandler object freeing its resources.
        /// </summary>
        public void Close()
        {
            if(null != iconHandles)
            {
                foreach(IntPtr hnd in iconHandles)
                {
                    if(hnd != IntPtr.Zero)
                    {
                        NativeMethods.DestroyIcon(hnd);
                    }
                }
                iconHandles = null;
            }

            if(null != imageList)
            {
                imageList.Dispose();
                imageList = null;
            }
        }

        /// <summary>
        /// Add an image to the ImageHandler.
        /// </summary>
        /// <param name="image">the image object to be added.</param>
        public void AddImage(Image image)
        {
            if(null == image)
            {
                throw new ArgumentNullException("image");
            }
            if(null == imageList)
            {
                imageList = new ImageList();
            }
            imageList.Images.Add(image);
            if(null != iconHandles)
            {
                iconHandles.Add(IntPtr.Zero);
            }
        }

        /// <summary>
        /// Get or set the ImageList object for this ImageHandler.
        /// </summary>
        public ImageList ImageList
        {
            get { return imageList; }
            set
            {
                Close();
                imageList = value;
            }
        }

        /// <summary>
        /// Returns the handle to an icon build from the image of index
        /// iconIndex in the image list.
        /// </summary>
        public IntPtr GetIconHandle(int iconIndex)
        {
            // Verify that the object is in a consistent state.
            if((null == imageList))
            {
                throw new InvalidOperationException();
            }
            // Make sure that the list of handles is initialized.
            if(null == iconHandles)
            {
                InitHandlesList();
            }

            // Verify that the index is inside the expected range.
            if((iconIndex < 0) || (iconIndex >= iconHandles.Count))
            {
                throw new ArgumentOutOfRangeException("iconIndex");
            }

            // Check if the icon is in the cache.
            if(IntPtr.Zero == iconHandles[iconIndex])
            {
                Bitmap bitmap = imageList.Images[iconIndex] as Bitmap;
                // If the image is not a bitmap, then we can not build the icon,
                // so we have to return a null handle.
                if(null == bitmap)
                {
                    return IntPtr.Zero;
                }

                iconHandles[iconIndex] = bitmap.GetHicon();
            }

            return iconHandles[iconIndex];
        }

        private void InitHandlesList()
        {
            iconHandles = new List<IntPtr>(imageList.Images.Count);
            for(int i = 0; i < imageList.Images.Count; ++i)
            {
                iconHandles.Add(IntPtr.Zero);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void Dispose(bool disposing)
        {
            if(!this.isDisposed)
            {
                lock(Mutex)
                {
                    if(disposing)
                    {
                        this.imageList.Dispose();
                    }

                    this.isDisposed = true;
                }
            }
        }
    }
}

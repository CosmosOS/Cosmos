/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project
{
	internal static class UnsafeNativeMethods
	{
		[DllImport(ExternDll.Kernel32, EntryPoint = "GlobalLock", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern IntPtr GlobalLock(IntPtr h);

		[DllImport(ExternDll.Kernel32, EntryPoint = "GlobalUnlock", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern bool GlobalUnLock(IntPtr h);

		[DllImport(ExternDll.Kernel32, EntryPoint = "GlobalSize", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern int GlobalSize(IntPtr h);

		[DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int OleSetClipboard(Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject);

		[DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int OleGetClipboard(out Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject);

		[DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int OleFlushClipboard();

		[DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int OpenClipboard(IntPtr newOwner);

		[DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int EmptyClipboard();

		[DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int CloseClipboard();

		[DllImport(ExternDll.Comctl32, CharSet = CharSet.Auto)]
		internal static extern int ImageList_GetImageCount(HandleRef himl);

		[DllImport(ExternDll.Comctl32, CharSet = CharSet.Auto)]
		internal static extern bool ImageList_Draw(HandleRef himl, int i, HandleRef hdcDst, int x, int y, int fStyle);

		[DllImport(ExternDll.Shell32, EntryPoint = "DragQueryFileW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern uint DragQueryFile(IntPtr hDrop, uint iFile, char[] lpszFile, uint cch);

		[DllImport(ExternDll.User32, EntryPoint = "RegisterClipboardFormatW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern ushort RegisterClipboardFormat(string format);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport(ExternDll.Shell32, EntryPoint = "SHGetSpecialFolderLocation")]
		internal static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] ppidl);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport(ExternDll.Shell32, EntryPoint = "SHGetPathFromIDList", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		internal static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);
	}
}


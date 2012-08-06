//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Imports ICorPublish interface from CorPublish.idl into managed code
//---------------------------------------------------------------------

using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using IStream=System.Runtime.InteropServices.ComTypes.IStream;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Samples.Debugging.CorPublish.NativeApi
{
      public enum __MIDL___MIDL_itf_corpub_0000_0001
      {
            // Fields
            COR_PUB_MANAGEDONLY = 1
      }

      public enum COR_PUB_ENUMPROCESS
      {
            // Fields
            COR_PUB_MANAGEDONLY = 1
      }

      [ComImport, Guid("9613A0E7-5A68-11D3-8F84-00A0C9B4D50C"), CoClass(typeof(CorpubPublishClass))]
      public interface CorpubPublish : ICorPublish
      {
      }

      [ComImport, TypeLibType(2), Guid("047a9a40-657e-11d3-8d5b-00104b35e7ef"), ClassInterface(ClassInterfaceType.None)]
      public class CorpubPublishClass : ICorPublish, CorpubPublish
      {
          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          public virtual extern void EnumProcesses(
                [In, ComAliasName("CorpubProcessLib.COR_PUB_ENUMPROCESS")] COR_PUB_ENUMPROCESS Type, 
                [Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishProcessEnum ppIEnum);

          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          public virtual extern void GetProcess([In] uint pid, [Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishProcess ppProcess);
      }

      [ComImport, Guid("9613A0E7-5A68-11D3-8F84-00A0C9B4D50C"), InterfaceType(1)]
      public interface ICorPublish
      {
            
            void EnumProcesses([In, ComAliasName("CorpubProcessLib.COR_PUB_ENUMPROCESS")] COR_PUB_ENUMPROCESS Type, [Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishProcessEnum ppIEnum);
            
            void GetProcess([In] uint pid, [Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishProcess ppProcess);
      }

      [ComImport, Guid("D6315C8F-5A6A-11D3-8F84-00A0C9B4D50C"), InterfaceType(1)]
      public interface ICorPublishAppDomain
      {
            
            void GetID([Out] out uint puId);
            
            void GetName([In] uint cchName, [Out] out uint pcchName, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName);
      }

      [ComImport, InterfaceType(1), Guid("9F0C98F5-5A6A-11D3-8F84-00A0C9B4D50C")]
      public interface ICorPublishAppDomainEnum : ICorPublishEnum
      {
            
            new void Skip([In] uint celt);
            
            new void Reset();
            
            new void Clone([Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishEnum ppEnum);
            
            new void GetCount([Out] out uint pcelt);
            [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            int Next([In] uint celt, [Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishAppDomain objects, [Out] out uint pceltFetched);
      }

      [ComImport, InterfaceType(1), Guid("C0B22967-5A69-11D3-8F84-00A0C9B4D50C")]
      public interface ICorPublishEnum
      {
            
            void Skip([In] uint celt);
            
            void Reset();
            
            void Clone([Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishEnum ppEnum);
            
            void GetCount([Out] out uint pcelt);
      }

      [ComImport, InterfaceType(1), Guid("18D87AF1-5A6A-11D3-8F84-00A0C9B4D50C")]
      public interface ICorPublishProcess
      {
            
            void IsManaged([Out] out int pbManaged);
            
            void EnumAppDomains([Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishAppDomainEnum ppEnum);
            
            void GetProcessID([Out] out uint pid);
            
            void GetDisplayName([In] uint cchName, [Out] out uint pcchName, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName);
      }

      [ComImport, InterfaceType(1), Guid("A37FBD41-5A69-11D3-8F84-00A0C9B4D50C")]
      public interface ICorPublishProcessEnum : ICorPublishEnum
      {
            
            new void Skip([In] uint celt);
            
            new void Reset();
            
            new void Clone([Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishEnum ppEnum);
            
            new void GetCount([Out] out uint pcelt);
            [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            int Next([In] uint celt, [Out, MarshalAs(UnmanagedType.Interface)] out ICorPublishProcess objects, [Out] out uint pceltFetched);
      }
}

 

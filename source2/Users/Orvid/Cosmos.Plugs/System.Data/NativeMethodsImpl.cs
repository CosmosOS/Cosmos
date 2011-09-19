namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class Bid+NativeMethodsImpl
    //{

    //    public static System.Void AddMetaText(System.IntPtr hID, System.IntPtr cmdSpace, Bid+CtlCmd cmd, System.IntPtr nop1, System.String txtID, System.IntPtr nop2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.AddMetaText' has not been implemented!");
    //    }

    //    public static System.Void DllBidEntryPoint(System.IntPtr* hID, System.Int32 bInitAndVer, System.String sIdentity, System.UInt32 propBits, Bid+ApiGroup* pGblFlags, Bid+CtrlCB fAddr, Bid+BIDEXTINFO* pExtInfo, System.IntPtr pHooks, System.IntPtr pHdr)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.DllBidEntryPoint' has not been implemented!");
    //    }

    //    public static System.Void DllBidEntryPoint(System.IntPtr* hID, System.Int32 bInitAndVer, System.IntPtr unused1, System.UInt32 propBits, Bid+ApiGroup* pGblFlags, System.IntPtr unused2, System.IntPtr unused3, System.IntPtr unused4, System.IntPtr unused5)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.DllBidEntryPoint' has not been implemented!");
    //    }

    //    public static System.Void DllBidInitialize()
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.DllBidInitialize' has not been implemented!");
    //    }

    //    public static System.Void DllBidFinalize()
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.DllBidFinalize' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void PutStr(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String str)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.PutStr' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String strConst)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.String a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void ScopeLeave(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeLeave' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String strConst)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void TraceBin(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Byte[] buff, System.UInt32 len)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.TraceBin' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.String a3, System.String a4, System.Int32 a5)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.Boolean a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int64 a3, System.UInt32 a4, System.Int32 a5, System.UInt32 a6, System.UInt32 a7)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.String a1, System.String a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.String a1, System.String a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.Int32 a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Boolean a2, System.Int32 a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.String a1, System.String a2, System.String a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.String a3, System.Int32 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.IntPtr a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Boolean a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.String fmtPrintfW2, System.Int32 a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.IntPtr a2, System.IntPtr a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.IntPtr a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.String a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.Int32 a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.String a3, System.Int32 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3, System.String a4, System.String a5, System.Int32 a6)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Boolean a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.String a3, System.String a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Boolean a1, System.String a2, System.String a3, System.String a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3, System.Int32 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Boolean a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3, System.Int32 a4, System.Int32 a5, System.Int32 a6, System.Int32 a7)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.Int32 a3, System.Int32 a4, System.Boolean a5)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int64 a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int64 a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW1, System.String fmtPrintfW2, System.String fmtPrintfW3, System.Int64 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.String a3, System.String a4, System.Int32 a5, System.Int64 a6)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int64 a2, System.Int32 a3, System.Int32 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int64 a3, System.Int32 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3, System.Int32 a4, System.String a5, System.String a6, System.String a7, System.Int32 a8)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void Trace(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.String a3, System.String a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.Trace' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.String a1)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.String a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Boolean a2)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.String a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.String a2, System.Boolean a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Boolean a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3, System.String a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Int32 a3)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }

    //    public static System.Void ScopeEnter(System.IntPtr hID, System.UIntPtr src, System.UIntPtr info, System.IntPtr* hScp, System.String fmtPrintfW, System.Int32 a1, System.Int32 a2, System.Boolean a3, System.Int32 a4)
    //    {
    //        throw new System.NotImplementedException("Method 'Bid+NativeMethods.ScopeEnter' has not been implemented!");
    //    }
    //}
}

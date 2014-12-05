//using System;
//using System.Collections.Generic;
//using IO = System.IO;
//using System.Linq;
//using System.Text;
//using Cosmos.IL2CPU.Plugs;

//namespace Cosmos.System.Plugs.System.IO {
//  [Plug(Target = typeof(IO::FileStream))]
//  public class FileStreamImpl {

//            //[PlugField(FieldId = "$$Storage$$", FieldType = typeof(char[]))]
//            //[PlugField(FieldId = "System.Char System.String.m_firstChar", IsExternalValue = true)]
//            //public static class StringImpl {
//            //  //[PlugMethod(Signature = "System_Void__System_String__ctor_System_Char____System_Int32__System_Int32_")]
//            //  public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength,
//            //    [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
//            //    [FieldAccess(Name = "System.Char System.String.m_firstChar")] ref char* aFirstChar) {
//            //    Char[] newChars = new Char[aLength];
//            //    Array.Copy(aChars, aStartIndex, newChars, 0, aLength);
//            //    aStorage = newChars;
//            //    aStringLength = newChars.Length;
//            //    fixed (char* xFirstChar = &aStorage[0]) {
//            //      aFirstChar = xFirstChar;
//            //    }
//            //  }

//    static public void Ctor(IO::FileStream aThis, string aPathname, IO::FileMode aMode) {
//    }

//    static public void CCtor() {
//      // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
//    }

//    static public int Read(IO::FileStream aThis, byte[] aBuffer, int aOffset, int aCount) {
//      return 0;
//    }

//    //static void Init(IO::FileStream aThis, string path, IO::FileMode mode, IO::FileAccess access, int rights, bool useRights, IO::FileShare share, int bufferSize
//    //  , IO::FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }

//  }
//}

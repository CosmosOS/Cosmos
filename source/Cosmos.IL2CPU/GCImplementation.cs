#if DEBUG
//#define GC_DEBUG
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Cosmos.Common;

namespace Cosmos.IL2CPU {
    [DebuggerStepThrough]
	public static class GCImplementation {
		private static void AcquireLock() {
//			do {
//			} while (Interlocked.CompareExchange(ref mLock, 1, 0) != 0);
            throw new NotImplementedException();
		}

		private static void ReleaseLock() {
//			do {
//			} while (Interlocked.CompareExchange(ref mLock, 0, 1) != 1);
            throw new NotImplementedException();
        }

		public static uint AllocNewObject(uint aSize) {
//			uint xNewObject = RuntimeEngine.Heap_AllocNewObject(aSize + 4);
//#if GC_DEBUG
//			Console.Write("New Object allocated: ");
//			NumberHelper.WriteNumber(xNewObject + 4, false);
//			Console.WriteLine();
//#endif
//			return xNewObject + 4;
            throw new NotImplementedException();

		}

		/// <summary>
		/// This function gets the pointer to the memory location of where it's stored. 
		/// </summary>
		/// <param name="aObject"></param>
		public static unsafe void IncRefCount(uint aObject) {
////			if (aObject == 0) {
////				return;
////			}
////			uint* xTheObject = (uint*)aObject;
////			xTheObject += 1;
////			if ((*xTheObject & 0x80000000) != 0) {
////				return;
////			}
////			xTheObject -= 2;
////			if ((*xTheObject & 0x88888888) != 0) {
////				Console.Write("StaleObject: ");
////				NumberHelper.WriteNumber(aObject, false);
////				Console.WriteLine();
////				return;
////			}
//			//xTheObject -= 2;
//#if GC_DEBUG
//			uint xCount =
//#endif
//// *xTheObject = *xTheObject + 1;
//#if GC_DEBUG
//			if (xCount == 0x80000000) {
//				Console.WriteLine("GC: RefCount Maximum Exceeded!");
//				return;
//			}
//			Console.Write("ObjectIncRefCount, Object = ");
//			NumberHelper.WriteNumber(aObject, false);
//			Console.Write(", RefCount = ");
//			NumberHelper.WriteNumber(xCount, false);
//			Console.WriteLine();
//#endif
            //throw new NotImplementedException();

		}

		/// <summary>
		/// This function gets the pointer to the memory location of where it's stored. 
		/// </summary>
		/// <param name="aObject"></param>
		public static unsafe void DecRefCount(uint aObject) {
      ////			if (aObject == 0) {
      ////				return;
      ////			}
      ////			uint* xTheObject = (uint*)aObject;
      ////			xTheObject += 1;
      //			// check for staticly embedded arrays/objects. these are embedded as values in the binary target, and should
      //			// therefore not be collected.
      ////			if ((*xTheObject & 0x80000000) == 0x80000000) {
      ////				return;
      ////			}
      ////			xTheObject -= 2;
      ////			uint xCount = *xTheObject;
      ////			if (xCount == 0) {
      ////				return;
      ////			}
      //#if GC_DEBUG
      //			if ((xCount & 0x80000000) == 0x80000000) {
      //				Console.Write("StaleObject: ");
      //				WriteNumber(aObject, false);
      //				Console.WriteLine();
      //				return;
      //			}
      //#endif
      ////			xCount = *xTheObject = xCount - 1;
      ////			if (xCount == 0) {
      //#if GC_DEBUG
      //				Console.Write("ObjectReclaimed, Object = ");
      //				NumberHelper.WriteNumber(aObject, false);
      //				Console.WriteLine();
      //				xTheObject = (uint*)(aObject - 4);
      //				*xTheObject = 0x80000000;
      //				xTheObject = (uint*)(aObject + 4);
      //				//				uint xObjectType = *xTheObject;
      //				//				if (xObjectType == 1) {
      //				//					xTheObject = (uint*)aObject + 8;
      //				//					uint xFieldCount = *xTheObject;
      //				//					for (uint i = 0; i < xFieldCount; i++) {
      //				//						xTheObject += 4;
      //				//						DecRefCount(*xTheObject);
      //				//					}
      //				//				}
      //#endif
      //#if !GC_DEBUG
      ////				RuntimeEngine.Heap_Free(aObject - 4);
      //#endif
      //#if GC_DEBUG
      //			} else {
      //				Console.Write("ObjectDecRefCount, Object = ");
      //				NumberHelper.WriteNumber(aObject, false);
      //				Console.Write(", RefCount = ");
      //				NumberHelper.WriteNumber(xCount, false);
      //				Console.WriteLine();
      //#endif
      ////			}
      ///     
      //throw new NotImplementedException();
    }
  }
}
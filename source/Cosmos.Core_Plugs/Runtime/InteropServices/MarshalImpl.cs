﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.InteropServices
{
    [Plug("System.Runtime.InteropServices.Marshal, System.Private.CoreLib")]
    class MarshalImpl
    {
        public static void FreeHGlobal(IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        public static Exception GetExceptionForHR(int aInt, IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        public static void SetLastWin32Error(int aInt)
        {
            throw new NotImplementedException();
        }

        public static int SizeOfHelper(Type aType, bool aBool)
        {
            throw new NotImplementedException();
        }

        public static bool IsPinnable(object aObject)
        {
            throw new NotImplementedException();
        }
    }
}

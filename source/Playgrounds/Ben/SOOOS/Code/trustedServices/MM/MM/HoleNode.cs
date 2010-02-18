using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MM
{

    internal unsafe struct HoleNode
    {
        internal const int Size = 20; // TODO needs to be correct for 32 and 64 bit pointers. We just waste 32 bit storage
        internal UIntPtr MemAddress;
        internal uint Length; //bytes
        HoleNode* next;

        internal HoleNode* Next
        {
            get { return next; }
            set {
                if (value != null && value < ( HoleNode*) 10000)
                    Debug.WriteLine("error");

                
                next = value; 
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU
{
    public class MethodInfo
    {
        public enum TypeEnum { Normal, Plug, NeedsPlug };

        public readonly MethodBase MethodBase;
        public readonly TypeEnum Type;
        public readonly UInt32 UID;
        public readonly MethodInfo PlugMethod;
        public readonly Type MethodAssembler;
        public readonly bool IsInlineAssembler = false;
        public readonly bool DebugStubOff;
        public MethodInfo PluggedMethod;
        public uint LocalVariablesSize;

        public MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, MethodInfo aPlugMethod, Type aMethodAssembler) : this(aMethodBase, aUID, aType, aPlugMethod, false)
        {
            MethodAssembler = aMethodAssembler;
        }


        public MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, MethodInfo aPlugMethod)
            : this(aMethodBase, aUID, aType, aPlugMethod, false)
        {
            //MethodBase = aMethodBase;
            //UID = aUID;
            //Type = aType;
            //PlugMethod = aPlugMethod;
        }

        public MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, MethodInfo aPlugMethod, bool isInlineAssembler)
        {
            MethodBase = aMethodBase;
            UID = aUID;
            Type = aType;
            PlugMethod = aPlugMethod;
            IsInlineAssembler = isInlineAssembler;

            Object[] attribs = this.MethodBase.GetCustomAttributes(typeof(Cosmos.IL2CPU.Plugs.DebugStubAttribute), false);
            if (attribs.Length > 0)
            {
                Cosmos.IL2CPU.Plugs.DebugStubAttribute attrib = attribs[0] as Cosmos.IL2CPU.Plugs.DebugStubAttribute;
                DebugStubOff = attrib.Off;
            }
        }

        public bool IsWildcard
        {
            get;
            set;
        }
    }
}

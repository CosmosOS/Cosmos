using System;
using System.Linq;
using System.Reflection;
using Cosmos.IL2CPU.API;

namespace Cosmos.IL2CPU
{
    public class _MethodInfo
    {
        public enum TypeEnum { Normal, Plug, NeedsPlug };

        public readonly MethodBase MethodBase;
        public readonly TypeEnum Type;
        public readonly UInt32 UID;
        public readonly _MethodInfo PlugMethod;
        public readonly Type MethodAssembler;
        public readonly bool IsInlineAssembler = false;
        public readonly bool DebugStubOff;
        public _MethodInfo PluggedMethod;
        public uint LocalVariablesSize;

        public _MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, _MethodInfo aPlugMethod, Type aMethodAssembler) : this(aMethodBase, aUID, aType, aPlugMethod, false)
        {
            MethodAssembler = aMethodAssembler;
        }


        public _MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, _MethodInfo aPlugMethod)
            : this(aMethodBase, aUID, aType, aPlugMethod, false)
        {
            //MethodBase = aMethodBase;
            //UID = aUID;
            //Type = aType;
            //PlugMethod = aPlugMethod;
        }

        public _MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, _MethodInfo aPlugMethod, bool isInlineAssembler)
        {
            MethodBase = aMethodBase;
            UID = aUID;
            Type = aType;
            PlugMethod = aPlugMethod;
            IsInlineAssembler = isInlineAssembler;

            var attribs = aMethodBase.GetCustomAttributes<DebugStubAttribute>(false).ToList();
            if (attribs.Any())
            {
                DebugStubAttribute attrib = new DebugStubAttribute
                                            {
                                                Off = attribs[0].Off,
                                            };
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

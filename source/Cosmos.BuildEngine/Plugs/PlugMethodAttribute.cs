using System;
using System.Reflection;

namespace Cosmos.BuildEngine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PlugMethodAttribute : Attribute
    {
        public enum PlugImplType : byte
        {
            CompiledCIL,
            EmittedCIL,
            Assembler
        }

        public readonly String TargetName;
        public readonly PlugImplType ImplType;
        public readonly CILPlugOverloader CILOverloader;
        public readonly ASMPlugOverloader ASMOverloader;

        public PlugMethodAttribute(String target)
        {
            TargetName = target;
            ImplType = PlugImplType.CompiledCIL;

            CILOverloader = null;
            ASMOverloader = null;
        }
        public PlugMethodAttribute(String target, CILPlugOverloader overloader)
        {
            TargetName = target;
            ImplType = PlugImplType.EmittedCIL;

            CILOverloader = overloader;
            ASMOverloader = null;
        }
        public PlugMethodAttribute(String target, ASMPlugOverloader overloader)
        {
            TargetName = target;
            ImplType = PlugImplType.Assembler;

            CILOverloader = null;
            ASMOverloader = overloader;
        }

        public PlugMethodAttribute(MethodBase target)
            : this(PlugUtils.ResolveFullName(target)) { }
        public PlugMethodAttribute(MethodBase target, CILPlugOverloader overloader)
            : this(PlugUtils.ResolveFullName(target), overloader) { }
        public PlugMethodAttribute(MethodBase target, ASMPlugOverloader overloader)
            : this(PlugUtils.ResolveFullName(target), overloader) { }
    }
}
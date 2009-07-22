using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.Compiler.IL
{
    public static class X86Util
    {
        public static Func<Op>[] GetInstructionCreatorArray()
        {
            var xResult = new Func<Op>[0xFE1F];
            foreach(var xType in typeof(X86Util).Assembly.GetExportedTypes())
            {
                if(xType.Namespace != "Cosmos.Compiler.IL.X86")
                {
                    continue;
                }
                if(!xType.IsSubclassOf(typeof(Op)))
                {
                    continue;
                }
                var xAttrib =
                    xType.GetCustomAttributes(typeof (OpCodeAttribute), false).FirstOrDefault() as OpCodeAttribute;
                if(xAttrib != null)
                {
                    //var xMethod = xCreater.DefineMethod("Create_" + xAttrib.OpCode + "_Obj", MethodAttributes.Public |MethodAttributes.Static);
                    //xMethod.SetReturnType(typeof (Op));
                    //xxMethod.GetILGenerator();
                    var xTemp = new DynamicMethod("Create_" + xAttrib.OpCode + "_Obj", typeof (IL.Op), new Type[0], true);
                    var xGen = xTemp.GetILGenerator();
                    var xCtor = xType.GetConstructor(new Type[0]);
                    xGen.Emit(OpCodes.Newobj, xCtor);
                    xGen.Emit(OpCodes.Ret);
                    xResult[(ushort) xAttrib.OpCode] = (Func<Op>)xTemp.CreateDelegate(typeof (Func<Op>));
                }
            }
            return xResult;
        }
    }
}
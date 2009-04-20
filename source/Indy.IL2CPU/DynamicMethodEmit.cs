using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Indy.IL2CPU
{
    public class DynamicMethodEmit
    {
        public abstract class CEmittedMethodInfo
        {
            public abstract bool GetHandlesMethod(MethodBase method);
            public abstract MethodInfo GetHandlerForMethod(MethodBase method);
        }
        public abstract class CMethodEmitterInfo
        {
            public abstract bool GetEmittsMethod(MethodBase method);
            public abstract MethodInfo EmitMethod(MethodBase method);
        }

        public class MAMGroup : CEmittedMethodInfo
        {
            public Type arrayType;
            public int Ranks;

            public MethodInfo mbctor;
            public MethodInfo mbget;
            public MethodInfo mbset;

            public MAMGroup(Type a, int r, MethodInfo c, MethodInfo g, MethodInfo s)
            {
                arrayType = a;
                Ranks = r;
                mbctor = c;
                mbget = g;
                mbset = s;
            }

            public override bool GetHandlesMethod(MethodBase method)
            {
                return (
                    (method.DeclaringType.FullName.Split('[')[0] == arrayType.FullName) &&
                    (method.DeclaringType.GetArrayRank() == Ranks)
                );
            }

            public override MethodInfo GetHandlerForMethod(MethodBase method)
            {
                switch (method.Name)
                {
                    case ".ctor":
                        return mbctor;
                    case "Get":
                        return mbget;
                    case "Set":
                        return mbset;
                    default:
                        throw new NotImplementedException("MultiArray method '" + method.Name + "' is not defined!");
                }
            }
        }
        public class MAMEmitter : CMethodEmitterInfo
        {
            private static void EmitRecursiveFillArray(int ranknum, int maxranks, Type arrayType, ILGenerator EmitIL)
            {
                byte lCPA = (byte)(ranknum * 2);
                byte lCRN = (byte)(lCPA + 1);
                byte lCAP = (byte)(lCPA + 2);

                EmitIL.Emit(OpCodes.Ldarg_S, (byte)(ranknum + 1));
                EmitIL.Emit(OpCodes.Stloc_S, lCRN);

                Label Lb_CreateArray = EmitIL.DefineLabel();
                EmitIL.MarkLabel(Lb_CreateArray);
                EmitIL.Emit(OpCodes.Ldloc_S, lCRN);
                EmitIL.Emit(OpCodes.Ldc_I4_1);
                EmitIL.Emit(OpCodes.Sub);
                EmitIL.Emit(OpCodes.Stloc_S, lCRN);
                EmitIL.Emit(OpCodes.Ldarg_S, (byte)(ranknum + 2));
                if (ranknum == (maxranks - 1))
                {
                    EmitIL.Emit(OpCodes.Newarr, arrayType);
                }
                else
                {
                    EmitIL.Emit(OpCodes.Newarr, typeof(int[]));
                }
                EmitIL.Emit(OpCodes.Stloc_S, lCAP);
                EmitIL.Emit(OpCodes.Ldloc_S, lCPA);
                EmitIL.Emit(OpCodes.Ldloc_S, lCRN);
                EmitIL.Emit(OpCodes.Ldloc_S, lCAP);
                EmitIL.Emit(OpCodes.Stelem, typeof(int[]));
                if (ranknum < (maxranks - 1))
                    EmitRecursiveFillArray(ranknum + 1, maxranks, arrayType, EmitIL);
                EmitIL.Emit(OpCodes.Ldloc_S, lCRN);
                EmitIL.Emit(OpCodes.Brtrue, Lb_CreateArray);
            }
            private static void EmitWalkArrays(int Ranks, ILGenerator EmitIL)
            {
                EmitIL.Emit(OpCodes.Ldarg_0);

                for (int i = 1;i < Ranks;i++)
                {
                    EmitIL.Emit(OpCodes.Ldarg_S, (byte)i);
                    EmitIL.Emit(OpCodes.Ldelem, typeof(int[]));
                }

                EmitIL.Emit(OpCodes.Ldarg_S, (byte)Ranks);
            }

            public static void Emit_MultiArray_Ctor(Type arrayType, int Ranks)
            {
                String MethodName = "Emit" + (Instance.EmitCount++);

                MethodBuilder EmitMeth = Instance.EmitContType.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);

                Type[] MethodParams = new Type[Ranks + 1];
                MethodParams[0] = arrayType.MakeArrayType(Ranks);
                for (int i = 1;i <= Ranks;i++)
                    MethodParams[i] = typeof(int);
                EmitMeth.SetParameters(MethodParams);

                ILGenerator EmitIL = EmitMeth.GetILGenerator();
                for (int i = 1;i < Ranks;i++)
                {
                    EmitIL.DeclareLocal(typeof(int[]));
                    EmitIL.DeclareLocal(typeof(int));
                }
                EmitIL.DeclareLocal(typeof(int[]));

                EmitIL.Emit(OpCodes.Ldarg_0);
                EmitIL.Emit(OpCodes.Stloc_0);

                EmitRecursiveFillArray(0, Ranks - 1, arrayType, EmitIL);

                EmitIL.Emit(OpCodes.Ret);
            }
            public static void Emit_MultiArray_Get(Type arrayType, int Ranks)
            {
                String MethodName = "Emit" + (Instance.EmitCount++);

                MethodBuilder EmitMeth = Instance.EmitContType.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);

                Type[] MethodParams = new Type[Ranks + 1];
                MethodParams[0] = arrayType.MakeArrayType(Ranks);
                for (int i = 1;i <= Ranks;i++)
                    MethodParams[i] = typeof(int);
                EmitMeth.SetParameters(MethodParams);
                EmitMeth.SetReturnType(arrayType);

                ILGenerator EmitIL = EmitMeth.GetILGenerator();

                EmitWalkArrays(Ranks, EmitIL);

                EmitIL.Emit(OpCodes.Ldelem, arrayType);
                EmitIL.Emit(OpCodes.Ret);
            }
            public static void Emit_MultiArray_Set(Type arrayType, int Ranks)
            {
                String MethodName = "Emit" + (Instance.EmitCount++);

                MethodBuilder EmitMeth = Instance.EmitContType.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);

                Type[] MethodParams = new Type[Ranks + 2];
                MethodParams[0] = arrayType.MakeArrayType(Ranks);
                for (int i = 1;i <= Ranks;i++)
                    MethodParams[i] = typeof(int);
                MethodParams[Ranks + 1] = arrayType;
                EmitMeth.SetParameters(MethodParams);

                ILGenerator EmitIL = EmitMeth.GetILGenerator();

                EmitWalkArrays(Ranks, EmitIL);

                EmitIL.Emit(OpCodes.Ldarg_S, (byte)(Ranks + 1));
                EmitIL.Emit(OpCodes.Stelem, arrayType);
                EmitIL.Emit(OpCodes.Ret);
            }

            public override bool GetEmittsMethod(MethodBase method)
            {
                return (
                    (method.DeclaringType.IsArray) &&
                    (method.DeclaringType.GetArrayRank() > 1)
                );
            }

            public override MethodInfo EmitMethod(MethodBase method)
            {
                Type arrayType = Type.GetType(method.DeclaringType.FullName.Split('[')[0]);
                int Ranks = method.DeclaringType.GetArrayRank();

                BeginType();
                Emit_MultiArray_Ctor(arrayType, Ranks);
                Emit_MultiArray_Get(arrayType, Ranks);
                Emit_MultiArray_Set(arrayType, Ranks);
                MethodInfo[] mbs = EndType();

                Instance.CEMIs.Add(new MAMGroup(arrayType, Ranks, mbs[0], mbs[1], mbs[2]));

                return GetDynamicMethod(method);
            }
        }

        public static DynamicMethodEmit Instance = new DynamicMethodEmit(AppDomain.CurrentDomain);

        public readonly AppDomain EmitDomain;
        public readonly AssemblyBuilder EmitAssm;
        public readonly ModuleBuilder EmitModu;
        public TypeBuilder EmitContType;
        private int EmitCount = 0;
        private int TypeCount = 0;
        List<CEmittedMethodInfo> CEMIs = new List<CEmittedMethodInfo>();
        List<CMethodEmitterInfo> CMEIs = new List<CMethodEmitterInfo>();

        public DynamicMethodEmit(AppDomain aEmitDomain)
        {
            EmitDomain = aEmitDomain;
            EmitAssm = EmitDomain.DefineDynamicAssembly(new AssemblyName() { Name = "IndyIL2CPU_EmitAssm" }, AssemblyBuilderAccess.RunAndSave);
            EmitModu = EmitAssm.DefineDynamicModule("IndyIL2CPU_EmitAssm");

            CMEIs.Add(new MAMEmitter());

            Instance = this;
        }

        public static void BeginType()
        {
            Instance.EmitContType = Instance.EmitModu.DefineType(
                "Indy.IL2CPU.MultiArrayEmit.ContType" + (Instance.TypeCount++),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit
            );
            Instance.EmitCount = 0;
        }
        public static MethodInfo[] EndType()
        {
            Type t = Instance.EmitContType.CreateType();

            MethodInfo[] mbs = new MethodInfo[Instance.EmitCount];
            for (int i = 0;i < Instance.EmitCount;i++)
                mbs[i] = t.GetMethod("Emit" + i);

            return mbs;
        }

        public static bool GetHasDynamicMethod(MethodBase method)
        {
            foreach (CMethodEmitterInfo cmei in Instance.CMEIs)
            {
                if (cmei.GetEmittsMethod(method))
                    return true;
            }

            return false;
        }
        public static MethodInfo GetDynamicMethod(MethodBase method)
        {
            foreach (CEmittedMethodInfo cemi in Instance.CEMIs)
            {
                if (cemi.GetHandlesMethod(method))
                {
                    return cemi.GetHandlerForMethod(method);
                }
            }
            foreach (CMethodEmitterInfo cmei in Instance.CMEIs)
            {
                if (cmei.GetEmittsMethod(method))
                {
                    return cmei.EmitMethod(method);
                }
            }

            throw new NotImplementedException();
        }
    }
}

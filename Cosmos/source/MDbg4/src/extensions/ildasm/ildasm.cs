//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Reflection;
using System.Threading;
using System.Diagnostics.SymbolStore;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Security.Permissions;
using System.Globalization;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorMetadata;

//////////////////////////////////////////////////////////////////////////////////
//
// DIAGnostic extension for MDbg
//
//////////////////////////////////////////////////////////////////////////////////

[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
//[assembly:CLSCompliant(true)]

namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    public abstract class IldasmExtension : CommandBase
    {
        public static void LoadExtension()
        {
            MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands, typeof(IldasmExtension));
            WriteOutput("Extension ildasm loaded");
            WriteOutput("ildasm command loaded. For help type 'help'");
        }

        public static void UnloadExtension()
        {
            MDbgAttributeDefinedCommand.RemoveCommandsFromType(Shell.Commands, typeof(IldasmExtension));
            //WriteOutput("Extension ildasm unloaded");
        }

        [
         CommandDescription(
           CommandName = "ildasm",
           MinimumAbbrev = 3,
           ShortHelp = "il disassembly",
           LongHelp =
             "Usage: ildasm"
          )
         ]
        public static void ILDasmCmd(string args)
        {
            // Provides disassembly of IL opcodes.

            ArgParser ap = new ArgParser(args);
            if (ap.Count > 1)
            {
                WriteError("Wrong # of arguments.");
                return;
            }

            MDbgFrame functionFrame = null;
            MDbgFunction function = null;
            if (ap.Exists(0))
            {
                function = Debugger.Processes.Active.ResolveFunctionNameFromScope(ap.AsString(0));
                if (function == null)
                    throw new MDbgShellException("no such function.");
            }
            else
            {
                functionFrame = Debugger.Processes.Active.Threads.Active.CurrentFrame;
                function = functionFrame.Function;
            }


            CorCode ilCode = function.CorFunction.ILCode;
            //CorMetadataImport importer = functionFrame.Function.Module.Importer;
            Debug.Assert(true == ilCode.IsIL);

            WriteOutput("code size: " + ilCode.Size);

            ILVirtualDocument doc = new ILVirtualDocument(function);

            int currentLine;
            if (functionFrame != null)
            {
                // we have frame and therefore we should show current location
                uint ip;
                CorDebugMappingResult mappingResult;
                functionFrame.CorFrame.GetIP(out ip, out mappingResult);
                WriteOutput("current IL-IP: " + ip);
                WriteOutput("mapping: " + mappingResult.ToString());
                WriteOutput("URL: " + doc.Path);
                currentLine = doc.Ip2LineNo((int)ip);
            }
            else
            {
                // no current IP.
                currentLine = -1;
            }
            for (int i = 0; i < doc.Count; i++)
                if (i == currentLine)
                    WriteOutput("*  " + doc[i]);
                else
                    WriteOutput("   " + doc[i]);
        }

        [
         CommandDescription(
           CommandName = "inext",
           ShortHelp = "inext",
           MinimumAbbrev = 2,
           LongHelp =
             "Usage: inext"
          )
         ]
        public static void INextCmd(string args)
        {
            Debugger.Processes.Active.StepOver(true).WaitOne();
            ILDasmCmd("");
        }

        [
         CommandDescription(
                            CommandName = "istep",
                            ShortHelp = "istep",
                            MinimumAbbrev = 2,
                            LongHelp =
                            "Usage: istep"
                            )
         ]
        public static void IStepCmd(string args)
        {
            Debugger.Processes.Active.StepInto(true).WaitOne();
            ILDasmCmd("");
        }

    }


    public class ILVirtualDocument : IMDbgSourceFile
    {
        public ILVirtualDocument(MDbgFunction mdbgFunction)
        {
            CorCode ilCode = mdbgFunction.CorFunction.ILCode;
            Debug.Assert(true == ilCode.IsIL);
            byte[] code = ilCode.GetCode();
            ILDisassembler.Disassemble(code, mdbgFunction.Module.Importer, out m_lines, out ip2lineMapping);
            Debug.Assert(m_lines != null && ip2lineMapping != null);

            m_functionName = mdbgFunction.FullName;
        }

        // implementation of MDbgSourceFile
        public string Path
        {
            get
            {
                return m_functionName;
            }
        }

        /// <include file='doc\MDbgExt.uex' path='docs/doc[@for="MDbgSourceFile.this"]/*' />
        public string this[int lineNo]
        {
            get
            {
                Debug.Assert(m_lines != null);
                return m_lines[lineNo];
            }
        }

        /// <include file='doc\MDbgExt.uex' path='docs/doc[@for="MDbgSourceFile.Count"]/*' />
        public int Count
        {
            get
            {
                Debug.Assert(m_lines != null);
                return m_lines.Length;
            }
        }

        // implementation of mapping
        public int Ip2LineNo(int ip)
        {
            return ip2lineMapping[ip];
        }

        private string m_functionName;
        private string[] m_lines;
        private int[] ip2lineMapping;
    }


    public class ILDisassembler
    {
        public static void Disassemble(byte[] ilCode, CorMetadataImport importer, out string[] lines, out int[] ip2line)
        {
            ArrayList ils = new ArrayList();
            ip2line = new int[ilCode.Length];
            int pc = 0;
            while (pc < ilCode.Length)
            {
                string instruction = "";
                int instruction_start = pc;

                int opCodeSize;
                ILOpCode opCode = DecodeOpcode(ilCode, pc, out opCodeSize);
                pc += opCodeSize;
                switch ((OpcodeFormat)GenTables.opCodeTypeInfo[(int)opCode].Type)
                {
                    default:
                        Debug.Assert(false);
                        break;

                    case OpcodeFormat.InlineNone:
                        instruction = GenTables.opCodeTypeInfo[(int)opCode].Name;
                        break;

                    case OpcodeFormat.ShortInlineI:
                    case OpcodeFormat.ShortInlineVar:
                        {
                            byte arg = ilCode[pc];
                            pc++;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.InlineVar:
                        {
                            Int16 arg = BitConverter.ToInt16(ilCode, pc);
                            pc += 2;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.InlineI:
                    case OpcodeFormat.InlineRVA:
                        {
                            Int32 arg = BitConverter.ToInt32(ilCode, pc);
                            pc += 4;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.InlineI8:
                        {
                            Int64 arg = BitConverter.ToInt64(ilCode, pc);
                            pc += 8;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.ShortInlineR:
                        {
                            float arg = BitConverter.ToSingle(ilCode, pc);
                            pc += 4;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.InlineR:
                        {
                            double arg = BitConverter.ToDouble(ilCode, pc);
                            pc += 8;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.ShortInlineBrTarget:
                        {
                            sbyte offset = (sbyte)ilCode[pc];
                            pc++;
                            int dest = pc + offset;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} IL_{1,-4:X}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                                    dest});
                            break;
                        }
                    case OpcodeFormat.InlineBrTarget:
                        {
                            Int32 offset = BitConverter.ToInt32(ilCode, pc);
                            pc += 4;
                            int dest = pc + offset;
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} IL_{1,-4:X}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                                    dest});
                            break;
                        }
                    case OpcodeFormat.InlineSwitch:
                    case OpcodeFormat.InlinePhi:
                        instruction = "MESSED UP!";
                        // variable size
                        Debug.Assert(false);
                        break;
                    case OpcodeFormat.InlineString:
                    case OpcodeFormat.InlineField:
                    case OpcodeFormat.InlineType:
                    case OpcodeFormat.InlineToken:
                    case OpcodeFormat.InlineMethod:
                        {
                            int token = BitConverter.ToInt32(ilCode, pc);
                            pc += 4;

                            CorTokenType tokenType = TokenUtils.TypeFromToken(token);
                            // if it is reference token we need to dereference it.
                            string arg = null;
                            switch (tokenType)
                            {
                                default:
                                    Debug.Assert(false);
                                    break;
                                case CorTokenType.mdtTypeDef:
                                    int extendsToken;
                                    arg = importer.GetTypeNameFromDef(token, out extendsToken);
                                    break;
                                case CorTokenType.mdtTypeRef:
                                    arg = importer.GetTypeNameFromRef(token);
                                    break;
                                case CorTokenType.mdtTypeSpec:
                                    arg = "NYI";
                                    break;
                                case CorTokenType.mdtMethodDef:
                                    MethodInfo mi = importer.GetMethodInfo(token);
                                    Type dt = mi.DeclaringType;
                                    arg = (dt == null ? "" : dt.Name) + "." + mi.Name;
                                    break;
                                case CorTokenType.mdtFieldDef:
                                    arg = "NYI";
                                    break;
                                case CorTokenType.mdtMemberRef:
                                    arg = importer.GetMemberRefName(token);
                                    break;
                                case CorTokenType.mdtString:
                                    arg = "\"" + importer.GetUserString(token) + "\"";
                                    break;
                            } // switch(tokenType)
                            instruction = String.Format(CultureInfo.InvariantCulture, "{0} {1}", new Object[]{GenTables.opCodeTypeInfo[(int)opCode].Name,
                                                                            arg});
                            break;
                        }
                    case OpcodeFormat.InlineSig:
                        instruction = GenTables.opCodeTypeInfo[(int)opCode].Name;
                        pc += 4;
                        break;
                } // switch((OpcodeFormat)GenTables.opCodeTypeInfo[(int)opCode].Type)
                ils.Add(String.Format(CultureInfo.InvariantCulture, "IL_{0,-4:X}:  {1}", new Object[] { instruction_start, instruction }));

                // add ip2line mapping
                for (int i = instruction_start; i < pc; i++)
                    ip2line[i] = ils.Count - 1; // last line
            } // while(pc<ilCode.Length)
            lines = (string[])ils.ToArray(typeof(string));
            return;

        }

        public static ILOpCode DecodeOpcode(byte[] ilCode, int pc, out int opCodeSize)
        {
            ILOpCode opCode = (ILOpCode)ilCode[pc];
            opCodeSize = 1;
            switch (opCode)
            {
                case ILOpCode.CEE_PREFIX1:
                    opCode = (ILOpCode)(ilCode[pc + 1] + 256);
                    opCodeSize = 2;
                    if (opCode < 0 || opCode >= ILOpCode.CEE_COUNT)
                        throw new Exception();
                    break;
                case ILOpCode.CEE_PREFIXREF:
                case ILOpCode.CEE_PREFIX2:
                case ILOpCode.CEE_PREFIX3:
                case ILOpCode.CEE_PREFIX4:
                case ILOpCode.CEE_PREFIX5:
                case ILOpCode.CEE_PREFIX6:
                case ILOpCode.CEE_PREFIX7:
                    throw new Exception();
            }
            return opCode;
        }
    }

    public struct OpCodeTypeInfo
    {
        private String m_name;
        private int m_type;
        private int m_len;
        private int m_Std1;
        private int m_Std2;

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public int Type
        {
            get
            {
                return m_type;
            }
        }

        public int Length
        {
            get
            {
                return m_len;
            }
        }

        public int Std1
        {
            get
            {
                return m_Std1;
            }
        }

        public int Std2
        {
            get
            {
                return m_Std2;
            }
        }

        public OpCodeTypeInfo(string name, int type, int len, int Std1, int Std2)
        {
            this.m_name = name;
            this.m_type = type;
            this.m_len = len;
            this.m_Std1 = Std1;
            this.m_Std2 = Std2;
        }

        public static bool operator !=(OpCodeTypeInfo operand, OpCodeTypeInfo operand2)
        {
            return !(operand == operand2);
        }

        public static bool operator ==(OpCodeTypeInfo operand, OpCodeTypeInfo operand2)
        {
            return operand.m_name == operand2.m_name
                && operand.m_type == operand2.m_type
                && operand.m_len == operand2.m_len
                && operand.m_Std1 == operand2.m_Std1
                && operand.m_Std2 == operand2.m_Std2;
        }

        public override int GetHashCode()
        {
            return m_name.GetHashCode();
        }

        public override bool Equals(Object value)
        {
            return (OpCodeTypeInfo)value == this;
        }
    }

    //copied from asmenum.h
    public enum OpcodeFormat
    {
        InlineNone = 0,    // no inline args       
        InlineVar = 1,    // local variable       (U2 (U1 if Short on))
        InlineI = 2,    // an signed integer    (I4 (I1 if Short on))
        InlineR = 3,    // a real number        (R8 (R4 if Short on))
        InlineBrTarget = 4,    // branch target        (I4 (I1 if Short on))
        InlineI8 = 5,
        InlineMethod = 6,   // method token (U4)
        InlineField = 7,   // field token  (U4)
        InlineType = 8,   // type token   (U4)
        InlineString = 9,   // string TOKEN (U4)
        InlineSig = 10,  // signature tok (U4)
        InlineRVA = 11,  // ldptr token  (U4)
        InlineToken = 12,  // a meta-data token of unknown type (U4)
        InlineSwitch = 13,  // count (U4), pcrel1 (U4) .... pcrelN (U4)
        InlinePhi = 14,  // count (U1), var1 (U2) ... varN (U2) 

        // WATCH OUT we are close to the limit here, if you add
        // more enumerations you need to change ShortIline definition below

        // The extended enumeration also encodes the size in the IL stream
        ShortInline = 16,                       // if this bit is set, the format is the 'short' format
        PrimaryMask = (ShortInline - 1),          // mask these off to get primary enumeration above
        ShortInlineVar = (ShortInline + InlineVar),
        ShortInlineI = (ShortInline + InlineI),
        ShortInlineR = (ShortInline + InlineR),
        ShortInlineBrTarget = (ShortInline + InlineBrTarget),
        InlineOpcode = (ShortInline + InlineNone),    // This is only used internally.  It means the 'opcode' is two byte instead of 1
    };

};

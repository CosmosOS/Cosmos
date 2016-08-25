using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Cosmos.Assembler
{
    public class DataMember : BaseAssemblerElement, IComparable<DataMember>
    {
        public string Name { get; private set; }
        public bool IsComment { get; set; }
        public byte[] RawDefaultValue { get; set; }
        public uint Alignment { get; set; }
        protected object[] UntypedDefaultValue;
        public string RawAsm = null;

        // Hack for not to emit raw data. See RawAsm
        public DataMember()
        {
            Name = "Dummy";
        }

        protected DataMember(string aName)
        {
            Name = aName;
        }

        public DataMember(string aName, string aValue)
        {
            Name = aName;
            var xBytes = ASCIIEncoding.ASCII.GetBytes(aValue);
            var xBytes2 = new byte[xBytes.Length + 1];
            xBytes.CopyTo(xBytes2, 0);
            xBytes2[xBytes2.Length - 1] = 0;
            RawDefaultValue = xBytes2;
        }

        public DataMember(string aName, params object[] aDefaultValue)
        {
            Name = aName;
            UntypedDefaultValue = aDefaultValue;
        }

        public DataMember(string aName, byte[] aDefaultValue)
        {
            Name = aName;
            RawDefaultValue = aDefaultValue;
            //UntypedDefaultValue = aDefaultValue;
        }

        // TODO Why not use <Cast> here too and instead to pack the short array into an Int32 array simply emit it with dw?
        public DataMember(string aName, short[] aDefaultValue)
        {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length * 2];
            for (int i = 0; i < aDefaultValue.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                            RawDefaultValue, i * 2, 2);
            }
            //UntypedDefaultValue = aDefaultValue;
        }

        // TODO Why not use <Cast> here too and instead to pack the short array into an Int32 array simply emit it with dw?
        public DataMember(string aName, params ushort[] aDefaultValue)
        {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length * 2];
            for (int i = 0; i < aDefaultValue.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                            RawDefaultValue, i * 2, 2);
            }
            //UntypedDefaultValue = aDefaultValue;
        }

        public DataMember(string aName, params uint[] aDefaultValue)
        {
            Name = aName;
            UntypedDefaultValue = aDefaultValue.Cast<object>().ToArray();
        }

        public DataMember(string aName, params int[] aDefaultValue)
        {
            Name = aName;
            UntypedDefaultValue = aDefaultValue.Cast<object>().ToArray();
        }

        public DataMember(string aName, params ulong[] aDefaultValue)
        {
            Name = aName;
            UntypedDefaultValue = aDefaultValue.Cast<object>().ToArray();
        }

        public DataMember(string aName, params long[] aDefaultValue)
        {
            Name = aName;
            UntypedDefaultValue = aDefaultValue.Cast<object>().ToArray();
        }

        public DataMember(string aName, Stream aData)
        {
            Name = aName;
            RawDefaultValue = new byte[aData.Length];
            aData.Read(RawDefaultValue, 0, RawDefaultValue.Length);
        }

        public static string GetStaticFieldName(FieldInfo aField)
        {
            return FilterStringForIncorrectChars("static_field__" + LabelName.GetFullName(aField.DeclaringType) + "." + aField.Name);
        }

        public const string IllegalIdentifierChars = "&.,+$<>{}-`\'/\\ ()[]*!=";
        public static string FilterStringForIncorrectChars(string aName)
        {
            string xTempResult = aName;
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return String.Intern(xTempResult);
        }

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, TextWriter aOutput)
        {
            if (RawAsm != null)
            {
                aOutput.WriteLine(RawAsm);
                return;
            }

            if (RawDefaultValue != null)
            {
                if (RawDefaultValue.Length == 0)
                {
                    aOutput.Write(Name);
                    aOutput.Write(":");
                    return;
                }
                if ((from item in RawDefaultValue
                     group item by item
                       into i
                     select i).Count() > 1 || RawDefaultValue.Length < 250)
                {
                    if (IsGlobal)
                    {
                        aOutput.Write("global ");
                        aOutput.WriteLine(Name);
                    }
                    aOutput.Write(Name);
                    aOutput.Write(" db ");
                    for (int i = 0; i < (RawDefaultValue.Length - 1); i++)
                    {
                        aOutput.Write(RawDefaultValue[i]);
                        aOutput.Write(", ");
                    }
                    aOutput.Write(RawDefaultValue.Last());
                }
                else
                {
                    //aOutputWriter.WriteLine("TIMES 0x50000 db 0");
                    aOutput.Write("global ");
                    aOutput.WriteLine(Name);
                    aOutput.Write(Name);
                    aOutput.Write(": TIMES ");
                    aOutput.Write(RawDefaultValue.Length);
                    aOutput.Write(" db ");
                    aOutput.Write(RawDefaultValue[0]);
                }
                return;
            }
            if (UntypedDefaultValue != null)
            {
                StringBuilder xSB = new StringBuilder();
                if (IsGlobal)
                {
                    aOutput.Write("global ");
                    aOutput.WriteLine(Name);
                }

                //aOutput.WriteLine("; Type of UntypedDefaultValue is " + UntypedDefaultValue[0].GetType());
                aOutput.Write(Name);

                if (UntypedDefaultValue[0] is Int64 || UntypedDefaultValue[0] is UInt64 || UntypedDefaultValue[0] is Double)
                    aOutput.Write(" dq ");
                else
                    aOutput.Write(" dd ");

                Func<object, string> xGetTextForItem = delegate (object aItem)
                {
                    var xElementRef = aItem as Cosmos.Assembler.ElementReference;
                    if (xElementRef == null)
                    {
                        return (aItem ?? 0).ToString();
                    }
                    else
                    {
                        if (xElementRef.Offset == 0)
                        {
                            return xElementRef.Name;
                        }
                        return xElementRef.Name + " + " + xElementRef.Offset;
                    }
                };
                for (int i = 0; i < (UntypedDefaultValue.Length - 1); i++)
                {
                    aOutput.Write(xGetTextForItem(UntypedDefaultValue[i]));
                    aOutput.Write(", ");
                }
                aOutput.Write(xGetTextForItem(UntypedDefaultValue.Last()));
                return;
            }
            throw new Exception("Situation unsupported!");
        }

        public int CompareTo(DataMember other)
        {
            return String.Compare(Name, other.Name);
        }

        public bool IsGlobal
        {
            get;
            set;
        }

        public override ulong? ActualAddress
        {
            get
            {
                // TODO: for now, we dont have any data alignment
                return StartAddress;
            }
        }

        public override void UpdateAddress(Cosmos.Assembler.Assembler aAssembler, ref ulong xAddress)
        {
            if (Alignment > 0)
            {
                if (xAddress % Alignment != 0)
                {
                    xAddress += Alignment - (xAddress % Alignment);
                }
            }
            base.UpdateAddress(aAssembler, ref xAddress);
            if (RawDefaultValue != null)
            {
                xAddress += (ulong)RawDefaultValue.LongLength;
            }
            if (UntypedDefaultValue != null)
            {
                // TODO: what to do with 64bit target platforms? right now we only support 32bit
                xAddress += (ulong)(UntypedDefaultValue.LongLength * 4);
            }
        }

        public override bool IsComplete(Assembler aAssembler)
        {
            if (RawAsm != null)
            {
                return true;
            }
            if (UntypedDefaultValue != null &&
                UntypedDefaultValue.LongLength > 0)
            {
                foreach (var xReference in (from item in UntypedDefaultValue
                                            let xRef = item as Cosmos.Assembler.ElementReference
                                            where xRef != null
                                            select xRef))
                {
                    var xRef = aAssembler.TryResolveReference(xReference);
                    if (xRef == null)
                    {
                        return false;
                    }
                    else if (!xRef.IsComplete(aAssembler))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void WriteData(Cosmos.Assembler.Assembler aAssembler, Stream aOutput)
        {
            if (UntypedDefaultValue != null &&
                UntypedDefaultValue.LongLength > 0)
            {
                //var xBuff = (byte[])Array.CreateInstance(typeof(byte), UntypedDefaultValue.LongLength * 4);
                for (int i = 0; i < UntypedDefaultValue.Length; i++)
                {
                    var xRef = UntypedDefaultValue[i] as Cosmos.Assembler.ElementReference;
                    //byte[] xTemp;
                    if (xRef != null)
                    {
                        var xTheRef = aAssembler.TryResolveReference(xRef);
                        if (xTheRef == null)
                        {
                            throw new Exception("Reference not found!");
                        }
                        if (!xTheRef.ActualAddress.HasValue)
                        {
                            Console.Write("");
                        }
                        aOutput.Write(BitConverter.GetBytes(xTheRef.ActualAddress.Value), 0, 4);
                        //xTemp = BitConverter.GetBytes();
                    }
                    else
                    {
                        if (UntypedDefaultValue[i] is int)
                        {
                            aOutput.Write(BitConverter.GetBytes((int)UntypedDefaultValue[i]), 0, 4);
                            //xTemp = BitConverter.GetBytes((int)UntypedDefaultValue[i]);
                        }
                        else
                        {
                            if (UntypedDefaultValue[i] is uint)
                            {
                                aOutput.Write(BitConverter.GetBytes((uint)UntypedDefaultValue[i]), 0, 4);

                                //xTemp = BitConverter.GetBytes((uint)UntypedDefaultValue[i]);
                            }
                            else
                            {
                                throw new Exception("Invalid value inside UntypedDefaultValue");
                            }
                        }
                    }
                    //Array.Copy(xTemp, 0, xBuff, i * 4, 4);
                }
            }
            else
            {
                aOutput.Write(RawDefaultValue, 0, RawDefaultValue.Length);
            }
        }
    }
}
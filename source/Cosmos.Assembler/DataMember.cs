using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Cosmos.Assembler.x86;

namespace Cosmos.Assembler
{
    public class DataMember : BaseAssemblerElement, IComparable<DataMember>
    {
        public const string IllegalIdentifierChars = "&.,+$<>{}-`\'/\\ ()[]*!=";

        public string Name { get; }
        public bool IsComment { get; set; }
        public byte[] RawDefaultValue { get; set; }
        public uint Alignment { get; set; }
        public bool IsGlobal { get; set; }
        protected object[] UntypedDefaultValue;
        public string RawAsm = null;
        private string Size;
        private string StringValue;

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
            var xBytes = Encoding.ASCII.GetBytes(aValue);
            var xBytes2 = new byte[xBytes.Length + 1];
            xBytes.CopyTo(xBytes2, 0);
            xBytes2[xBytes2.Length - 1] = 0;
            RawDefaultValue = xBytes2;
        }

        public DataMember(string aName, string size, string aValue)
        {
            Name = aName;
            Size = size;
            StringValue = aValue;
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
        }

        public DataMember(string aName, short[] aDefaultValue)
        {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length*2];
            for (int i = 0; i < aDefaultValue.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0, RawDefaultValue, i*2, 2);
            }
        }

        public DataMember(string aName, params ushort[] aDefaultValue)
        {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length*2];
            for (int i = 0; i < aDefaultValue.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0, RawDefaultValue, i*2, 2);
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

        public static string FilterStringForIncorrectChars(string aName)
        {
            string xTempResult = aName;
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return string.Intern(xTempResult);
        }

        public override void WriteText(Assembler aAssembler, TextWriter aOutput)
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
                if (IsGlobal)
                {
                    aOutput.Write("global ");
                    aOutput.WriteLine(Name);
                }
                aOutput.Write(Name);

                if (UntypedDefaultValue[0] is Int64 || UntypedDefaultValue[0] is UInt64 || UntypedDefaultValue[0] is Double)
                {
                    aOutput.Write(" dq ");
                }
                else
                {
                    aOutput.Write(" dd ");
                }

                Func<object, string> xGetTextForItem = delegate(object aItem)
                {
                    var xElementRef = aItem as ElementReference;
                    if (xElementRef == null)
                    {
                        return (aItem ?? 0).ToString();
                    }

                    if (xElementRef.Offset == 0)
                    {
                        return xElementRef.Name;
                    }
                    return xElementRef.Name + " + " + xElementRef.Offset;
                };
                for (int i = 0; i < (UntypedDefaultValue.Length - 1); i++)
                {
                    aOutput.Write(xGetTextForItem(UntypedDefaultValue[i]));
                    aOutput.Write(", ");
                }
                aOutput.Write(xGetTextForItem(UntypedDefaultValue.Last()));
                return;
            }

            if (StringValue != null)
            {
                aOutput.Write(Name);
                aOutput.Write(" ");
                aOutput.Write(Size);
                aOutput.Write(" ");
                aOutput.Write(StringValue);
                return;
            }

            throw new Exception("Situation unsupported!");
        }

        public int CompareTo(DataMember other)
        {
            return string.Compare(Name, other.Name);
        }

        public override ulong? ActualAddress
        {
            get
            {
                // TODO: for now, we dont have any data alignment
                return StartAddress;
            }
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong xAddress)
        {
            if (Alignment > 0)
            {
                if (xAddress%Alignment != 0)
                {
                    xAddress += Alignment - (xAddress%Alignment);
                }
            }
            base.UpdateAddress(aAssembler, ref xAddress);
            if (RawDefaultValue != null)
            {
                xAddress += (ulong) RawDefaultValue.LongLength;
            }
            if (UntypedDefaultValue != null)
            {
                // TODO: what to do with 64bit target platforms? right now we only support 32bit
                xAddress += (ulong) (UntypedDefaultValue.LongLength*4);
            }
        }

        public override bool IsComplete(Assembler aAssembler)
        {
            if (RawAsm != null)
            {
                return true;
            }

            if (UntypedDefaultValue != null && UntypedDefaultValue.LongLength > 0)
            {
                foreach (var xReference in (from item in UntypedDefaultValue
                    let xRef = item as ElementReference
                    where xRef != null
                    select xRef))
                {
                    var xRef = aAssembler.TryResolveReference(xReference);

                    if (xRef == null)
                    {
                        return false;
                    }

                    if (!xRef.IsComplete(aAssembler))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void WriteData(Assembler aAssembler, Stream aOutput)
        {
            if (UntypedDefaultValue != null && UntypedDefaultValue.LongLength > 0)
            {
                for (int i = 0; i < UntypedDefaultValue.Length; i++)
                {
                    var xRef = UntypedDefaultValue[i] as ElementReference;
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
                    }
                    else
                    {
                        if (UntypedDefaultValue[i] is int)
                        {
                            aOutput.Write(BitConverter.GetBytes((int) UntypedDefaultValue[i]), 0, 4);
                        }
                        else
                        {
                            if (UntypedDefaultValue[i] is uint)
                            {
                                aOutput.Write(BitConverter.GetBytes((uint) UntypedDefaultValue[i]), 0, 4);
                            }
                            else
                            {
                                throw new Exception("Invalid value inside UntypedDefaultValue");
                            }
                        }
                    }
                }
            }
            else
            {
                aOutput.Write(RawDefaultValue, 0, RawDefaultValue.Length);
            }
        }
    }
}

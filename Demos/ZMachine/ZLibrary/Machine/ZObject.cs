using Cosmos.Common.Extensions;
using ZLibrary.Constants;
namespace ZLibrary.Machine
{
    // TODO: Create an object class instead of these extensions?
    public static class ZObject
    {
        public const ushort MAX_OBJECT = 2000;
        
        public const ushort O1_PARENT = 4;
        public const ushort O1_SIBLING = 5;
        public const ushort O1_CHILD = 6;
        public const ushort O1_PROPERTY_OFFSET = 7;
        public const ushort O1_SIZE = 9;
        
        public const ushort O4_PARENT = 6;
        public const ushort O4_SIBLING = 8;
        public const ushort O4_CHILD = 10;
        public const ushort O4_PROPERTY_OFFSET = 12;
        public const ushort O4_SIZE = 14;

        public static string GetObjectName(this ZMachine aMachine, ushort aObject)
        {
            ushort objAddress = aMachine.GetObjectAddress(aObject);

            if (aMachine.Story.Header.Version <= FileVersion.V3)
            {
                objAddress += O1_PROPERTY_OFFSET;
            }
            else
            {
                objAddress += O4_PROPERTY_OFFSET;
            }

            aMachine.Memory.GetWord(objAddress, out ushort nameAddress);

            return ZText.DecodeString((ushort)(nameAddress + 1));
        }

        public static ushort GetObjectAddress(this ZMachine aMachine, ushort aObject)
        {
            if (aObject > ((aMachine.Story.Header.Version <= FileVersion.V3) ? 255 : MAX_OBJECT))
            {
                aMachine.Output.PrintString("@Attempt to address illegal object ");
                aMachine.Output.PrintString(aObject.ToHex());
                aMachine.Output.PrintString(".  This is normally fatal.");
                aMachine.Output.PrintZSCII(13);
            }

            if (aMachine.Story.Header.Version <= FileVersion.V3)
            {
                return (ushort) (aMachine.Story.Header.ObjectsOffset + ((aObject - 1) * O1_SIZE + 62));
            }

            return (ushort) (aMachine.Story.Header.ObjectsOffset + ((aObject - 1) * O4_SIZE + 126));
        }

        public static ushort GetFirstProperty(this ZMachine aMachine, ushort aObject)
        {
            ushort xObjectAddress = aMachine.GetObjectAddress(aObject);
            aMachine.Memory.GetWord(xObjectAddress + 7, out ushort propTable);
            aMachine.Memory.GetByte(propTable, out byte nameSize);
            propTable += (ushort)(2 * nameSize + 1);
            return propTable;
        }

        public static ushort GetNextProperty(this ZMachine aMachine, ushort aProperty)
        {
            aMachine.Memory.GetByte(aProperty, out byte value);
            aProperty++;

            if (aMachine.Story.Header.Version <= FileVersion.V3)
            {
                value >>= 5;
            }
            else if (!((value & 0x80) > 0))
            {
                value >>= 6;
            }
            else
            {
                aMachine.Memory.GetByte(aProperty, out value);
                value &= 0x3f;

                if (value == 0)
                {
                    value = 64;
                }

            }

            return (ushort) (aProperty + value + 1);
        }

        public static void UnlinkObject(this ZMachine aMachine, ushort aObject)
        {
            if (aObject == 0)
            {
                return;
            }

            ushort xObjectAddress = aMachine.GetObjectAddress(aObject);

            if (aMachine.Story.Header.Version <= FileVersion.V3)
            {
                byte zero = 0;

                xObjectAddress += O1_PARENT;
                aMachine.Memory.GetByte(xObjectAddress, out byte xParentObject);
                if (xParentObject == 0)
                {
                    return;
                }

                aMachine.Memory.SetByte(xObjectAddress, zero);
                xObjectAddress += O1_SIBLING - O1_PARENT;
                aMachine.Memory.GetByte(xObjectAddress, out byte xOlderSibling);
                aMachine.Memory.SetByte(xObjectAddress, zero);

                ushort xParentAddress = (ushort) (aMachine.GetObjectAddress(xParentObject) + O1_CHILD);
                aMachine.Memory.GetByte(xParentAddress, out byte xYoungerSibling);

                if (xYoungerSibling == aObject)
                {
                    aMachine.Memory.SetByte(xParentAddress, xOlderSibling);
                }
                else
                {
                    ushort xSiblingAddress;
                    do
                    {
                        xSiblingAddress = (ushort) (aMachine.GetObjectAddress(xYoungerSibling) + O1_SIBLING);
                        aMachine.Memory.GetByte(xSiblingAddress, out xYoungerSibling);
                    } while (xYoungerSibling != aObject);

                    aMachine.Memory.SetByte(xSiblingAddress, xOlderSibling);
                }

            }
            else
            {
                ushort zero = 0;

                xObjectAddress += O4_PARENT;
                aMachine.Memory.GetWord(xObjectAddress, out ushort xParentObject);
                if (xParentObject == 0)
                {
                    return;
                }

                aMachine.Memory.SetWord(xObjectAddress, zero);
                xObjectAddress += O4_SIBLING - O4_PARENT;
                aMachine.Memory.GetWord(xObjectAddress, out ushort xOlderSibling);
                aMachine.Memory.SetWord(xObjectAddress, zero);

                ushort xParentAddress = (ushort) (aMachine.GetObjectAddress(xParentObject) + O4_CHILD);
                aMachine.Memory.GetWord(xParentAddress, out ushort xYoungerSibling);

                if (xYoungerSibling == aObject)
                {
                    aMachine.Memory.SetWord(xParentAddress, xOlderSibling);
                }
                else
                {
                    ushort xSiblingAddress;
                    do
                    {
                        xSiblingAddress = (ushort) (aMachine.GetObjectAddress(xYoungerSibling) + O4_SIBLING);
                        aMachine.Memory.GetWord(xYoungerSibling, out xYoungerSibling);
                    } while (xYoungerSibling != aObject);

                    aMachine.Memory.SetWord(xSiblingAddress, xOlderSibling);
                }
            }
        }      
    }
}

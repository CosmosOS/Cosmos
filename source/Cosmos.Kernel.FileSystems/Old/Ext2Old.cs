using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.FileSystem.Ext2 {
    public partial class Ext2Old {

        private static unsafe GroupDescriptor[] ReadGroupDescriptorsOfBlock(byte aController, byte aDrive, uint aBlockGroup, SuperBlock aSuperBlock, ushort* aBuffer) {
            uint xGroupDescriptorCount = aSuperBlock.INodesCount / aSuperBlock.INodesPerGroup;
            GroupDescriptor[] xResult = new GroupDescriptor[xGroupDescriptorCount];
            GroupDescriptor* xDescriptorPtr = (GroupDescriptor*)aBuffer;
            for (int i = 0; i < xGroupDescriptorCount; i++) {
                int xATABlock = (int)((8));
                xATABlock += (i / 16);
                if ((i % 16) == 0) {
                    if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, xATABlock, aBuffer)) {
                        Console.WriteLine("[Ext2|GroupDescriptors] Error while reading GroupDescriptor data");
                        return null;
                    }
                }
                xResult[i] = xDescriptorPtr[i % 16];
            }
            return xResult;
        }

        public static unsafe void PrintAllFilesAndDirectories(byte aController, byte aDrive) {
            //ushort* xBuffer = (ushort*)Heap.MemAlloc(512);
            ushort* xBuffer = null;
            if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, 2, xBuffer)) {
                Console.WriteLine("[Ext2|SuperBlock] Error while reading SuperBlock data");
                return;
            }
            SuperBlock xSuperBlock = *(SuperBlock*)xBuffer;
            int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
            uint xGroupsCount = xSuperBlock.INodesCount / xSuperBlock.INodesPerGroup;
            uint xGroupDescriptorsPerBlock = (uint)(xBlockSize / sizeof(GroupDescriptor));
            GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(aController, aDrive, xSuperBlock.FirstDataBlock + 1, xSuperBlock, xBuffer);
            if (xGroupDescriptors == null) {
                return;
            }
            bool[] xUsedINodes = new bool[32];
            uint xCount = 0;
            INode* xINodeTable = (INode*)xBuffer;
            for (uint g = 0; g < xGroupDescriptors.Length; g++) {
                GroupDescriptor xGroupDescriptor = xGroupDescriptors[g];
                if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)((xGroupDescriptor.INodeBitmap) * 8), xBuffer)) {
                    return;
                }
                if (!ConvertBitmapToBoolArray((uint*)xBuffer, xUsedINodes)) {
                    return;
                }
                for (int i = 0; i < 32; i++) {
                    if ((i % 4) == 0) {
                        uint index = (uint)((i % xSuperBlock.INodesPerGroup) * sizeof(INode));
                        uint offset = index / 512;
                        if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)((xGroupDescriptor.INodeTable * 8) + offset), xBuffer)) {
                            Console.WriteLine("[Ext2] Error reading INode table entries");
                            return;
                        }
                    }
                    uint xINodeIdentifier = (uint)((g * xSuperBlock.INodesPerGroup) + i + 1);
                    if (xINodeIdentifier == 0xB) {
                        continue;
                    }
                    if (xUsedINodes[i]) {
                        INode xINode = xINodeTable[i % 4];
                        //DebugUtil.SendExt2_INode((uint)((g * xSuperBlock.INodesPerGroup) + i), g, &xINodeTable[i % 4]);
                        if ((xINode.Mode & INodeModeEnum.Directory) != 0) {
                            //DebugUtil.SendNumber("Ext2", "Directory found", (uint)((g * xSuperBlock.INodesPerGroup) + i + 1), 32);
                            if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)(xINode.Block1 * 8), xBuffer)) {
                                Console.WriteLine("[Ext2] Error reading INode entries");
                                return;
                            }
                            DirectoryEntry* xEntryPtr = (DirectoryEntry*)xBuffer;
                            uint xTotalSize = xINode.Size;
                            while (xTotalSize != 0/* && xEntryPtr->@INode != 0*/) {
                                //DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
                                uint xPtrAddress = (uint)xEntryPtr;
                                //if (xEntryPtr->@INode == 0xC) {
                                char[] xName = new char[xEntryPtr->NameLength];
                                byte* xNamePtr = &xEntryPtr->FirstNameChar;
                                for (int c = 0; c < xName.Length; c++) {
                                    xName[c] = (char)xNamePtr[c];
                                }
                                string s = new String(xName);
//								DebugUtil.SendMessage("Ext2, DirectoryEntryName", s);
                                //}
                                xPtrAddress += xEntryPtr->RecordLength;
                                xTotalSize -= xEntryPtr->RecordLength;
                                xEntryPtr = (DirectoryEntry*)xPtrAddress;
                            }
                        }
                        xCount++;
                    }
                }
            }

//			DebugUtil.SendNumber("Ext2", "Used INode count", xCount, 32);
        }

        public static unsafe byte[] ReadFile(byte aController, byte aDrive, string[] xPath) {
            //ushort* xBuffer = (ushort*)Heap.MemAlloc(512);
            ushort* xBuffer = null;
            if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, 2, xBuffer)) {
                Console.WriteLine("[Ext2|SuperBlock] Error while reading SuperBlock data");
                return null;
            }
            SuperBlock xSuperBlock = *(SuperBlock*)xBuffer;
            //DebugUtil.SendExt2_SuperBlock("ReadFileContents", &xSuperBlock);
            int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
            uint xGroupsCount = xSuperBlock.INodesCount / xSuperBlock.INodesPerGroup;
            uint xGroupDescriptorsPerBlock = (uint)(xBlockSize / sizeof(GroupDescriptor));
            GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(aController, aDrive, xSuperBlock.FirstDataBlock + 1, xSuperBlock, xBuffer);
            if (xGroupDescriptors == null) {
                return null;
            }
            bool[] xUsedINodes = new bool[32];
            uint xCount = 0;
            INode* xINodeTable = (INode*)xBuffer;
            uint xPathPointer = 0;
            uint xCurrentINode = 2;
            Console.WriteLine("Looking up directories");
            Console.Write("    xPath.Length = ");
            Hardware.Storage.ATAOld.WriteNumber((uint)xPath.Length, 16);
            Console.WriteLine("");
            int xIterations = 0;
            while (xPathPointer != (xPath.Length + 1)) {
                if (xIterations == 5) {
                    Console.WriteLine("DEBUG: Stopping iteration");
                    break;
                }
                Console.WriteLine("Searching items");
                Console.Write("    xPathPointer = ");
                Hardware.Storage.ATAOld.WriteNumber(xPathPointer, 16);
                Console.WriteLine("");
                Console.Write("    xCurrentINode = ");
                Hardware.Storage.ATAOld.WriteNumber(xCurrentINode, 32);
                Console.WriteLine("");

//				DebugUtil.SendNumber("Ext2", "Current INode", xCurrentINode, 32);
                for (uint g = 0; g < xGroupDescriptors.Length; g++) {
                    GroupDescriptor xGroupDescriptor = xGroupDescriptors[g];
                    if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)((xGroupDescriptor.INodeBitmap) * 8), xBuffer)) {
                        return null;
                    }
                    if (!ConvertBitmapToBoolArray((uint*)xBuffer, xUsedINodes)) {
                        return null;
                    }
                    for (int i = 0; i < 32; i++) {
                        if ((i % 4) == 0) {
                            uint index = (uint)((i % xSuperBlock.INodesPerGroup) * sizeof(INode));
                            uint offset = index / 512;
                            if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)((xGroupDescriptor.INodeTable * 8) + offset), xBuffer)) {
                                Console.WriteLine("[Ext2] Error reading INode table entries");
                                return null;
                            }
                        }
                        uint xINodeIdentifier = (uint)((g * xSuperBlock.INodesPerGroup) + i + 1);
                        //DebugUtil.SendNumber("Ext2", "Checking INode", xINodeIdentifier, 32);
                        if (xINodeIdentifier != xCurrentINode) {
                            continue;
                        }
                        if (xUsedINodes[i]) {
                            INode xINode = xINodeTable[i % 4];
                            //DebugUtil.SendExt2_INode((uint)((g * xSuperBlock.INodesPerGroup) + i), g, &xINodeTable[i % 4]);
                            if (xPathPointer == xPath.Length) {
                                #region temporary checks
                                if (xINode.Block2 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block3 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block4 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block5 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block6 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block7 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block8 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block9 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block10 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block11 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block12 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block13 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block14 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                if (xINode.Block15 != 0) {
                                    Console.WriteLine("Multiblock files not supported yet!");
                                    return null;
                                }
                                #endregion
                                Console.WriteLine("Get file contents now");
                                if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)(xINode.Block1 * 8), xBuffer)) {
                                    Console.WriteLine("[Ext2] Error reading INode entries");
                                    return null;
                                }
                                byte[] xResult = new byte[512];
                                byte* xByteBuff = (byte*)xBuffer;
                                for (int b = 0; b < 512; b++) {
                                    xResult[b] = xByteBuff[b];
                                }
                                return xResult;
                            } else {
                                if ((xINode.Mode & INodeModeEnum.Directory) != 0) {
                                    //DebugUtil.SendNumber("Ext2", "Directory found", (uint)((g * xSuperBlock.INodesPerGroup) + i + 1), 32);
                                    //DebugUtil.SendExt2_INode(xINodeIdentifier, g, &xINodeTable[i % 4]);
                                    //DebugUtil.SendExt2_INodeMode(xINodeIdentifier, (ushort)xINode.Mode);
                                    if (!Hardware.Storage.ATAOld.ReadDataNew(aController, aDrive, (int)(xINode.Block1 * 8), xBuffer)) {
                                        Console.WriteLine("[Ext2] Error reading INode entries");
                                        return null;
                                    }
                                    DirectoryEntry* xEntryPtr = (DirectoryEntry*)xBuffer;
                                    uint xTotalSize = xINode.Size;
                                    while (xTotalSize != 0/* && xEntryPtr->@INode != 0*/) {
                                        //DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
                                        uint xPtrAddress = (uint)xEntryPtr;
                                        char[] xName = new char[xEntryPtr->NameLength];
                                        byte* xNamePtr = &xEntryPtr->FirstNameChar;
                                        for (int c = 0; c < xName.Length; c++) {
                                            xName[c] = (char)xNamePtr[c];
                                        }
                                        if (EqualsName(xPath[xPathPointer], xName)) {
                                            xPathPointer++;
                                            xCurrentINode = xEntryPtr->@INode;
                                            continue;
                                        }
                                        xPtrAddress += xEntryPtr->RecordLength;
                                        xTotalSize -= xEntryPtr->RecordLength;
                                        xEntryPtr = (DirectoryEntry*)xPtrAddress;
                                    }
                                }
                            }
                            xCount++;
                        }
                    }
                }
            }
            //DebugUtil.SendMessage("Ext2", "Lookup file now");
            //DebugUtil.SendExt2_SuperBlock("Looking up file", &xSuperBlock);
            //Console.WriteLine("Lookup file now:");
            //DebugUtil.SendNumber("Ext2", "CurrentINode", xCurrentINode, 32);
            //uint xGroup = xCurrentINode / xSuperBlock.INodesPerGroup;
            //uint xGroupINodeBase = xGroup * xSuperBlock.INodesPerGroup;
            //for (int i = 0; i < 32; i++) {
            //    if ((i % 4) == 0) {
            //        uint index = (uint)((i % xSuperBlock.INodesPerGroup) * sizeof(INode));
            //        uint offset = index / 512;
            //        if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, (int)((xGroupDescriptors[xGroup].INodeTable * 8) + offset), xBuffer)) {
            //            Console.WriteLine("[Ext2] Error reading INode entries");
            //            return null;
            //        }
            //    }
            //    INode xItem = xINodeTable[i % 4];
            //    if (((xGroupINodeBase + i) == xCurrentINode) && ((xItem.Mode & INodeModeEnum.RegularFile) != 0)) {
            //        DebugUtil.SendExt2_INode(xCurrentINode, xGroup, &xItem);
            //        Console.WriteLine("File found :-)");
            //        //return null;
            //    }
            //}
            //Console.WriteLine("Directory found");
            //DebugUtil.SendNumber("Ext2", "Used INode count", xCount, 32);
            Console.WriteLine("TODO: Lookup File now");
            return null;
        }

        private static unsafe bool ConvertBitmapToBoolArray(uint* aBitmap, bool[] aArray) {
            if (aArray == null || aArray.Length != 32) {
                Console.WriteLine("[ConvertBitmapToBoolArray] Incorrect Array");
                return false;
            }
            uint xValue = *aBitmap;
            for (byte b = 0; b < 32; b++) {
                uint xCheckBit = (uint)(1 << b);
                aArray[b] = (xValue & xCheckBit) != 0;
            }
            return true;
        }

        private static bool EqualsName(string a1, char[] a2) {
            if (a1 == null || a2 == null) {
                return false;
            }
            if (a1.Length != a2.Length) {
                return false;
            }
            for (int i = 0; i < a1.Length; i++) {
                if (a1[i] != a2[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}
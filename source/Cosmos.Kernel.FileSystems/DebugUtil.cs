using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.FileSystem
{
    public static class DebugUtil
    {
        internal static void Send_Ext2SuperBlock(Ext2.Ext2.SuperBlock aSuperBlock)
        {
            Hardware.DebugUtil.StartLogging();
            Hardware.DebugUtil.WriteSerialString("<Ext2_SuperBlock ");
            Hardware.DebugUtil.WriteSerialString("INodesCount=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.INodesCount, 32);
            Hardware.DebugUtil.WriteSerialString("\" BlocksCount=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.BlockCount, 32);
            Hardware.DebugUtil.WriteSerialString("\" FreeBlockCount=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.FreeBlocksCount, 32); 
            Hardware.DebugUtil.WriteSerialString("\" FreeINodesCount=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.FreeINodesCount, 32); 
            Hardware.DebugUtil.WriteSerialString("\" FirstDataBlock=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.FirstDataBlock, 32); 
            Hardware.DebugUtil.WriteSerialString("\" LogBlockSize=\"");
            Hardware.DebugUtil.WriteNumber((uint)aSuperBlock.LogBlockSize, 32); 
            Hardware.DebugUtil.WriteSerialString("\" LogFragSize=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.LogFragSize, 32); 
            Hardware.DebugUtil.WriteSerialString("\" BlocksPerGroup=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.BlocksPerGroup, 32); 
            Hardware.DebugUtil.WriteSerialString("\" FragsPerGroup=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.FragsPerGroup, 32); 
            Hardware.DebugUtil.WriteSerialString("\" INodesPerGroup=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.INodesPerGroup, 32); 
            Hardware.DebugUtil.WriteSerialString("\" MTime=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.MTime, 32); 
            Hardware.DebugUtil.WriteSerialString("\" WTime=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.WTime, 32); 
            Hardware.DebugUtil.WriteSerialString("\" MountCount=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.MntCount, 16); 
            Hardware.DebugUtil.WriteSerialString("\" MaxMountCount=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.MaxMntCount, 16);
            Hardware.DebugUtil.WriteSerialString("\" Magic=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.Magic, 16);
            Hardware.DebugUtil.WriteSerialString("\" State=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.State, 16);
            Hardware.DebugUtil.WriteSerialString("\" Errors=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.Errors, 16);
            Hardware.DebugUtil.WriteSerialString("\" MinorRevLevel=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.MinorRevLevel, 16);
            Hardware.DebugUtil.WriteSerialString("\" LastCheck=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.LastCheck, 32);
            Hardware.DebugUtil.WriteSerialString("\" CheckInterval=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.CheckInterval, 32);
            Hardware.DebugUtil.WriteSerialString("\" CreatorOS=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.CreatorOS, 32);
            Hardware.DebugUtil.WriteSerialString("\" RevLevel=\"");
            Hardware.DebugUtil.WriteNumber(aSuperBlock.RevLevel, 32);
            Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
            Hardware.DebugUtil.EndLogging();
        }

        internal static void Send_Ext2GroupDescriptors(Ext2.Ext2.GroupDescriptor[] aDescriptors) {
            Hardware.DebugUtil.StartLogging();
            for(uint i = 0; i < aDescriptors.Length;i++) {
                Hardware.DebugUtil.WriteSerialString("<Ext2_GroupDescriptor Index=\"");
                Hardware.DebugUtil.WriteNumber(i, 32);
                Hardware.DebugUtil.WriteSerialString("\" BlockBitmap=\"");
                Hardware.DebugUtil.WriteNumber(aDescriptors[i].BlockBitmap, 32);
                Hardware.DebugUtil.WriteSerialString("\" INodeBitmap=\"");
                Hardware.DebugUtil.WriteNumber(aDescriptors[i].INodeBitmap, 32);
                Hardware.DebugUtil.WriteSerialString("\" INodeTable=\"");
                Hardware.DebugUtil.WriteNumber(aDescriptors[i].INodeTable, 32);
                Hardware.DebugUtil.WriteSerialString("\" FreeBlocksCount=\"");
                Hardware.DebugUtil.WriteNumber(aDescriptors[i].FreeBlocksCount, 32);
                Hardware.DebugUtil.WriteSerialString("\" FreeINodesCount=\"");
                Hardware.DebugUtil.WriteNumber(aDescriptors[i].FreeINodesCount, 32);
                Hardware.DebugUtil.WriteSerialString("\" UsedDirsCount=\"");
                Hardware.DebugUtil.WriteNumber(aDescriptors[i].UsedDirsCount, 32);
                Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
            }
            Hardware.DebugUtil.EndLogging();
        }
    }
}
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
            //Hardware2.DebugUtil.StartLogging();
            //Hardware2.DebugUtil.WriteSerialString("<Ext2_SuperBlock ");
            //Hardware2.DebugUtil.WriteSerialString("INodesCount=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.INodesCount, 32);
            //Hardware2.DebugUtil.WriteSerialString("\" BlocksCount=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.BlockCount, 32);
            //Hardware2.DebugUtil.WriteSerialString("\" FreeBlockCount=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.FreeBlocksCount, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" FreeINodesCount=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.FreeINodesCount, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" FirstDataBlock=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.FirstDataBlock, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" LogBlockSize=\"");
            //Hardware2.DebugUtil.WriteNumber((uint)aSuperBlock.LogBlockSize, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" LogFragSize=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.LogFragSize, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" BlocksPerGroup=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.BlocksPerGroup, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" FragsPerGroup=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.FragsPerGroup, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" INodesPerGroup=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.INodesPerGroup, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" MTime=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.MTime, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" WTime=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.WTime, 32); 
            //Hardware2.DebugUtil.WriteSerialString("\" MountCount=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.MntCount, 16); 
            //Hardware2.DebugUtil.WriteSerialString("\" MaxMountCount=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.MaxMntCount, 16);
            //Hardware2.DebugUtil.WriteSerialString("\" Magic=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.Magic, 16);
            //Hardware2.DebugUtil.WriteSerialString("\" State=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.State, 16);
            //Hardware2.DebugUtil.WriteSerialString("\" Errors=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.Errors, 16);
            //Hardware2.DebugUtil.WriteSerialString("\" MinorRevLevel=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.MinorRevLevel, 16);
            //Hardware2.DebugUtil.WriteSerialString("\" LastCheck=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.LastCheck, 32);
            //Hardware2.DebugUtil.WriteSerialString("\" CheckInterval=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.CheckInterval, 32);
            //Hardware2.DebugUtil.WriteSerialString("\" CreatorOS=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.CreatorOS, 32);
            //Hardware2.DebugUtil.WriteSerialString("\" RevLevel=\"");
            //Hardware2.DebugUtil.WriteNumber(aSuperBlock.RevLevel, 32);
            //Hardware2.DebugUtil.WriteSerialString("\"/>\r\n");
            //Hardware2.DebugUtil.EndLogging();
        }

        internal static void Send_Ext2GroupDescriptors(Ext2.Ext2.GroupDescriptor[] aDescriptors) {
            //Hardware2.DebugUtil.StartLogging();
            //for(uint i = 0; i < aDescriptors.Length;i++) {
            //    Hardware2.DebugUtil.WriteSerialString("<Ext2_GroupDescriptor Index=\"");
            //    Hardware2.DebugUtil.WriteNumber(i, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\" BlockBitmap=\"");
            //    Hardware2.DebugUtil.WriteNumber(aDescriptors[i].BlockBitmap, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\" INodeBitmap=\"");
            //    Hardware2.DebugUtil.WriteNumber(aDescriptors[i].INodeBitmap, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\" INodeTable=\"");
            //    Hardware2.DebugUtil.WriteNumber(aDescriptors[i].INodeTable, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\" FreeBlocksCount=\"");
            //    Hardware2.DebugUtil.WriteNumber(aDescriptors[i].FreeBlocksCount, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\" FreeINodesCount=\"");
            //    Hardware2.DebugUtil.WriteNumber(aDescriptors[i].FreeINodesCount, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\" UsedDirsCount=\"");
            //    Hardware2.DebugUtil.WriteNumber(aDescriptors[i].UsedDirsCount, 32);
            //    Hardware2.DebugUtil.WriteSerialString("\"/>\r\n");
            //}
            //Hardware2.DebugUtil.EndLogging();
        }
    }
}
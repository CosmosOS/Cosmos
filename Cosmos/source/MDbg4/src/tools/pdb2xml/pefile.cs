//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------


/*
 * Utility code to update a PE Header with new PDB signature
 * 
 ************************************************
 * This code is not intended to act as a sample *
 ************************************************
 * 
 * Most programs that write managed symbol infromation into a pdb file 
 * would also be emitting an image (exe or dll) file at the same time.
 * A real compiler would clearly need to fully generate the PE file 
 * from scratch and this code just finds and updates the debug info 
 * section of the file.
 * 
 * For a good reference on the PE/COFF file format, see this document:
 * http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
 */

using System;
using System.IO;

namespace Pdb2Xml
{
    internal class PEFile
    {
        // Private members to track information at runtime
        private string m_ExePath;
        private ushort m_PEFormat;
        private byte m_PESignatureOffset;
        private uint m_COFFHeaderOffset;
        private ushort m_NumberOfSections;
        private ushort m_SizeOfOptionalHeader;
        private uint m_StartOfSections;
        private uint m_PEHeaderStart;
        private uint m_DebugInfoFilePointer;

        // Constants that are known ahead of time.
        const byte PESignatureOffsetLoc = 0x3C;
        const byte SizeOfCOFFHeader = 0x14;
        const byte SizeOfSection = 0x28;
        const byte SizeOfDebugDirectory = 0x1C;
        const byte SizeOfDebugInfo = 0x18;
        const ushort PE32 = 0x010B;
        const ushort PE32PLUS = 0x020B;


        internal PEFile(string path)
        {
            m_ExePath = path;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    // Find the address of the PE Header
                    br.BaseStream.Seek(PESignatureOffsetLoc, SeekOrigin.Begin);
                    m_PESignatureOffset = br.ReadByte();
                    br.BaseStream.Seek(m_PESignatureOffset, SeekOrigin.Begin);
                    // Start of PE Signature
                    Byte[] sig = br.ReadBytes(4);
                    if (sig[0] != 'P' || sig[1] != 'E' || sig[2] != 0 || sig[3] != 0)
                        Util.Error("PE Signature corrupted");

                    // Start of COFF Header
                    m_COFFHeaderOffset = (uint)m_PESignatureOffset + 4;
                    // COFF: 0x02 contains 2 bytes that indicate the NumberOfSections
                    // COFF: 0x10 contains 2 bytes that indicate the SizeOfOptionalHeader
                    br.BaseStream.Seek(m_COFFHeaderOffset + 0x02, SeekOrigin.Begin);
                    m_NumberOfSections = br.ReadUInt16();
                    br.BaseStream.Seek(m_COFFHeaderOffset + 0x10, SeekOrigin.Begin);
                    m_SizeOfOptionalHeader = br.ReadUInt16();

                    // Start of PE Header
                    m_PEHeaderStart = (ushort)(m_COFFHeaderOffset + SizeOfCOFFHeader);
                    m_StartOfSections = (ushort)(m_PEHeaderStart + m_SizeOfOptionalHeader);
                    br.BaseStream.Seek(m_PEHeaderStart, SeekOrigin.Begin);

                    m_PEFormat = (ushort)br.ReadInt16();
                    if (m_PEFormat != PE32 && m_PEFormat != PE32PLUS)
                    {
                        Util.Error("Unrecognized PE Format: " + m_PEFormat);
                    }
                    // The layout of the optional header varies (0x60 or 0x70) based upon the PE_Format setting
                    uint DebugDirectoryOffset = m_PEHeaderStart + (m_PEFormat == PE32 ? 0x90u : 0xA0u);
                    br.BaseStream.Seek(DebugDirectoryOffset, SeekOrigin.Begin);
                    uint DebugRVA = br.ReadUInt32();

                    m_DebugInfoFilePointer = GetFileOffsetForRVA(br, DebugRVA);
                }
            }
        }

        private uint GetFileOffsetForRVA(BinaryReader br, uint RVA)
        {
            // Find a section containing this RVA, ie where:
            // VirtualAdddress <= RVA < VirtualAddress+VirtualSize

            const int SizeOfSection = 0x28;
            for (int i = 0; i < m_NumberOfSections; i++)
            {
                // VirtualSize at Section:0x08 (size 4)
                // VirtualAddress at Section:0x0C (size 4)
                // PointerToRawData at Section:0x14 (size 4)
                br.BaseStream.Seek(m_StartOfSections + i * SizeOfSection, SeekOrigin.Begin);
                br.BaseStream.Seek(0x08, SeekOrigin.Current);
                uint VirtualSize = br.ReadUInt32();
                uint VirtualAddress = br.ReadUInt32();
                if (RVA >= VirtualAddress && RVA < VirtualAddress + VirtualSize)
                {
                    uint SizeOfRawData = br.ReadUInt32();       // not used here
                    uint PointerToRawData = br.ReadUInt32();
                    uint FileOffset = PointerToRawData + (RVA - VirtualAddress);
                    return FileOffset;
                }
            }
            Util.Error("Section not found");
            return 0;
        }

        internal void UpdateHeader(byte[] DebugInfo)
        {
            using (FileStream fs = new FileStream(m_ExePath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    // Get to the beginning of the debug data
                    // TODO: This assumes there is only a single entry in the debug directory and that the
                    // data for that entry follows the directory.  We should really be computing more of this
                    // from the structures, such as IMAGE_DEBUG_DIRECTORY.PointerToRawData.
                    uint PointerToRawData = m_DebugInfoFilePointer + SizeOfDebugDirectory;
                    bw.Seek((int)PointerToRawData, SeekOrigin.Begin);
                    int count = DebugInfo.Length;
                    if (count > SizeOfDebugInfo)
                        count = SizeOfDebugInfo;
                    bw.Write(DebugInfo, 0, count);
                }
            }
        }
    }
}
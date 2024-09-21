using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.ISO9660
{
    public class ISO9660FileSystem : FileSystem
    {
        private string label;
        public override long AvailableFreeSpace => 0;

        public override long TotalFreeSpace => 0;

        public override string Type => "ISO9660";

        public override string Label { get => label; set => throw new NotImplementedException("Read only file system"); }

        private ISO9660Directory RootDir;

        public ISO9660FileSystem(Partition aDevice, string aRootPath, long aSize)
            : base(aDevice, aRootPath, aSize)
        {
            if (aDevice == null)
            {
                throw new ArgumentNullException(nameof(aDevice));
            }

            if (String.IsNullOrEmpty(aRootPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aRootPath));
            }

            //Read primary descriptor
            var primaryDescriptor = aDevice.NewBlockArray(1);
            aDevice.ReadBlock(0x10, 1, ref primaryDescriptor);
            using var r = new BinaryReader(new MemoryStream(primaryDescriptor));

            var volType = r.ReadByte();
            var id = r.ReadBytes(5);
            var version = r.ReadByte();

            //this should always be 1
            if (volType != 1)
            {
                throw new Exception("Volume descriptor " + volType + " not implemented");
            }

            r.ReadByte(); //unused
            r.ReadBytes(32); //	System Identifier
            label = Encoding.ASCII.GetString(r.ReadBytes(32)); //Volume Identifier

            r.BaseStream.Position = 156;
            var b = r.ReadBytes(34);
            using BinaryReader rootdir = new BinaryReader(new MemoryStream(b));
            RootDir = ReadDirectoryEntry(rootdir);
        }
        /// <summary>
        /// Reads directory contents
        /// </summary>
        /// <param name="locOfExtent">Location of extent</param>
        /// <returns>List of Directorys</returns>
        private List<ISO9660Directory> ReadDirectory(uint locOfExtent)
        {
            var dirs = new List<ISO9660Directory>();
            var locOfExt = Device.NewBlockArray(1);
            Device.ReadBlock(locOfExtent, 1, ref locOfExt);
            var lba = new BinaryReader(new MemoryStream(locOfExt));

            //Skip past the . and .. directoryies
            var len1 = lba.ReadByte();
            lba.BaseStream.Position += len1 - 1;
            var len2 = lba.ReadByte();
            lba.BaseStream.Position += len2 - 1;

            while (true)
            {
                var e = ReadDirectoryEntry(lba);
                if (e.Length == 0)
                    break;
                dirs.Add(e);
            }
            return dirs;
        }
        /// <summary>
        /// Reads a DirectoryEntry
        /// </summary>
        /// <param name="lba">Binary reader</param>
        /// <returns>A ISO9660Directory</returns>
        private ISO9660Directory ReadDirectoryEntry(BinaryReader lba)
        {
            var old = lba.BaseStream.Position;

            var dir = new ISO9660Directory();
            dir.Length = lba.ReadByte();
            dir.ExtLength = lba.ReadByte();


            //Read LBA (8 bytes)
            var locofextbytesPT1 = lba.ReadBytes(4);
            var locofextbytesPT2 = lba.ReadBytes(4);
            locofextbytesPT1 = ReverseArray(locofextbytesPT1, true);
            byte[] rv = new byte[locofextbytesPT1.Length + locofextbytesPT2.Length];
            Buffer.BlockCopy(locofextbytesPT1, 0, rv, 0, locofextbytesPT1.Length);
            Buffer.BlockCopy(locofextbytesPT2, 0, rv, locofextbytesPT1.Length, locofextbytesPT2.Length);
            dir.LBA = BitConverter.ToUInt32(rv, 0);

            //Read data length (8 bytes)
            var sizePT1 = lba.ReadBytes(4);
            var sizePT2 = lba.ReadBytes(4);
            sizePT1 = ReverseArray(sizePT1, true);
            byte[] rv2 = new byte[sizePT1.Length + sizePT2.Length];
            Buffer.BlockCopy(sizePT1, 0, rv2, 0, sizePT1.Length);
            Buffer.BlockCopy(sizePT2, 0, rv2, sizePT1.Length, sizePT2.Length);
            dir.FileSize = BitConverter.ToUInt32(rv2, 0);

            lba.ReadBytes(7); //recording date & time
            lba.BaseStream.Position = old + 25;
            dir.FileFlags = lba.ReadByte();


            lba.ReadBytes(6); //Unneeded fields

            dir.FileIdLen = lba.ReadByte();

            var str = lba.ReadBytes(dir.FileIdLen);
            dir.FileID = Encoding.ASCII.GetString(str);
            lba.BaseStream.Position = old + dir.Length;

            return dir;
        }
        /// <summary>
        /// Reverses an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        private byte[] ReverseArray(byte[] array, bool isLittleEndian)
        {
            List<byte> b = new List<byte>();
            if (BitConverter.IsLittleEndian)
            {
                if (isLittleEndian)
                {
                    //no need to swap
                    return array;
                }
            }
            for (int i = array.Length - 1; i >= 0; i--)
            {
                b.Add(array[i]);
            }
            return b.ToArray();
        }
        /// <summary>
        /// Reads a file
        /// </summary>
        /// <param name="entry">File Entry</param>
        /// <returns>Byte array of the file</returns>
        internal byte[] ReadFile(ISO9660Directory entry)
        {
            var oldsize = entry.FileSize;
            byte[] file = new byte[entry.FileSize];
            var SectorIndex = entry.LBA;
            int offset = 0;
            byte[] sector = new byte[2048];
            while (true)
            {
                Device.ReadBlock(SectorIndex, 1, ref sector);

                if (entry.FileSize < 2048)
                {
                    for (int i = 0; i < entry.FileSize; i++)
                    {
                        file[offset + i] = sector[i];
                    }
                    entry.FileSize = oldsize;
                    break;
                }
                else
                {
                    //Read more sectors
                    Buffer.BlockCopy(sector, 0, file, offset, sector.Length);

                    offset += 2048;
                    entry.FileSize -= 2048;
                    SectorIndex++;
                }
            }
            return file;
        }
        public override void DisplayFileSystemInfo()
        {
            global::System.Console.WriteLine("Volume label is " + label);
        }
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            var baseEntry = (ISO9660DirectoryEntry)baseDirectory;
            var dirEntries = ReadDirectory(baseEntry.internalEntry.LBA);
            List<DirectoryEntry> entries = new List<DirectoryEntry>();
            var parent = new ISO9660DirectoryEntry(baseEntry.internalEntry, this, baseEntry, baseDirectory.mFullPath, ".", 0, DirectoryEntryTypeEnum.Directory);
            foreach (var item in dirEntries)
            {
                DirectoryEntryTypeEnum type;
                var fName = item.FileID;

                if ((item.FileFlags & (1 << 1)) != 0)
                {
                    type = DirectoryEntryTypeEnum.Directory;
                }
                else
                {
                    type = DirectoryEntryTypeEnum.File; //remove the ;1 part from the file name
                    fName = fName.Remove(fName.Length - 2);
                }
                entries.Add(new ISO9660DirectoryEntry(item, this, parent, Path.Combine(parent.mFullPath, fName), fName, 0, type));
            }
            return entries;
        }
        public override DirectoryEntry GetRootDirectory()
        {
            return new ISO9660DirectoryEntry(RootDir, this, null, RootPath, RootPath, 0, DirectoryEntryTypeEnum.Directory);
        }
        #region Unused
        public override DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory)
        {
            throw new NotImplementedException("Read only file system");
        }

        public override DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile)
        {
            throw new NotImplementedException("Read only file system");
        }

        public override void DeleteDirectory(DirectoryEntry aPath)
        {
            throw new NotImplementedException("Read only file system");
        }

        public override void DeleteFile(DirectoryEntry aPath)
        {
            throw new NotImplementedException("Read only file system");
        }
        public override void Format(FileSystemFormat aDriveFormat, bool aQuick)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;
using DuNodes.HAL.FileSystem.Crypto;
using DuNodes.Kernel.Base.IO;

namespace DuNodes.HAL.FileSystem.Base
{
    public class DNFS : StorageDevice
    {
        private Partition part = (Partition)null;
        public int ID = 0;
        private string sname = (string)null;
        private int last_addr = 0;
        public int prevLoc = 0;
        private int node_faddr;

        public DNFS(Partition p)
        {
            this.part = p;
            byte[] numArray = new byte[512];
            ((BlockDevice)this.part).ReadBlock(1UL, 1U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(512));
            binaryReader.BaseStream.Data = numArray;
            if (binaryReader.ReadString() != "GFS SC")
                return;
            this.DriveLabel = binaryReader.ReadString();
        }

        public override void makeDir(string name, string owner)
        {
            name = this.cleanName(name);
            if (!this.Contains(name, FileSystem.Root.Seperator))
            {
                //if ((int)CurrentUser.Privilages != 0 && this.DriveLabel == "GruntyOS")
                //    throw new Exception("Can not make files here");
                this.createNode(name, 2, owner);
            }
            else
            {
                //if (!this.CanWrite(name.Substring(0, Util.LastIndexOf(name, FileSystem.Root.Seperator))))
                //    throw new Exception("Access denied!");
                this.createNode(name, 2, owner);
            }
        }

        public void Format(string VolumeLABEL)
        {
            byte[] numArray = new byte[512];
            MemoryStream memoryStream = new MemoryStream(512);
            for (int index = 0; index < 512; ++index)
                numArray[index] = (byte)0;
            for (int index = 0; index < 100; ++index)
                ((BlockDevice)this.part).WriteBlock((ulong)index, 1U, numArray);
            memoryStream.Data = numArray;
            BinaryWriter binaryWriter = new BinaryWriter((ioStream)memoryStream);
            binaryWriter.Write("GFS SC");
            binaryWriter.Write(VolumeLABEL);
            binaryWriter.Write(4);
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock(1UL, 1U, binaryWriter.BaseStream.Data);
        }

        public override void Delete(string Path)
        {
            this.Unlink(Path);
        }

        private bool Contains(string Str, char c)
        {
            foreach (int num in Str)
            {
                if (num == (int)c)
                    return true;
            }
            return false;
        }

        public static bool isGFS(Partition part)
        {
            byte[] numArray = new byte[512];
            ((BlockDevice)part).ReadBlock(1UL, 1U, numArray);
            return new BinaryReader((ioStream)new MemoryStream(512))
            {
                BaseStream = {
          Data = numArray
        }
            }.ReadString() == "GFS SC";
        }

        public byte[] GetFile(int index)
        {
            return (byte[])null;
        }

        private byte[] ReadData(int loc, int count)
        {
            if (count < 512)
            {
                byte[] numArray1 = new byte[512];
                ((BlockDevice)this.part).ReadBlock((ulong)loc, 1U, numArray1);
                byte[] numArray2 = new byte[count];
                for (int index = 4; index < count; ++index)
                    numArray2[index] = numArray1[index];
                return numArray2;
            }
            int index1 = 0;
            byte[] numArray3 = new byte[count];
            int num = 4;
            while (index1 < count)
            {
                byte[] numArray1 = new byte[512];
                ((BlockDevice)this.part).ReadBlock((ulong)loc, 1U, numArray1);
                for (int index2 = num; index2 < 512; ++index2)
                {
                    numArray3[index1] = numArray1[index2];
                    if (index1 == count)
                        return numArray3;
                    ++index1;
                }
                num = 0;
                ++loc;
            }
            return (byte[])null;
        }

        private int IndexOf(string str, char c)
        {
            int num1 = 0;
            foreach (int num2 in str)
            {
                if (num2 == (int)c)
                    return num1;
                ++num1;
            }
            return -1;
        }

        private bool GetBit(byte b, int bitNumber)
        {
            return ((int)b & 1 << bitNumber - 1) != 0;
        }

        public bool CanWrite(string file)
        {
            //if ((int)CurrentUser.Privilages == 0)
            //    return true;
            //fsEntry fsEntry = this.readFromNode(file);
            //return this.GetBit(fsEntry.User, 2) && CurrentUser.Username == fsEntry.Owner || this.GetBit(fsEntry.Group, 2) || this.GetBit(fsEntry.Global, 2);
            return true;
        }

        public bool CanRead(string file)
        {
            //if ((int)CurrentUser.Privilages == 0)
            //    return true;
            //fsEntry fsEntry = this.readFromNode(file);
            //return this.GetBit(fsEntry.User, 3) && CurrentUser.Username == fsEntry.Owner || this.GetBit(fsEntry.Group, 3) || this.GetBit(fsEntry.Global, 3);
            return true;
        }

        public bool CanExecute(string file)
        {
            //fsEntry fsEntry = this.readFromNode(file);
            //return this.GetBit(fsEntry.User, 1) && CurrentUser.Username == fsEntry.Owner || this.GetBit(fsEntry.Group, 1) || this.GetBit(fsEntry.Global, 1);
            return true;
        }

        private void insertEntry(fsEntry ent, int Node_block)
        {
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)Node_block, 2U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray;
            int data = binaryReader.ReadInt32() + 1;
            for (int index = 0; index < data - 1; ++index)
            {
                binaryReader.ReadString();
                binaryReader.ReadInt32();
                binaryReader.ReadInt32();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadInt32();
            }
            int num = binaryReader.BaseStream.Position;
            BinaryWriter binaryWriter = new BinaryWriter((ioStream)new MemoryStream(1024));
            binaryWriter.BaseStream.Data = numArray;
            binaryWriter.Write(data);
            binaryWriter.BaseStream.Position = num;
            binaryWriter.Write(ent.Name);
            binaryWriter.Write(ent.Pointer);
            binaryWriter.Write(ent.Length);
            binaryWriter.BaseStream.Write(ent.Attributes);
            binaryWriter.Write(ent.User);
            binaryWriter.Write(ent.Group);
            binaryWriter.Write(ent.Global);
            binaryWriter.Write(RTC.Now.GetDate(RTC.Now.DateFormat.DD_MM_YYYY));
            binaryWriter.Write(ent.Owner);
            binaryWriter.Write(ent.Checksum);
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock((ulong)Node_block, 2U, binaryWriter.BaseStream.Data);
        }

        public int getWriteAddress()
        {
            byte[] numArray = new byte[512];
            ((BlockDevice)this.part).ReadBlock(1UL, 1U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(512));
            binaryReader.BaseStream.Data = numArray;
            binaryReader.ReadString();
            binaryReader.ReadString();
            return binaryReader.ReadInt32();
        }

        private void increaseWriteAddress(int amount)
        {
            byte[] numArray = new byte[512];
            ((BlockDevice)this.part).ReadBlock(1UL, 1U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(512));
            binaryReader.BaseStream.Data = numArray;
            binaryReader.ReadString();
            binaryReader.ReadString();
            BinaryWriter binaryWriter = new BinaryWriter(binaryReader.BaseStream);
            binaryWriter.BaseStream.Position = binaryReader.BaseStream.Position;
            binaryWriter.Write(this.getWriteAddress() + amount);
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock(1UL, 1U, binaryWriter.BaseStream.Data);
        }

        public void setWriteAddress(int amount)
        {
            byte[] numArray = new byte[512];
            ((BlockDevice)this.part).ReadBlock(1UL, 1U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(512));
            binaryReader.BaseStream.Data = numArray;
            binaryReader.ReadString();
            binaryReader.ReadString();
            BinaryWriter binaryWriter = new BinaryWriter(binaryReader.BaseStream);
            binaryWriter.BaseStream.Position = binaryReader.BaseStream.Position;
            binaryWriter.Write(amount);
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock(1UL, 1U, binaryWriter.BaseStream.Data);
        }

        private void createNode(string name, int Node_block, string owner)
        {
            name = this.cleanName(name);
            byte[] numArray1 = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)Node_block, 2U, numArray1);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray1;
            this.prevLoc = Node_block;
            int num1 = binaryReader.ReadInt32();
            if (this.Contains(name, FileSystem.Root.Seperator))
            {
                string str1 = name.Substring(0, this.IndexOf(name, FileSystem.Root.Seperator));
                for (int index = 0; index < num1; ++index)
                {
                    string str2 = binaryReader.ReadString();
                    if (str2 == name && !this.Contains(name, FileSystem.Root.Seperator))
                        break;
                    int num2 = binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    if (str1 == str2)
                    {
                        Node_block = num2;
                        this.prevLoc = num2;
                        this.createNode(name.Substring(this.IndexOf(name, FileSystem.Root.Seperator) + 1), Node_block, owner);
                        break;
                    }
                    binaryReader.BaseStream.Read();
                    binaryReader.BaseStream.Read();
                    binaryReader.BaseStream.Read();
                    binaryReader.BaseStream.Read();
                    binaryReader.ReadString();
                    binaryReader.ReadString();
                    binaryReader.ReadInt32();
                }
            }
            else
            {
                fsEntry ent = new fsEntry();
                ent.Name = name;
                ent.Attributes = (byte)2;
                ent.Length = 2;
                ent.Pointer = this.getWriteAddress();
                byte[] numArray2 = new byte[1024];
                for (int index = 0; index < 1024; ++index)
                    numArray2[index] = (byte)0;
                ((BlockDevice)this.part).WriteBlock((ulong)this.getWriteAddress(), 2U, numArray2);
                ent.Owner = owner;
                this.increaseWriteAddress(2);
                this.insertEntry(ent, this.prevLoc);
            }
        }

        private int LastIndexOf(string This, char ch)
        {
            int num1 = -1;
            int num2 = 0;
            foreach (int num3 in This)
            {
                if (num3 == (int)ch)
                    num1 = num2;
                ++num2;
            }
            return num1;
        }

        private void Unlink(string item)
        {
            if (!this.CanWrite(item))
                throw new Exception("Access denied!");
            string str = item.Substring(Util.LastIndexOf(item, FileSystem.Root.Seperator) + 1);
            item = item.Substring(0, Util.LastIndexOf(item, FileSystem.Root.Seperator));
            this.cleanName(item);
            item = FileSystem.Root.Seperator.ToString() + item;
            int num1 = 2;
            if (Util.Contains(item, FileSystem.Root.Seperator))
                num1 = this.getNodeAddress(item, 2);
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)(uint)num1, 2U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            BinaryWriter binaryWriter = new BinaryWriter((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray;
            int num2 = binaryReader.ReadInt32();
            bool flag = false;
            binaryWriter.Write(num2 - 1);
            for (int index = 0; index < num2; ++index)
            {
                string data1 = binaryReader.ReadString();
                int data2 = binaryReader.ReadInt32();
                int data3 = binaryReader.ReadInt32();
                byte data4 = binaryReader.BaseStream.Read();
                byte data5 = binaryReader.BaseStream.Read();
                byte data6 = binaryReader.BaseStream.Read();
                byte data7 = binaryReader.BaseStream.Read();
                string data8 = binaryReader.ReadString();
                string data9 = binaryReader.ReadString();
                int data10 = binaryReader.ReadInt32();
                if (data1 == str)
                {
                    flag = true;
                }
                else
                {
                    binaryWriter.Write(data1);
                    binaryWriter.Write(data2);
                    binaryWriter.Write(data3);
                    binaryWriter.Write(data4);
                    binaryWriter.Write(data5);
                    binaryWriter.Write(data6);
                    binaryWriter.Write(data7);
                    binaryWriter.Write(data8);
                    binaryWriter.Write(data9);
                    binaryWriter.Write(data10);
                }
            }
            if (!flag)
                return;
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock((ulong)num1, 2U, binaryWriter.BaseStream.Data);
        }

        public override void Chown(string item, string mod)
        {
            if (!this.CanWrite(item))
                throw new Exception("Error: Access Denied!");
            this.cleanName(item);
            string str = item.Substring(Util.LastIndexOf(item, FileSystem.Root.Seperator) + 1);
            item = item.Substring(0, Util.LastIndexOf(item, FileSystem.Root.Seperator));
            int nodeAddress = this.getNodeAddress(item, 2);
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)nodeAddress, 2U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            BinaryWriter binaryWriter = new BinaryWriter((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray;
            int data1 = binaryReader.ReadInt32();
            binaryWriter.Write(data1);
            for (int index = 0; index < data1; ++index)
            {
                string data2 = binaryReader.ReadString();
                int data3 = binaryReader.ReadInt32();
                int data4 = binaryReader.ReadInt32();
                byte data5 = binaryReader.BaseStream.Read();
                byte data6 = binaryReader.BaseStream.Read();
                byte data7 = binaryReader.BaseStream.Read();
                byte data8 = binaryReader.BaseStream.Read();
                string data9 = binaryReader.ReadString();
                string data10 = binaryReader.ReadString();
                int data11 = binaryReader.ReadInt32();
                if (data2 == str)
                {
                    binaryWriter.Write(data2);
                    binaryWriter.Write(data3);
                    binaryWriter.Write(data4);
                    binaryWriter.Write(data5);
                    binaryWriter.Write(data6);
                    binaryWriter.Write(data7);
                    binaryWriter.Write(data8);
                    binaryWriter.Write(data9);
                    binaryWriter.Write(mod);
                    binaryWriter.Write(data11);
                }
                else
                {
                    binaryWriter.Write(data2);
                    binaryWriter.Write(data3);
                    binaryWriter.Write(data4);
                    binaryWriter.Write(data5);
                    binaryWriter.Write(data6);
                    binaryWriter.Write(data7);
                    binaryWriter.Write(data8);
                    binaryWriter.Write(data9);
                    binaryWriter.Write(data10);
                    binaryWriter.Write(data11);
                }
            }
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock((ulong)nodeAddress, 2U, binaryWriter.BaseStream.Data);
        }

        public override void Chmod(string item, string mod)
        {
            if (!this.CanWrite(item))
                throw new Exception("Error: Access Denied!");
            byte data1 = (byte)Conversions.StringToInt(mod.Substring(0, 1));
            byte data2 = (byte)Conversions.StringToInt(mod.Substring(1, 1));
            byte data3 = (byte)Conversions.StringToInt(mod.Substring(2, 1));
            this.cleanName(item);
            string str = item.Substring(Util.LastIndexOf(item, FileSystem.Root.Seperator) + 1);
            item = item.Substring(0, Util.LastIndexOf(item, FileSystem.Root.Seperator));
            int nodeAddress = this.getNodeAddress(item, 2);
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)nodeAddress, 2U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            BinaryWriter binaryWriter = new BinaryWriter((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray;
            int data4 = binaryReader.ReadInt32();
            binaryWriter.Write(data4);
            for (int index = 0; index < data4; ++index)
            {
                string data5 = binaryReader.ReadString();
                int data6 = binaryReader.ReadInt32();
                int data7 = binaryReader.ReadInt32();
                byte data8 = binaryReader.BaseStream.Read();
                byte data9 = binaryReader.BaseStream.Read();
                byte data10 = binaryReader.BaseStream.Read();
                byte data11 = binaryReader.BaseStream.Read();
                string data12 = binaryReader.ReadString();
                string data13 = binaryReader.ReadString();
                int data14 = binaryReader.ReadInt32();
                if (data5 == str)
                {
                    binaryWriter.Write(data5);
                    binaryWriter.Write(data6);
                    binaryWriter.Write(data7);
                    binaryWriter.Write(data8);
                    binaryWriter.Write(data1);
                    binaryWriter.Write(data2);
                    binaryWriter.Write(data3);
                    binaryWriter.Write(data12);
                    binaryWriter.Write(data13);
                    binaryWriter.Write(data14);
                }
                else
                {
                    binaryWriter.Write(data5);
                    binaryWriter.Write(data6);
                    binaryWriter.Write(data7);
                    binaryWriter.Write(data8);
                    binaryWriter.Write(data9);
                    binaryWriter.Write(data10);
                    binaryWriter.Write(data11);
                    binaryWriter.Write(data12);
                    binaryWriter.Write(data13);
                    binaryWriter.Write(data14);
                }
            }
            binaryWriter.BaseStream.Close();
            ((BlockDevice)this.part).WriteBlock((ulong)nodeAddress, 2U, binaryWriter.BaseStream.Data);
        }

        public override fsEntry[] getLongList(string dir)
        {
            if (dir != "" && dir != FileSystem.Root.Seperator.ToString() && !this.CanRead(dir))
                throw new Exception("Access Denied!");
            if (dir == FileSystem.Root.Seperator.ToString())
                dir = "";
            string[] strArray = this.ListFiles(dir);
            dir = this.cleanName(dir);
            fsEntry[] fsEntryArray = new fsEntry[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
                fsEntryArray[index] = this.readFromNode(Util.cleanName(dir + FileSystem.Root.Seperator.ToString() + strArray[index]));
            return fsEntryArray;
        }

        public override void Move(string f, string dest)
        {
            fsEntry ent = this.readFromNode(f);
            this.Unlink(f);
            ent.Name = dest.Substring(Util.LastIndexOf(dest, FileSystem.Root.Seperator) + 1);
            int Node_block = !Util.Contains(dest, FileSystem.Root.Seperator) ? this.getNodeAddress(f.Substring(0, this.LastIndexOf(f, FileSystem.Root.Seperator)), 2) : this.getNodeAddress(dest.Substring(0, this.LastIndexOf(dest, FileSystem.Root.Seperator)), 2);
            this.insertEntry(ent, Node_block);
        }

        public fsEntry readFromNode(string name)
        {
            name = this.cleanName(name);
            int num1 = !this.Contains(name, FileSystem.Root.Seperator) ? 2 : this.getNodeAddress(name.Substring(0, this.LastIndexOf(name, FileSystem.Root.Seperator)), 2);
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)num1, 2U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray;
            int num2 = binaryReader.ReadInt32();
            for (int index = 0; index < num2; ++index)
            {
                string str1 = binaryReader.ReadString();
                int num3 = binaryReader.ReadInt32();
                int num4 = binaryReader.ReadInt32();
                byte num5 = binaryReader.BaseStream.Read();
                byte num6 = binaryReader.BaseStream.Read();
                byte num7 = binaryReader.BaseStream.Read();
                byte num8 = binaryReader.BaseStream.Read();
                string str2 = binaryReader.ReadString();
                string str3 = binaryReader.ReadString();
                int num9 = binaryReader.ReadInt32();
                if (str1 == name.Substring(this.LastIndexOf(name, FileSystem.Root.Seperator) + 1) || str1 == name)
                    return new fsEntry()
                    {
                        Name = str1,
                        Attributes = num5,
                        Length = num4,
                        Pointer = num3,
                        User = num6,
                        Group = num7,
                        Global = num8,
                        Owner = str3,
                        Time = str2,
                        Checksum = num9
                    };
            }
            throw new Exception("File not found!!!");
        }

        public int getNodeAddress(string name, int Node_block = 2)
        {
            name = this.cleanName(name);
            if (name == "" || name == null)
                return 2;
            if (Node_block == 0)
                Node_block = 2;
            string str1 = "";
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)Node_block, 2U, numArray);
            BinaryReader binaryReader = new BinaryReader((ioStream)new MemoryStream(1024));
            binaryReader.BaseStream.Data = numArray;
            binaryReader.BaseStream.Position = 0;
            int num = binaryReader.ReadInt32();
            if (this.Contains(name, FileSystem.Root.Seperator))
                str1 = name.Substring(0, this.IndexOf(name, FileSystem.Root.Seperator));
            for (int index = 0; index < num; ++index)
            {
                string str2 = binaryReader.ReadString();
                int Node_block1 = binaryReader.ReadInt32();
                binaryReader.ReadInt32();
                if (name.Substring(this.IndexOf(name, FileSystem.Root.Seperator) + 1) == str2 && !this.Contains(name, FileSystem.Root.Seperator))
                    return Node_block1;
                if (str1 == str2 && this.Contains(name, FileSystem.Root.Seperator))
                {
                    Node_block = Node_block1;
                    this.prevLoc = Node_block1;
                    name = name.Substring(this.IndexOf(name, FileSystem.Root.Seperator) + 1);
                    return this.getNodeAddress(name, Node_block1);
                }
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadInt32();
            }
            
            throw new Exception("File not found :(");
        }

        public override byte[] readFile(string name)
        {
            if (!this.CanRead(name))
                throw new Exception("Access Denied!");
            name = this.cleanName(name);
            fsEntry fsEntry = this.readFromNode(name);
            byte[] numArray = new byte[fsEntry.Length * 512];
            ((BlockDevice)this.part).ReadBlock((ulong)fsEntry.Pointer, (uint)fsEntry.Length, numArray);
            MemoryStream memoryStream = new MemoryStream(numArray.Length);
            memoryStream.Data = numArray;
            BinaryReader binaryReader = new BinaryReader((ioStream)memoryStream);
            int length = binaryReader.ReadInt32();
            byte[] s = new byte[length];
            for (int index = 0; index < length; ++index)
                s[index] = binaryReader.BaseStream.Read();
            if (Hash.getCRC(s) != fsEntry.Checksum)
                return this.readFile(name);
            return s;
        }

        public override string[] ListJustFiles(string dir)
        {
            if (dir != "" && dir != FileSystem.Root.Seperator.ToString() && !this.CanRead(dir))
                throw new Exception("Access Denied!");
            dir = this.cleanName(dir);
            List<string> list = new List<string>();
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)this.getNodeAddress(dir, 2), 2U, numArray);
            MemoryStream memoryStream = new MemoryStream(1024);
            memoryStream.Data = numArray;
            BinaryReader binaryReader = new BinaryReader((ioStream)memoryStream);
            int num1 = binaryReader.ReadInt32();
            for (int index = 0; index < num1; ++index)
            {
                string str = binaryReader.ReadString();
                binaryReader.ReadInt32();
                binaryReader.ReadInt32();
                byte num2 = binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadInt32();
                if ((int)num2 != 2)
                    list.Add(str);
            }
            return list.ToArray();
        }

        public override string[] ListDirectories(string dir)
        {
            if (dir != "" && dir != FileSystem.Root.Seperator.ToString() && !this.CanRead(dir))
                throw new Exception("Access Denied!");
            dir = this.cleanName(dir);
            List<string> list = new List<string>();
            byte[] numArray = new byte[1024];
            ((BlockDevice)this.part).ReadBlock((ulong)this.getNodeAddress(dir, 2), 2U, numArray);
            MemoryStream memoryStream = new MemoryStream(1024);
            memoryStream.Data = numArray;
            BinaryReader binaryReader = new BinaryReader((ioStream)memoryStream);
            int num1 = binaryReader.ReadInt32();
            for (int index = 0; index < num1; ++index)
            {
                string str = binaryReader.ReadString();
                binaryReader.ReadInt32();
                binaryReader.ReadInt32();
                byte num2 = binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.BaseStream.Read();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadInt32();
                if ((int)num2 == 2)
                    list.Add(str);
            }
            return list.ToArray();
        }

        public override string[] ListFiles(string dir)
        {
            try
            {
                if (dir != "" && dir != FileSystem.Root.Seperator.ToString() && !this.CanRead(dir))
                    throw new Exception("Access Denied!");
                List<string> list = new List<string>();
                byte[] numArray = new byte[1024];
                
                var nodeAddress = this.getNodeAddress(dir, 2);
                
                ((BlockDevice) this.part).ReadBlock((ulong) (uint) nodeAddress, 2U, numArray);
                dir = this.cleanName(dir);
                MemoryStream memoryStream = new MemoryStream(1024);
                memoryStream.Data = numArray;
                BinaryReader binaryReader = new BinaryReader((ioStream) memoryStream);
                int num = binaryReader.ReadInt32();
                for (int index = 0; index < num; ++index)
                {
                    string str = binaryReader.ReadString();
                    binaryReader.ReadInt32();
                    binaryReader.ReadInt32();
                    binaryReader.BaseStream.Read();
                    binaryReader.BaseStream.Read();
                    binaryReader.BaseStream.Read();
                    binaryReader.BaseStream.Read();
                    binaryReader.ReadString();
                    binaryReader.ReadString();
                    binaryReader.ReadInt32();
                    list.Add(str);
                }
                return list.ToArray();
            }
            catch (Exception ex)
            {
                //An error occured, path maybe not existent or something like this
                Console.WriteLine("Does not exist");
                throw new Exception("Does not exist");
            }
        }

        private string cleanName(string name)
        {
            if (name.Substring(0, 1) == FileSystem.Root.Seperator.ToString())
                name = name.Substring(1, name.Length - 1);
            if (name.Substring(name.Length - 1, 1) == FileSystem.Root.Seperator.ToString())
                name = name.Substring(0, name.Length - 1);
            return name;
        }

        public override void saveFile(byte[] data, string name, string owner)
        {
            name = this.cleanName(name);
         //   if (Util.Contains(name, FileSystem.Root.Seperator) && !this.CanWrite(name.Substring(0, Util.LastIndexOf(name, FileSystem.Root.Seperator))))
          //      throw new Exception("Access denied");
            fsEntry ent1 = new fsEntry();
            ent1.Checksum = Hash.getCRC(data);
            ent1.Attributes = (byte)1;
            BinaryWriter binaryWriter = new BinaryWriter((ioStream)new MemoryStream(new byte[data.Length + 4].Length));
            binaryWriter.Write(data.Length);
            for (int index = 0; index < data.Length; ++index)
                binaryWriter.BaseStream.Write(data[index]);
            binaryWriter.BaseStream.Close();
            data = binaryWriter.BaseStream.Data;
            ent1.Name = name;
            int amount;
            for (amount = 0; amount < data.Length; ++amount)
            {
                byte[] numArray = new byte[512];
                int num = 0;
                while (num < 512)
                    ++num;
            }
            ent1.Length = amount;
            ent1.Pointer = this.getWriteAddress();
            ent1.Attributes = (byte)1;
            ent1.Owner = owner;
            int Node_block = 2;
            if (Util.Contains(ent1.Name, FileSystem.Root.Seperator))
                Node_block = this.getNodeAddress(ent1.Name.Substring(0, this.LastIndexOf(ent1.Name, FileSystem.Root.Seperator)), 2);
            if (this.Contains(name, FileSystem.Root.Seperator))
                ent1.Name = name.Substring(this.LastIndexOf(name, FileSystem.Root.Seperator) + 1);
            foreach (string str in this.ListFiles(name.Substring(0, this.LastIndexOf(name, FileSystem.Root.Seperator))))
            {
                if (str == name.Substring(Util.LastIndexOf(name, FileSystem.Root.Seperator) + 1))
                {
                    if (ent1.Length * 512 <= amount)
                    {
                        fsEntry ent2 = this.readFromNode(name);
                        this.Unlink(name);
                        if (data.Length < 512)
                        {
                            MemoryStream memoryStream = new MemoryStream(512);
                            for (int index = 0; index < data.Length; ++index)
                                memoryStream.Write(data[index]);
                            ((BlockDevice)this.part).WriteBlock((ulong)ent2.Pointer, 1U, memoryStream.Data);
                            this.insertEntry(ent2, Node_block);
                            return;
                        }
                        int index1 = 0;
                        int num = ent2.Pointer;
                        while (index1 < data.Length)
                        {
                            byte[] numArray = new byte[512];
                            for (int index2 = 0; index2 < 512; ++index2)
                            {
                                numArray[index2] = data[index1];
                                ++index1;
                            }
                          ((BlockDevice)this.part).WriteBlock((ulong)num, 1U, numArray);
                            ++num;
                        }
                        this.insertEntry(ent2, Node_block);
                        return;
                    }
                    ent1.Owner = this.readFromNode(name).Owner;
                    ent1.Group = this.readFromNode(name).Group;
                    ent1.Global = this.readFromNode(name).Global;
                    ent1.User = this.readFromNode(name).User;
                    this.Unlink(name);
                    break;
                }
            }
            if (data.Length < 512)
            {
                MemoryStream memoryStream = new MemoryStream(512);
                for (int index = 0; index < data.Length; ++index)
                    memoryStream.Write(data[index]);
                ((BlockDevice)this.part).WriteBlock((ulong)ent1.Pointer, 1U, memoryStream.Data);
                this.insertEntry(ent1, Node_block);
                this.increaseWriteAddress(amount);
            }
            else
            {
                int index1 = 0;
                int writeAddress = this.getWriteAddress();
                this.increaseWriteAddress(amount);
                while (index1 < data.Length)
                {
                    byte[] numArray = new byte[512];
                    for (int index2 = 0; index2 < 512; ++index2)
                    {
                        numArray[index2] = data[index1];
                        ++index1;
                    }
                  ((BlockDevice)this.part).WriteBlock((ulong)writeAddress, 1U, numArray);
                    ++writeAddress;
                }
                this.insertEntry(ent1, Node_block);
            }
        }
    }
}

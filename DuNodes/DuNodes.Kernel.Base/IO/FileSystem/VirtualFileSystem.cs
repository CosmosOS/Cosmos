/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

// I got kind of lazy and decieded to use the code from GDOS....

using System;
using System.Collections.Generic;

namespace DuNodes.Kernel.Base.IO.FileSystem
{
    public class Drive
    {
        public GruntyOS.HAL.StorageDevice Filesystem;
        public string DeviceFile;
    }
    public class VirtualFileSystem
    {
        private List<Drive> fileSystems = new List<Drive>();
        public void AddDrive(Drive fileSystem)
        {
            fileSystems.Add(fileSystem);
        }
        public GruntyOS.HAL.StorageDevice getDrive(string path)
        {
            int DriveNum = (int)(((byte)path.ToCharArray()[0]) - 65);
            return fileSystems[DriveNum].Filesystem;
        }
        public string GetDeviceHandle(string path)
        {
            int DriveNum = (int)(((byte)path.ToCharArray()[0]) - 65);
            return fileSystems[DriveNum].DeviceFile;
        }
        public void Chmod(string f, string perms)
        {
            string RealPath = f.Substring(3);
            getDrive(f).Chmod(RealPath, perms);
        }
        public void Delete(string Path)
        {
            string RealPath = Path.Substring(3);
            getDrive(Path).Delete(RealPath);
        }
        public void Chown(string f, string perms)
        {

            string RealPath = f.Substring(2);
            getDrive(f).Chmod(RealPath, perms);
        }
        public GruntyOS.HAL.fsEntry[] getLongList(string dir)
        {
            string RealPath = dir.Substring(2);
            return getDrive(dir).getLongList(RealPath);
        }
        public string[] ListDirectories(string dir)
        {
            string RealPath = dir.Substring(2);
            return getDrive(dir).ListDirectories(RealPath);
        }
        public string[] ListFiles(string dir)
        {

            string RealPath = dir.Substring(2);

            return getDrive(dir).ListFiles(RealPath);
        }
        public string[] ListJustFiles(string dir)
        {

            string RealPath = dir.Substring(2);
            return getDrive(dir).ListJustFiles(RealPath);
        }
        public void makeDir(string name, string owner)
        {
            string RealPath = name.Substring(2);
            getDrive(name).makeDir(RealPath, owner);
        }
        public void Move(string s1, string s2)
        {
            throw new NotImplementedException();
        }
        public byte[] readFile(string name)
        {
            string RealPath = name.Substring(2);
            return getDrive(name).readFile(RealPath);
        }
        public void saveFile(byte[] data, string name, string owner)
        {
            string RealPath = name.Substring(2);
            getDrive(name).saveFile(data, RealPath, owner);
        }
    }

}

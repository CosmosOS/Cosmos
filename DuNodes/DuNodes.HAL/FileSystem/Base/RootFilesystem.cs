using System.Collections.Generic;

namespace DuNodes.HAL.FileSystem.Base
{
    public class RootFilesystem : StorageDevice
    {
        public List<MountPoint> Mountpoints = new List<MountPoint>();
        public char Seperator = '/';

        public void Mount(string dir, StorageDevice sd)
        {
            MountPoint mountPoint = new MountPoint();
            dir = Util.cleanName(dir);
            mountPoint.Path = dir;
            mountPoint.device = sd;
            this.Mountpoints.Add(mountPoint);
        }

        public bool isMountPoint(string dir)
        {
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (Util.cleanName(this.Mountpoints[index].Path) == Util.cleanName(dir))
                    return true;
            }
            return false;
        }

        public void Unmount(string device)
        {
            List<MountPoint> list = new List<MountPoint>();
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (Util.cleanName(this.Mountpoints[index].Path) != Util.cleanName(device))
                    list.Add(this.Mountpoints[index]);
            }
            this.Mountpoints = list;
        }

        public override void Chmod(string dir, string perms)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    (dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path &&
                     this.Mountpoints[index].Path != ""))
                {
                    this.Mountpoints[index].device.Chmod(dir.Substring(this.Mountpoints[index].Path.Length), perms);
                    return;
                }
            }
            this.Mountpoints[0].device.Chmod(dir, perms);
        }

        public override void Chown(string dir, string perms)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    (dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path &&
                     this.Mountpoints[index].Path != ""))
                {
                    this.Mountpoints[index].device.Chown(dir.Substring(this.Mountpoints[index].Path.Length), perms);
                    return;
                }
            }
            this.Mountpoints[0].device.Chown(dir, perms);
        }

        public override string[] ListJustFiles(string dir)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                    return
                        this.Mountpoints[index].device.ListJustFiles(dir.Substring(this.Mountpoints[index].Path.Length));
            }
            return this.Mountpoints[0].device.ListJustFiles(dir);
        }

        public override fsEntry[] getLongList(string dir)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                    return this.Mountpoints[index].device.getLongList(dir.Substring(this.Mountpoints[index].Path.Length));
            }
            return this.Mountpoints[0].device.getLongList(dir);
        }

        public override void Move(string dir, string dir2)
        {
            dir = Util.cleanName(dir);
            dir2 = Util.cleanName(dir2);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                {
                    this.Mountpoints[index].device.Move(dir.Substring(this.Mountpoints[index].Path.Length),
                        dir2.Substring(this.Mountpoints[index].Path.Length));
                    return;
                }
            }
            this.Mountpoints[0].device.Move(dir, dir2);
        }

        public override void makeDir(string dir, string owner)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                {
                    this.Mountpoints[index].device.makeDir(dir.Substring(this.Mountpoints[index].Path.Length), owner);
                    return;
                }
            }
            this.Mountpoints[0].device.makeDir(dir, owner);
        }

        public override byte[] readFile(string dir)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                    return this.Mountpoints[index].device.readFile(dir.Substring(this.Mountpoints[index].Path.Length));
            }
            return this.Mountpoints[0].device.readFile(dir);
        }

        public override void saveFile(byte[] data, string dir, string owner)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                {
                    this.Mountpoints[index].device.saveFile(data, dir.Substring(this.Mountpoints[index].Path.Length),
                        owner);
                    return;
                }
            }
            this.Mountpoints[0].device.saveFile(data, dir, owner);
        }

        public override void Delete(string dir)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "" &&
                    dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                {
                    this.Mountpoints[index].device.Delete(dir.Substring(this.Mountpoints[index].Path.Length));
                    return;
                }
            }
            this.Mountpoints[0].device.Delete(dir);
        }

        public override string[] ListDirectories(string dir)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "")
                {
                    if (dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                        return
                            this.Mountpoints[index].device.ListDirectories(
                                dir.Substring(this.Mountpoints[index].Path.Length));
                }
                else if (!(this.Mountpoints[index].Path == ""))
                    ;
            }
            return this.Mountpoints[0].device.ListDirectories(dir);
        }

        public override string[] ListFiles(string dir)
        {
            dir = Util.cleanName(dir);
            for (int index = 0; index < this.Mountpoints.Count; ++index)
            {
                if (this.Mountpoints[index].Path.Length <= dir.Length && this.Mountpoints[index].Path != "")
                {
                    if (dir.Substring(0, this.Mountpoints[index].Path.Length) == this.Mountpoints[index].Path)
                        return
                            this.Mountpoints[index].device.ListFiles(dir.Substring(this.Mountpoints[index].Path.Length));
                }
                else if (!(this.Mountpoints[index].Path == ""))
                    ;
            }
            return this.Mountpoints[0].device.ListFiles(dir);
        }

        public bool FileExist(string dir, string filename)
        {
            //TODO Support multiple partition
            dir = Util.cleanName(dir);
            var files = this.ListFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] == filename)
                    return true;
            }
            return false;
        }

        public bool DirectoryExist(string path,string dirname)
        {
            //TODO Support multiple partition
            path = Util.cleanName(path);

            var dirs = this.ListDirectories(path);

            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirname == dirs[i])
                    return true;
            }
            return false;
        }
    }
}

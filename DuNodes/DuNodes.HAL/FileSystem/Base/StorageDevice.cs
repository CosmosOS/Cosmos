namespace DuNodes.HAL.FileSystem.Base
{
    public abstract class StorageDevice
    {
        public string DriveLabel = "";
        public char Label;

        public abstract string[] ListFiles(string dir);

        public abstract byte[] readFile(string name);

        public abstract void saveFile(byte[] data, string name, string owner);

        public abstract void makeDir(string name, string owner);

        public abstract string[] ListDirectories(string dir);

        public abstract string[] ListJustFiles(string dir);

        public abstract void Move(string s1, string s2);

        public abstract fsEntry[] getLongList(string dir);

        public abstract void Delete(string Path);

        public abstract void Chmod(string f, string perms);

        public abstract void Chown(string f, string perms);
    }
}

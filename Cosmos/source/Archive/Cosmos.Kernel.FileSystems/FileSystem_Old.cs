using System;
using System.Collections.Generic;
using System.IO;

namespace Cosmos.Sys.FileSystem {
	public abstract class FileSystem_Old 
    {
        public virtual FSDirectory GetDirectory(string path)
        {
            FSObject o = GetObject(path);
            if (o is FSDirectory)
                return o as FSDirectory;
            throw new Exception("DiretoryNotFound");
        }

        public virtual FSFile GetFile(string path)
        {
            FSObject o = GetObject(path);
            if (o is FSFile)
                return o as FSFile;
            throw new Exception("FileNotFound");
        }

        protected abstract FSObject GetObject(string path);
	}

    public abstract class FSObject
    {
        public abstract string Name
        {
            get;
            set;
        }

        public abstract int Size
        {
            get;
        }
                
        public abstract bool Delete(string name);
    }

    public abstract class FSFile : FSObject
    {
        public abstract Stream GetStream();
    }

    public abstract class FSDirectory : FSObject
    {
        protected FSDirectory[] SubDirectorys;
        protected FSFile[] Files;

        public FSDirectory GetDirectory(string name)
        {
            foreach (var dir in SubDirectorys)
                if (dir.Name == name)
                    return dir;
            throw new Exception("DirectoryNotFound");
        }

        public FSFile GetFile(string name)
        {
            foreach (var file in Files)
                if (file.Name == name)
                    return file;
            throw new Exception("fileNotFound");
        }

        public abstract FSFile CreateFile(string name);
        public abstract FSDirectory CreateDirectory(string name);

    
    }
}   
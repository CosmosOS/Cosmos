using DuNodes.Kernel.Base.IO;

namespace DuNodes.HAL.FileSystem.Base
{
    public class FileStream : ioStream
    {
        private string fname = "";
        private string fmode = "";

        public FileStream(string fname, string mode)
        {
            this.fname = fname;
            this.init(7000);
            this.fmode = mode;
            if (!(mode == "r"))
                return;
            this.init(FileSystem.Root.readFile(fname).Length);
            this.Data = FileSystem.Root.readFile(fname);
        }

        public override void Flush()
        {
            base.Flush();
        }

        public override void Write(byte i)
        {
            base.Write(i);
        }

        public override byte Read()
        {
            return base.Read();
        }

        public override void Close()
        {
            if (!(this.fmode == "w"))
                return;
            MemoryStream memoryStream = new MemoryStream(this.Position);
            for (int index = 0; index < this.Position; ++index)
                memoryStream.Write(this.Data[index]);
            this.Data = memoryStream.Data;
            FileSystem.Root.saveFile(this.Data, this.fname, "DN");
        }
    }
}

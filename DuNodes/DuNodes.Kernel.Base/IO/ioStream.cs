namespace DuNodes.Kernel.Base.IO
{
    public abstract class ioStream
    {
        private bool Resize = true;
        public int Position;
        public byte[] Data;

        public virtual void Write(byte i)
        {
            if (this.Data.Length + 1 < this.Position)
            {
                byte[] numArray = new byte[this.Data.Length + 1000];
                for (int index = 0; index < this.Data.Length; ++index)
                    numArray[index] = i;
                this.Data = numArray;
            }
            this.Data[this.Position] = i;
            ++this.Position;
        }

        public virtual byte Read()
        {
            ++this.Position;
            return this.Data[this.Position - 1];
        }

        public virtual void Flush()
        {
            this.Data = (byte[])null;
            this.Position = 0;
        }

        public virtual void Close()
        {
            Flush();
        }

        public void init(int size)
        {
            this.Resize = false;
            this.Data = new byte[size];
        }
    }
}

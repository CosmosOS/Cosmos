using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace CoreLib.IPC.BufferManager
{
    //TODO Benchmark vs normal stream 
    // A normal stream incurs a copying cost as the data is read from the buffers ,
    // in normal .NET this copying is needed as we go from native buffers to managed ,
    // however once read access to the buffer copied to is fast allowing for loops.
    // ZeroCOpy stream can change the data when the appropriate buffer is finished ,
    // this means it needs to understand the stream layout which could lead to security issues 
    // ( need to make sure the only buffers they can touch are valid ones 
    // or else we need a trusted reader for each format). In addition reading it will require a foreach loop and enumerator. 

    //Gut feel is Zero COpy is much faster where the buffer is read once and of non trivial size.
    // use for things like web servers eg TCP Buffers read into Stream which is then read into an encoded string.


    /// <summary>
    ///  Better performance over large streams note cost of creating Enumerator.
    ///  
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZeroCopyStream<T> : IEnumerable<T>, IDisposable where T : struct
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator() as IEnumerator;
        }

        public void Dispose()
        {
           
        }

        //public  bool CanRead
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public  bool CanSeek
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public  bool CanWrite
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public  void Flush()
        //{
        //    throw new NotImplementedException();
        //}

        //public  long Length
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public  long Position
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}


        ////we remove from buffers and read into a congious buffer. 
        //public  int Read(byte[] buffer, int offset, int count)
        //{
        //    throw new NotImplementedException();
        //}

        //public  long Seek(long offset, SeekOrigin origin)
        //{
        //    throw new NotImplementedException();
        //}

        //public  void SetLength(long value)
        //{
        //    throw new NotImplementedException();
        //}

        //public  void Write(byte[] buffer, int offset, int count)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

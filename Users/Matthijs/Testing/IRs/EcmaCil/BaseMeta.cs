using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public abstract class BaseMeta: IDisposable
    {
        protected BaseMeta()
        {
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private Dictionary<int, object> mData;
        public IDictionary<int, object> Data
        {
            get
            {
                if (mData == null)
                {
                    mData = new Dictionary<int, object>();
                }
                return mData;
            }
        }

        //public override int GetHashCode()
        //{
        //    return MetaId;
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.VS.Windows
{
    public class StateStorer
    {
        protected object mCurrLineId_Lock = new object();
        protected string mCurrLineId = null;
        public string CurrLineId
        {
            get
            {
                return mCurrLineId;
            }
            set
            {
                lock (mCurrLineId_Lock)
                {
                    mCurrLineId = value;

                    if(mCurrLineId != null && !mStates.ContainsKey(mCurrLineId))
                    {
                        mStates.Add(mCurrLineId, new Dictionary<string, byte[]>());
                    }
                }
            }
        }

        //LineId->StateId->Data for that state
        protected Dictionary<string, Dictionary<string, byte[]>> mStates = new Dictionary<string, Dictionary<string, byte[]>>();

        public StateStorer()
        {

        }

        public void StoreState(string stateId, byte[] data)
        {
            Dictionary<string, byte[]> currLineStates = null;
            lock (mCurrLineId_Lock)
            {
                currLineStates = mStates[mCurrLineId];
            }
            if (currLineStates.ContainsKey(stateId))
            {
                currLineStates.Remove(stateId);
            }

            currLineStates.Add(stateId, data);
        }
        public byte[] RetrieveState(string lineId, string stateId)
        {
            if (mStates.ContainsKey(lineId))
            {
                Dictionary<string, byte[]> currLineStates = mStates[lineId];
                if(currLineStates.ContainsKey(stateId))
                {
                    return currLineStates[stateId];
                }
            }
            return null;
        }
        public bool ContainsStatesForLine(string lineId)
        {
            return mStates.ContainsKey(lineId);
        }

        public void ClearState()
        {
            lock (mCurrLineId_Lock)
            {
                mStates = new Dictionary<string, Dictionary<string, byte[]>>();
                //Ensure current line gets added to new dictionary
                CurrLineId = CurrLineId;
            }
        }
    }
}

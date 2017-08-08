using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FS = Cosmos.System.Filesystem.FAT.FatFileSystem;
namespace LWatson
{
	public class FileSystem
	{
		// Currently we map to the Windows scheme of single lettter: for drives. Cosmos will 
		// NOT do this in the future, but it will be able to map paths to things that look like
		// drive letters for compatibility with Windows code.
		// For now we use Dictionary for simplicity, but in future this will change.
		//static protected Dictionary<string, FileSystem> mMappings = new Dictionary<string, FileSystem>();

		static protected Mapping[] mFS; 
		protected class Mapping
		{

			// public string CosmosKey;
			public string Key;

			public FS FileSystem;
			public Mapping(string k, FS f = default(FS))
			{
				this.Key = k;
				this.FileSystem = f;
			}
		}
		static bool doneInit = false;
		static void Init()
		{
			string[] wrk = "C,D,E,F,G,H,I,J,K,L,M,N,O.P,Q,R,S,T,U,V,W,X,Y,Z,C1,D1,E1,F1,G1,H1,I1,J1,K1,L1,M1,N1,O.P1,Q1,R1,S1,T1,U1,V1,W1,X1,Y1,Z1,".Split(new char[] {','});
			mFS = new Mapping[wrk.Length];
			for (int i = 0; i < wrk.Length; i++) mFS[i] = new Mapping(wrk[i]);
			doneInit = true;
		}
		static public void AddMapping(FS aFileSystem)
		{
			if (!doneInit) Init();
			int p = 0;
			while (mFS[p].FileSystem != default(FS)) p++;
			if (p < mFS.Length) mFS[p].FileSystem = aFileSystem;
		}
		static public FS GetMapping(string aPath)
		{
			for (int i = 0; i < 78; i++)
			{
				if (mFS[i].Key == aPath) return mFS[i].FileSystem;
			}
			return default(FS);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.FileSystem.ext2 {
	public class Ext2FS: FileSystem {
		public Ext2FS(Hardware.BlockDevice aBackend)
			: base(aBackend) {
		}

		public override ulong Read(string[] aFile, ulong aStart, ulong aCount, byte[] aBuffer) {
			throw new NotImplementedException();
		}

		public override ulong Write(string[] aFile, ulong aStart, ulong aCount, byte[] aBuffer) {
			throw new NotImplementedException();
		}

		public override void DeleteFile(string[] aFile) {
			throw new NotImplementedException();
		}

		public override void CreateFile(string[] aFile) {
			throw new NotImplementedException();
		}

		public override void MoveFile(string[] aSource, string[] aDest) {
			throw new NotImplementedException();
		}

		public override string[] GetDirContents(string[] aDir) {
			throw new NotImplementedException();
		}
	}
}
using System;
using System.Collections.Generic;

namespace Cosmos.Kernel.FileSystems {
	public abstract class BaseFileSystem {
		protected readonly Hardware.BlockDevice mBackend;
		protected BaseFileSystem(Hardware.BlockDevice aBackend) {
			if (aBackend == null) {
				throw new ArgumentNullException("aBackend");
			}
			mBackend = aBackend;
		}

		public abstract ulong Read(string[] aFile, ulong aStart, ulong aCount, byte[] aBuffer);
		public abstract ulong Write(string[] aFile, ulong aStart, ulong aCount, byte[] aBuffer);
		public abstract void DeleteFile(string[] aFile);
		public abstract void CreateFile(string[] aFile);
		public abstract void MoveFile(string[] aSource, string[] aDest);
		public abstract string[] GetDirContents(string[] aDir);
	}
}
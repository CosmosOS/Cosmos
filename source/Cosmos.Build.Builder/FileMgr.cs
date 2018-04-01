﻿using System;
using System.IO;

namespace Cosmos.Build.Builder
{
    internal class FileMgr : IDisposable
    {
        private string _srcPath;
        private string _destPath;
        private ILogger _logger;

        public FileMgr(ILogger logger, string srcPath, string destPath)
        {
            _srcPath = srcPath;
            _destPath = destPath;
            _logger = logger;
        }

        public void ResetReadOnly(string aPathname)
        {
            var xAttrib = File.GetAttributes(aPathname);
            if ((xAttrib & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(aPathname, xAttrib & ~FileAttributes.ReadOnly);
            }
        }

        public void Copy(string aSrcPathname, bool aClearReadonly = true)
        {
            Copy(aSrcPathname, Path.GetFileName(aSrcPathname), aClearReadonly);
        }
        public void Copy(string aSrcPathname, string aDestPathname, bool aClearReadonly = true)
        {
            _logger.LogMessage("Copy");

            string xSrc = Path.Combine(_srcPath, aSrcPathname);
            _logger.LogMessage("  From: " + xSrc);

            string xDest = Path.Combine(_destPath, aDestPathname);
            _logger.LogMessage("  To: " + xDest);

            // Copying files that are in TFS often they will be read only, so need to kill this file before copy
            // We don't use TFS any more.. but left the code.
            if (aClearReadonly && File.Exists(xDest))
            {
                ResetReadOnly(xDest);
            }
            File.Copy(xSrc, xDest, true);
            ResetReadOnly(xDest);
        }

        // Dummy pattern to allow scoping via using.
        // Hacky, but for what we are doing its fine and the GC
        // effects are negligible in our usage.
        protected virtual void Dispose(bool aDisposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}

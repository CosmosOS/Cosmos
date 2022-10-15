using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO.Compression;
using System.Diagnostics;

namespace Cosmos.Build.Tasks
{
    public class Gzip : Task
    {
        [Required]
        public string BinFile { get; set; }

        [Required]
        public string OutputFile { get; set; }
        
        public override bool Execute()
        {
            if (!File.Exists(BinFile))
            {
                Log.LogError($"'{BinFile}' is missing!");
                return false;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            using FileStream fileStream = File.Open(BinFile, FileMode.Open);
            using FileStream compressedFileStream = File.Create(OutputFile);
            using var gzip = new GZipStream(compressedFileStream, CompressionLevel.Optimal);
            fileStream.CopyTo(gzip);

            stopwatch.Stop();

            int beforeMB = (int)(new FileInfo(BinFile).Length / 1024 / 1024);
            int afterMB = (int)(new FileInfo(OutputFile).Length / 1024 / 1024);

            Log.LogMessage(MessageImportance.High, $"Compressed '{BinFile}' with gzip in {stopwatch.ElapsedMilliseconds} ms ({beforeMB} MB -> {afterMB} MB)");

            return true;
        }
    }
}

# Vendored: SharpZipLib inflater

The DEFLATE/zlib decompression classes of
[SharpZipLib](https://github.com/icsharpcode/SharpZipLib) (MIT), whitespace-reformatted to the repository style (dotnet format) but
otherwise unmodified except for `SharpZipBaseException.cs`, which is a minimal local replacement
for the original exception hierarchy (the inflater only ever constructs it
with a message).

Pure safe managed code — no `unsafe`, no P/Invoke — so malformed input fails
with an exception instead of corrupting kernel memory. Used by the PNG
decoder (`src/Cosmos.Kernel.System/Graphics/Images/BigGustave/`) and available for any other
consumer of raw or zlib-wrapped DEFLATE streams via
`ICSharpCode.SharpZipLib.Zip.Compression.Inflater`.

See `docs/credits.md` for the full third-party list.

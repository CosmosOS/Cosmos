# Vendored: BigGustave (PNG decoder)

Decode-only subset of [BigGustave](https://github.com/EliotJones/BigGustave)
(Unlicense, public domain), as integrated for Cosmos by
[CosmosPNG](https://github.com/Szymekk44/CosmosPNG) (Unlicense).

Local changes:

- The encoder (`PngBuilder`) is not included — the kernel only decodes.
- `Ionic.Zlib` is replaced by the vendored SharpZipLib inflater
  (`src/Cosmos.Kernel.System/IO/Compression/SharpZipLib/`), which also
  verifies the zlib header and Adler-32 checksum.
- CosmosPNG's Gen2 enum workarounds are reverted to upstream semantics
  (they mis-mapped PNG color types 3, 4 and 6); IHDR fields are validated
  against the PNG specification in `PngOpener.ReadImageHeader`.
- All types are `internal`; the public API is the `Png` image class
  (namespace `Cosmos.Kernel.System.Graphics`) defined in
  `src/Cosmos.Kernel.System/Graphics/Images/Png.cs`, one directory up.

See `docs/credits.md` for the full third-party list.

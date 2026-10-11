# Vendored: LunarFonts

The TrueType loader and rasterizer from
[LunarFonts](https://github.com/Relfos/LunarFonts) (MIT) by Sérgio Flores, a
hand-written port of [stb_truetype](https://github.com/nothings/stb) to pure
safe C#. Taken at upstream commit `6501f54` — the last synchronous version,
before OpenType-SVG support pulled `async`/`Task`, LINQ and `System.Drawing`
into the file. (Do not vendor the older `fa96fc9`: it truncates the glyf
point-data offset to 16 bits, so every glyph stored past byte 65535 of the
glyf table rasterizes empty; `6501f54` is that one-cast fix.) Local changes:

- Top-level types are `internal`; the public kernel API is
  `Cosmos.Kernel.System.Graphics.Fonts.TrueTypeFont`
  (`src/Cosmos.Kernel.System/Graphics/Fonts/TrueTypeFont.cs`).
- A leftover debug `Console.WriteLine` in the active-edge resort loop of
  `RasterizeSortedEdges` was removed (it fires on any glyph with overlapping
  contour edges).
- Whitespace-reformatted to the repository style (dotnet format), otherwise
  unmodified.

Pure safe managed code — no `unsafe`, no P/Invoke — so a malformed font file
fails with an exception instead of corrupting kernel memory. Kerning uses the
legacy `kern` table only; fonts that ship kerning exclusively in the GPOS
table render without pair kerning (upstream added GPOS support later, in the
async rewrite this snapshot predates).

The TrueType integration follows
[CosmosTTF](https://github.com/GoldenretriverYT/CosmosTTF) (MIT), which first
brought LunarFonts to Cosmos Gen2.

See `docs/credits.md` for the full third-party list.

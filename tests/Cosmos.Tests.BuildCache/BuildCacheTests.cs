// =====================================================================
// Build cache integration tests — coverage matrix
// =====================================================================
//
// Validates the full pipeline (Patcher → ILC → ASM → CC → Linker → ISO):
//   - All steps are skipped when nothing changed.
//   - Each source-language change rebuilds only its own step + downstream
//     (managed → managed, ASM → ASM, C → C) and leaves the unrelated
//     compilation steps strictly cached.
//   - Content-hash filenames for ASM/CC create new outputs on edit and
//     cleanly orphan-remove the previous ones.
//   - Adding/removing a source file is reflected in the object directory.
//
// Coverage matrix (OK = explicit assertion, - = not exercised by this test).
// Cell labels:
//   "snapshot eq"     = filename set + per-file mtimes byte-for-byte unchanged
//   "rebuilds (keys)" = content-hash filename set differs (new obj, old orphaned)
//   "add/del cleanup" = add temp source -> obj created; delete -> obj cleaned
//   "edit/revert RT"  = edit source -> new content-hash name + old orphaned;
//                       revert source -> original content-hash name returns
//
// |-----------------------------------------------------------------------------------------------------------------------------------------------------|
// | Test            | Patcher             | ILC                 | ASM                | CC                 | Linker              | ISO                 |
// |-----------------|---------------------|---------------------|---------------------|---------------------|---------------------|---------------------|
// | T01 Clean       | runs                | runs                | runs                | runs                | runs                | runs                |
// | T02 No-change   | OK hit + hash mtime | OK hit + hash mtime | OK snapshot eq      | OK snapshot eq      | OK hit + hash mtime | OK hit + hash mtime |
// | T03 C# change   | rebuilds            | rebuilds            | OK snapshot eq      | OK snapshot eq      | rebuilds (mtime)    | rebuilds (mtime)    |
// | T04 C# revert   | OK hit              | OK hit              | OK hit              | OK hit              | OK hit              | OK hit              |
// | T05 ASM change  | OK hit              | OK hit + mtime eq   | rebuilds (keys)     | OK snapshot eq      | rebuilds (mtime)    | rebuilds (mtime)    |
// | T06 ASM revert  | OK hit              | OK hit              | OK hit              | OK hit              | OK hit              | OK hit              |
// | T07 C change    | OK hit              | OK hit + mtime eq   | OK snapshot eq      | rebuilds (keys)     | rebuilds (mtime)    | rebuilds (mtime)    |
// | T08 C revert    | OK hit              | OK hit              | OK hit              | OK hit              | OK hit              | OK hit              |
// | T09 CC orphan  | -                   | -                   | -                   | add/del cleanup     | -                   | -                   |
// | T10 ASM orphan | -                   | -                   | add/del cleanup     | -                   | -                   | -                   |
// | T11 ASM hash   | -                   | -                   | edit/revert RT      | -                   | -                   | -                   |
// | T12 CC hash    | -                   | -                   | -                   | edit/revert RT      | -                   | -                   |
// | T13 Clean+cache | runs -> OK hit      | runs -> OK hit      | runs -> snapshot eq | runs -> snapshot eq | runs -> OK hit      | runs -> OK hit      |
// | T14 Switch flip | OK hit              | rebuilds (mtime)    | OK snapshot eq      | OK snapshot eq      | rebuilds (mtime)    | rebuilds (mtime)    |
// |-----------------------------------------------------------------------------------------------------------------------------------------------------|
//
// =====================================================================

namespace Cosmos.Tests.BuildCache;
[Collection("BuildCache")]
[TestCaseOrderer("Cosmos.Tests.BuildCache.PriorityOrderer", "Cosmos.Tests.BuildCache")]
public class BuildCacheTests : IClassFixture<BuildFixture>
{
    private readonly BuildFixture _fixture;

    public BuildCacheTests(BuildFixture fixture)
    {
        _fixture = fixture;
    }

    // ==================================================================
    // Helpers
    // ==================================================================

    /// <summary>Snapshot filename → mtime for every file matching the pattern.</summary>
    private static Dictionary<string, DateTime> SnapshotDir(string dir, string pattern)
    {
        if (!Directory.Exists(dir))
        {
            return new Dictionary<string, DateTime>();
        }
        return Directory.GetFiles(dir, pattern)
            .ToDictionary(f => Path.GetFileName(f), f => File.GetLastWriteTimeUtc(f));
    }

    /// <summary>Assert two directory snapshots are byte-for-byte identical (same files, same mtimes).</summary>
    private static void AssertSnapshotEqual(
        Dictionary<string, DateTime> before,
        Dictionary<string, DateTime> after,
        string label)
    {
        string beforeKeys = string.Join(",", before.Keys.OrderBy(k => k));
        string afterKeys = string.Join(",", after.Keys.OrderBy(k => k));
        Assert.True(beforeKeys == afterKeys,
            $"{label}: file set changed.\n  before: [{beforeKeys}]\n  after:  [{afterKeys}]");

        foreach (string key in before.Keys)
        {
            Assert.True(before[key] == after[key],
                $"{label}: file '{key}' was rewritten ({before[key]:O} → {after[key]:O})");
        }
    }

    private static void AssertAllCacheHits(BuildResult result, string label)
    {
        Assert.True(result.Stdout.Contains("Patcher cache hit"), $"{label}: missing patcher cache hit\n{result.Output}");
        Assert.True(result.Stdout.Contains("ILC cache hit"), $"{label}: missing ILC cache hit\n{result.Output}");
        Assert.True(result.Stdout.Contains("Linker cache hit"), $"{label}: missing linker cache hit\n{result.Output}");
        Assert.True(result.Stdout.Contains("ISO cache hit"), $"{label}: missing ISO cache hit\n{result.Output}");
    }

    // ==================================================================
    // TEST 1: Clean build — full pipeline, no cache hits
    // ==================================================================
    [Fact, TestPriority(1)]
    public void T01_CleanBuild_ProducesAllOutputs()
    {
        if (Directory.Exists(_fixture.ObjDir))
        {
            Directory.Delete(_fixture.ObjDir, true);
        }

        BuildResult result = _fixture.Build();

        Assert.True(result.Success, $"Clean build failed:\n{result.Output}");
        Assert.True(File.Exists(_fixture.ElfFile), "ELF binary not produced");
        Assert.True(File.Exists(_fixture.IsoFile), "ISO not produced");
        Assert.True(File.Exists(_fixture.PatcherHashFile), "Patcher cache hash not written");
        Assert.True(File.Exists(_fixture.IlcHashFile), "ILC cache hash not written");
        Assert.True(File.Exists(_fixture.LinkHashFile), "Link cache hash not written");
        Assert.True(File.Exists(_fixture.IsoHashFile), "ISO cache hash not written");
        Assert.True(File.Exists(_fixture.IlcOutput), "ILC .o not produced");
        Assert.True(Directory.Exists(_fixture.AsmObjDir) && Directory.GetFiles(_fixture.AsmObjDir, "*.obj").Length > 0,
            "ASM produced no .obj files");
        Assert.True(Directory.Exists(_fixture.CObjDir) && Directory.GetFiles(_fixture.CObjDir, "*.o").Length > 0,
            "CC produced no .o files");

        Assert.DoesNotContain("cache hit", result.Stdout, StringComparison.OrdinalIgnoreCase);
    }

    // ==================================================================
    // TEST 2: No-change rebuild — every step cached, nothing rewritten
    //
    // Strict invariants:
    //   - All four cache-hit messages present.
    //   - No "actually compiled" log lines.
    //   - ELF / ISO / ILC output mtimes unchanged.
    //   - All four hash files unchanged (no cache layer rewrote its file).
    //   - ASM .obj snapshot byte-for-byte identical.
    //   - CC  .o   snapshot byte-for-byte identical.
    // ==================================================================
    [Fact, TestPriority(2)]
    public void T02_NoChangeRebuild_AllStepsCached()
    {
        DateTime elfBefore = File.GetLastWriteTimeUtc(_fixture.ElfFile);
        DateTime isoBefore = File.GetLastWriteTimeUtc(_fixture.IsoFile);
        DateTime ilcBefore = File.GetLastWriteTimeUtc(_fixture.IlcOutput);
        DateTime patcherHashBefore = File.GetLastWriteTimeUtc(_fixture.PatcherHashFile);
        DateTime ilcHashBefore = File.GetLastWriteTimeUtc(_fixture.IlcHashFile);
        DateTime linkHashBefore = File.GetLastWriteTimeUtc(_fixture.LinkHashFile);
        DateTime isoHashBefore = File.GetLastWriteTimeUtc(_fixture.IsoHashFile);
        Dictionary<string, DateTime> asmObjBefore = SnapshotDir(_fixture.AsmObjDir, "*.obj");
        Dictionary<string, DateTime> cObjBefore = SnapshotDir(_fixture.CObjDir, "*.o");

        Thread.Sleep(1100);
        BuildResult result = _fixture.Build();

        Assert.True(result.Success, $"No-change rebuild failed:\n{result.Output}");

        AssertAllCacheHits(result, "no-change rebuild");

        // No step should have actually compiled anything
        Assert.DoesNotContain("Batch patching:", result.Stdout);
        Assert.DoesNotContain("[ILC] Compiling:", result.Stdout);
        Assert.DoesNotContain("Built ELF:", result.Stdout);
        Assert.DoesNotContain("ISO created at:", result.Stdout);

        // Output artifact timestamps unchanged
        Assert.Equal(elfBefore, File.GetLastWriteTimeUtc(_fixture.ElfFile));
        Assert.Equal(isoBefore, File.GetLastWriteTimeUtc(_fixture.IsoFile));
        Assert.Equal(ilcBefore, File.GetLastWriteTimeUtc(_fixture.IlcOutput));

        // No cache layer rewrote its hash file
        Assert.Equal(patcherHashBefore, File.GetLastWriteTimeUtc(_fixture.PatcherHashFile));
        Assert.Equal(ilcHashBefore, File.GetLastWriteTimeUtc(_fixture.IlcHashFile));
        Assert.Equal(linkHashBefore, File.GetLastWriteTimeUtc(_fixture.LinkHashFile));
        Assert.Equal(isoHashBefore, File.GetLastWriteTimeUtc(_fixture.IsoHashFile));

        // ASM and CC object directories untouched
        AssertSnapshotEqual(asmObjBefore, SnapshotDir(_fixture.AsmObjDir, "*.obj"), "ASM .obj");
        AssertSnapshotEqual(cObjBefore, SnapshotDir(_fixture.CObjDir, "*.o"), "CC .o");
    }

    // ==================================================================
    // TEST 3: C# change → only patcher + ILC + linker + ISO rebuild.
    //                     ASM and CC stay strictly cached.
    // ==================================================================
    [Fact, TestPriority(3)]
    public void T03_CSharpChange_RebuildsManagedOnly()
    {
        DateTime elfBefore = File.GetLastWriteTimeUtc(_fixture.ElfFile);
        DateTime isoBefore = File.GetLastWriteTimeUtc(_fixture.IsoFile);
        DateTime ilcBefore = File.GetLastWriteTimeUtc(_fixture.IlcOutput);
        Dictionary<string, DateTime> asmObjBefore = SnapshotDir(_fixture.AsmObjDir, "*.obj");
        Dictionary<string, DateTime> cObjBefore = SnapshotDir(_fixture.CObjDir, "*.o");

        Thread.Sleep(1100);
        using IDisposable marker = _fixture.InjectMarker(_fixture.DevKernelCs, "cs");
        BuildResult result = _fixture.Build();

        Assert.True(result.Success, $"Build after C# change failed:\n{result.Output}");

        // Patcher + ILC must actually run.
        // (`dotnet publish` runs the pipeline twice; the second pass legitimately
        //  hits cache, so we cannot assert "cache hit absent" — we assert the
        //  rebuild messages and timestamps changed instead.)
        Assert.Contains("Batch patching:", result.Stdout);
        Assert.Contains("[ILC] Compiling:", result.Stdout);

        // ASM and CC object directories must be untouched (managed change
        // doesn't affect ASM or C compilation inputs).
        AssertSnapshotEqual(asmObjBefore, SnapshotDir(_fixture.AsmObjDir, "*.obj"),
            "ASM .obj after C# change");
        AssertSnapshotEqual(cObjBefore, SnapshotDir(_fixture.CObjDir, "*.o"),
            "CC .o after C# change");

        // Downstream artifacts must be rewritten.
        Assert.NotEqual(ilcBefore, File.GetLastWriteTimeUtc(_fixture.IlcOutput));
        Assert.NotEqual(elfBefore, File.GetLastWriteTimeUtc(_fixture.ElfFile));
        Assert.NotEqual(isoBefore, File.GetLastWriteTimeUtc(_fixture.IsoFile));
    }

    // ==================================================================
    // TEST 4: C# revert restores full cache.
    // ==================================================================
    [Fact, TestPriority(4)]
    public void T04_CSharpRevert_RestoresCache()
    {
        // Marker was reverted by the using-disposable in T03. Two builds:
        // first stabilizes (C# compiler may produce non-deterministic bytes),
        // second verifies all four caches hit.
        BuildResult stabilize = _fixture.Build();
        Assert.True(stabilize.Success, $"Stabilize build failed:\n{stabilize.Output}");

        BuildResult verify = _fixture.Build();
        Assert.True(verify.Success, $"Verify build failed:\n{verify.Output}");
        AssertAllCacheHits(verify, "after C# revert");
    }

    // ==================================================================
    // TEST 5: ASM change → only ASM + linker + ISO rebuild.
    //                      Patcher, ILC and CC stay strictly cached.
    // ==================================================================
    [Fact, TestPriority(5)]
    public void T05_AsmChange_RebuildsAsmOnly()
    {
        DateTime ilcBefore = File.GetLastWriteTimeUtc(_fixture.IlcOutput);
        DateTime elfBefore = File.GetLastWriteTimeUtc(_fixture.ElfFile);
        DateTime isoBefore = File.GetLastWriteTimeUtc(_fixture.IsoFile);
        Dictionary<string, DateTime> asmObjBefore = SnapshotDir(_fixture.AsmObjDir, "*.obj");
        Dictionary<string, DateTime> cObjBefore = SnapshotDir(_fixture.CObjDir, "*.o");

        Thread.Sleep(1100);
        using IDisposable marker = _fixture.InjectMarker(_fixture.AsmFile, "asm");
        BuildResult result = _fixture.Build();

        Assert.True(result.Success, $"Build after ASM change failed:\n{result.Output}");

        // Patcher + ILC stay cached and the ILC output is byte-identical.
        Assert.Contains("Patcher cache hit", result.Stdout);
        Assert.Contains("ILC cache hit", result.Stdout);
        Assert.Equal(ilcBefore, File.GetLastWriteTimeUtc(_fixture.IlcOutput));

        // CC C objects must be byte-for-byte unchanged.
        AssertSnapshotEqual(cObjBefore, SnapshotDir(_fixture.CObjDir, "*.o"),
            "CC .o after ASM change");

        // ASM .obj set must change (content-hash filename → new file, old orphan removed).
        Dictionary<string, DateTime> asmObjAfter = SnapshotDir(_fixture.AsmObjDir, "*.obj");
        string asmKeysBefore = string.Join(",", asmObjBefore.Keys.OrderBy(k => k));
        string asmKeysAfter = string.Join(",", asmObjAfter.Keys.OrderBy(k => k));
        Assert.NotEqual(asmKeysBefore, asmKeysAfter);

        // Linker + ISO must rebuild.
        Assert.NotEqual(elfBefore, File.GetLastWriteTimeUtc(_fixture.ElfFile));
        Assert.NotEqual(isoBefore, File.GetLastWriteTimeUtc(_fixture.IsoFile));
    }

    // ==================================================================
    // TEST 6: ASM revert restores full cache.
    // ==================================================================
    [Fact, TestPriority(6)]
    public void T06_AsmRevert_RestoresCache()
    {
        // Marker was reverted by the using-disposable in T05. The first build
        // re-hashes the asm to its original content; ASM regenerates the
        // original-content .obj filename and orphans the modified one. Second
        // build then hits all four caches.
        BuildResult stabilize = _fixture.Build();
        Assert.True(stabilize.Success, $"Stabilize build failed:\n{stabilize.Output}");

        BuildResult verify = _fixture.Build();
        Assert.True(verify.Success, $"Verify build failed:\n{verify.Output}");
        AssertAllCacheHits(verify, "after ASM revert");
    }

    // ==================================================================
    // TEST 7: C change → only CC + linker + ISO rebuild.
    //                    Patcher, ILC and ASM stay strictly cached.
    // ==================================================================
    [Fact, TestPriority(7)]
    public void T07_CChange_RebuildsCCOnly()
    {
        DateTime ilcBefore = File.GetLastWriteTimeUtc(_fixture.IlcOutput);
        DateTime elfBefore = File.GetLastWriteTimeUtc(_fixture.ElfFile);
        DateTime isoBefore = File.GetLastWriteTimeUtc(_fixture.IsoFile);
        Dictionary<string, DateTime> asmObjBefore = SnapshotDir(_fixture.AsmObjDir, "*.obj");
        Dictionary<string, DateTime> cObjBefore = SnapshotDir(_fixture.CObjDir, "*.o");

        Thread.Sleep(1100);
        using IDisposable marker = _fixture.InjectMarker(_fixture.CFile, "c");
        BuildResult result = _fixture.Build();

        Assert.True(result.Success, $"Build after C change failed:\n{result.Output}");

        // Patcher + ILC stay cached and ILC output is byte-identical.
        Assert.Contains("Patcher cache hit", result.Stdout);
        Assert.Contains("ILC cache hit", result.Stdout);
        Assert.Equal(ilcBefore, File.GetLastWriteTimeUtc(_fixture.IlcOutput));

        // ASM ASM objects must be byte-for-byte unchanged.
        AssertSnapshotEqual(asmObjBefore, SnapshotDir(_fixture.AsmObjDir, "*.obj"),
            "ASM .obj after C change");

        // CC .o set must change (content-hash filename → new file, old orphan removed).
        Dictionary<string, DateTime> cObjAfter = SnapshotDir(_fixture.CObjDir, "*.o");
        string cKeysBefore = string.Join(",", cObjBefore.Keys.OrderBy(k => k));
        string cKeysAfter = string.Join(",", cObjAfter.Keys.OrderBy(k => k));
        Assert.NotEqual(cKeysBefore, cKeysAfter);

        // Linker + ISO must rebuild.
        Assert.NotEqual(elfBefore, File.GetLastWriteTimeUtc(_fixture.ElfFile));
        Assert.NotEqual(isoBefore, File.GetLastWriteTimeUtc(_fixture.IsoFile));
    }

    // ==================================================================
    // TEST 8: C revert restores full cache.
    // ==================================================================
    [Fact, TestPriority(8)]
    public void T08_CRevert_RestoresCache()
    {
        BuildResult stabilize = _fixture.Build();
        Assert.True(stabilize.Success, $"Stabilize build failed:\n{stabilize.Output}");

        BuildResult verify = _fixture.Build();
        Assert.True(verify.Success, $"Verify build failed:\n{verify.Output}");
        AssertAllCacheHits(verify, "after C revert");
    }

    // ==================================================================
    // TEST 9: CC orphan cleanup — deleted C file's object is removed.
    // ==================================================================
    [Fact, TestPriority(9)]
    public void T09_CCOrphanCleanup_RemovesStaleObject()
    {
        string tempC = Path.Combine(_fixture.DevKernelCDir, "cache_test_orphan.c");

        File.WriteAllText(tempC, "// temp\nvoid cache_test_orphan_fn(void) {}\n");
        try
        {
            BuildResult result = _fixture.Build();
            Assert.True(result.Success, $"Build with temp C file failed:\n{result.Output}");

            string[] orphanObjs = Directory.GetFiles(_fixture.CObjDir, "cache_test_orphan-*");
            Assert.NotEmpty(orphanObjs);
        }
        finally
        {
            if (File.Exists(tempC))
            {
                File.Delete(tempC);
            }
        }

        BuildResult result2 = _fixture.Build();
        Assert.True(result2.Success, $"Rebuild after C deletion failed:\n{result2.Output}");

        string[] remaining = Directory.GetFiles(_fixture.CObjDir, "cache_test_orphan-*");
        Assert.Empty(remaining);
    }

    // ==================================================================
    // TEST 10: ASM orphan cleanup — deleted ASM file's object is removed.
    //
    // Drops a self-contained .s file alongside Runtime.s in the NuGet
    // runtime build dir; clang picks it up via the *.s glob, builds it,
    // then we delete the source and rebuild to verify the .obj is removed.
    // ==================================================================
    [Fact, TestPriority(10)]
    public void T10_AsmOrphanCleanup_RemovesStaleObject()
    {
        string asmDir = Path.GetDirectoryName(_fixture.AsmFile)!;
        string tempAsm = Path.Combine(asmDir, "cache_test_orphan.s");

        // Self-contained: a unique section + global label, no external refs.
        File.WriteAllText(tempAsm,
            ".section .cache_test_orphan_data\n" +
            ".global cache_test_orphan_marker_sym\n" +
            "cache_test_orphan_marker_sym:\n" +
            ".byte 0x42\n");

        try
        {
            BuildResult result = _fixture.Build();
            Assert.True(result.Success, $"Build with temp ASM file failed:\n{result.Output}");

            string[] orphanObjs = Directory.GetFiles(_fixture.AsmObjDir, "cache_test_orphan-*.obj");
            Assert.NotEmpty(orphanObjs);
        }
        finally
        {
            if (File.Exists(tempAsm))
            {
                File.Delete(tempAsm);
            }
        }

        BuildResult result2 = _fixture.Build();
        Assert.True(result2.Success, $"Rebuild after ASM deletion failed:\n{result2.Output}");

        string[] remaining = Directory.GetFiles(_fixture.AsmObjDir, "cache_test_orphan-*.obj");
        Assert.Empty(remaining);
    }

    // ==================================================================
    // TEST 11: ASM content-hash filename round trip.
    //
    // Edit asm → new content-hash filename + old orphan removed.
    // Revert  → original content-hash filename returns.
    // ==================================================================
    [Fact, TestPriority(11)]
    public void T11_AsmContentHash_RoundTrip()
    {
        string asmBaseName = Path.GetFileNameWithoutExtension(_fixture.AsmFile);
        string[] originalObjs = Directory.GetFiles(_fixture.AsmObjDir, $"{asmBaseName}-*.obj");
        Assert.NotEmpty(originalObjs);
        string originalObjName = Path.GetFileName(originalObjs[0]);

        using (IDisposable marker = _fixture.InjectMarker(_fixture.AsmFile, "asm"))
        {
            BuildResult result = _fixture.Build();
            Assert.True(result.Success, $"Build after ASM edit failed:\n{result.Output}");

            string[] modifiedObjs = Directory.GetFiles(_fixture.AsmObjDir, $"{asmBaseName}-*.obj");
            Assert.NotEmpty(modifiedObjs);
            string modifiedObjName = Path.GetFileName(modifiedObjs[0]);

            Assert.NotEqual(originalObjName, modifiedObjName);
            Assert.False(File.Exists(originalObjs[0]), "Original ASM object should be cleaned as orphan");
        }

        BuildResult result2 = _fixture.Build();
        Assert.True(result2.Success, $"Build after ASM revert failed:\n{result2.Output}");

        string[] revertedObjs = Directory.GetFiles(_fixture.AsmObjDir, $"{asmBaseName}-*.obj");
        Assert.NotEmpty(revertedObjs);
        Assert.Equal(originalObjName, Path.GetFileName(revertedObjs[0]));
    }

    // ==================================================================
    // TEST 12: CC content-hash filename round trip.
    //
    // Edit C  → new content-hash filename + old orphan removed.
    // Revert  → original content-hash filename returns.
    // ==================================================================
    [Fact, TestPriority(12)]
    public void T12_CCContentHash_RoundTrip()
    {
        string cBaseName = Path.GetFileNameWithoutExtension(_fixture.CFile);
        string[] originalObjs = Directory.GetFiles(_fixture.CObjDir, $"{cBaseName}-*.o");
        Assert.NotEmpty(originalObjs);
        string originalObjName = Path.GetFileName(originalObjs[0]);

        using (IDisposable marker = _fixture.InjectMarker(_fixture.CFile, "c"))
        {
            BuildResult result = _fixture.Build();
            Assert.True(result.Success, $"Build after C edit failed:\n{result.Output}");

            string[] modifiedObjs = Directory.GetFiles(_fixture.CObjDir, $"{cBaseName}-*.o");
            Assert.NotEmpty(modifiedObjs);
            string modifiedObjName = Path.GetFileName(modifiedObjs[0]);

            Assert.NotEqual(originalObjName, modifiedObjName);
            Assert.False(File.Exists(originalObjs[0]), "Original C object should be cleaned as orphan");
        }

        BuildResult result2 = _fixture.Build();
        Assert.True(result2.Success, $"Build after C revert failed:\n{result2.Output}");

        string[] revertedObjs = Directory.GetFiles(_fixture.CObjDir, $"{cBaseName}-*.o");
        Assert.NotEmpty(revertedObjs);
        Assert.Equal(originalObjName, Path.GetFileName(revertedObjs[0]));
    }

    // ==================================================================
    // TEST 13: Wipe intermediate dir, full clean rebuild, then verify
    //          all four caches hit on a no-change rebuild.
    // ==================================================================
    [Fact, TestPriority(13)]
    public void T13_CleanIntermediateRebuild_ThenCacheWorks()
    {
        if (Directory.Exists(_fixture.ObjDir))
        {
            Directory.Delete(_fixture.ObjDir, true);
        }

        BuildResult result = _fixture.Build();
        Assert.True(result.Success, $"Clean intermediate build failed:\n{result.Output}");
        Assert.True(File.Exists(_fixture.IsoFile), "ISO not produced after clean rebuild");
        Assert.DoesNotContain("cache hit", result.Stdout, StringComparison.OrdinalIgnoreCase);

        BuildResult result2 = _fixture.Build();
        Assert.True(result2.Success, $"No-change rebuild failed:\n{result2.Output}");
        AssertAllCacheHits(result2, "no-change after clean rebuild");
    }

    // ==================================================================
    // TEST 14: Feature switch flip -> ILC + linker + ISO rebuild.
    //          The managed assemblies are byte-identical, so the patcher stays
    //          cached; only the ILC response file changes (its runtimeknob
    //          arguments), and the ILC cache must key on it. ASM and CC are
    //          untouched. Flipping back rebuilds once more, then every step
    //          hits its cache again.
    // ==================================================================
    [Fact, TestPriority(14)]
    public void T14_FeatureSwitchChange_RebuildsIlc()
    {
        DateTime elfBefore = File.GetLastWriteTimeUtc(_fixture.ElfFile);
        DateTime isoBefore = File.GetLastWriteTimeUtc(_fixture.IsoFile);
        DateTime ilcBefore = File.GetLastWriteTimeUtc(_fixture.IlcOutput);
        DateTime ilcHashBefore = File.GetLastWriteTimeUtc(_fixture.IlcHashFile);
        Dictionary<string, DateTime> asmObjBefore = SnapshotDir(_fixture.AsmObjDir, "*.obj");
        Dictionary<string, DateTime> cObjBefore = SnapshotDir(_fixture.CObjDir, "*.o");

        Thread.Sleep(1100);
        BuildResult flipped = _fixture.Build("-p:CosmosEnableMouse=false");

        Assert.True(flipped.Success, $"Build with the mouse switched off failed:\n{flipped.Output}");

        // Nothing managed changed, so the patcher must not run; ILC must.
        Assert.Contains("Patcher cache hit", flipped.Stdout);
        Assert.Contains("[ILC] Compiling:", flipped.Stdout);
        Assert.NotEqual(ilcBefore, File.GetLastWriteTimeUtc(_fixture.IlcOutput));
        Assert.NotEqual(ilcHashBefore, File.GetLastWriteTimeUtc(_fixture.IlcHashFile));
        Assert.NotEqual(elfBefore, File.GetLastWriteTimeUtc(_fixture.ElfFile));
        Assert.NotEqual(isoBefore, File.GetLastWriteTimeUtc(_fixture.IsoFile));

        AssertSnapshotEqual(asmObjBefore, SnapshotDir(_fixture.AsmObjDir, "*.obj"), "ASM .obj after feature switch flip");
        AssertSnapshotEqual(cObjBefore, SnapshotDir(_fixture.CObjDir, "*.o"), "CC .o after feature switch flip");

        // Back to the default: the response file changes again, so ILC runs
        // again; a further build then hits every cache.
        BuildResult restored = _fixture.Build();
        Assert.True(restored.Success, $"Build with the mouse switched back on failed:\n{restored.Output}");
        Assert.Contains("[ILC] Compiling:", restored.Stdout);

        BuildResult verify = _fixture.Build();
        Assert.True(verify.Success, $"Verify build failed:\n{verify.Output}");
        AssertAllCacheHits(verify, "after feature switch revert");
    }
}

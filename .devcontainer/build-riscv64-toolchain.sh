#!/usr/bin/env bash
#
# Builds what NativeAOT needs to target riscv64 and Microsoft does not publish
# on nuget.org, from the dotnet/runtime submodule at the commit this repository
# pins:
#
#   jit/linux-x64/libclrjit_unix_riscv64_x64.so
#       The cross JIT ILC loads to emit riscv64 code (--jitpath). The host
#       ILCompiler package ships the x64 and arm64 JITs only.
#   aotsdk/System.Private.*.dll
#       CoreLib and its siblings, compiled for riscv64: the part of the NativeAOT
#       runtime pack that differs per architecture. The rest of the pack, the
#       framework assemblies, is the same IL on every Linux architecture, and a
#       riscv64 build takes it from the linux-x64 pack.
#
# Output goes to artifacts/riscv64-toolchain/ (or $1), which
# src/Cosmos.Build.Ilc.RISCV64 packs. A second run with the submodule at the
# same commit only copies the outputs again.

set -euo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
RUNTIME="$REPO_ROOT/dotnet/runtime"
OUT="${1:-$REPO_ROOT/artifacts/riscv64-toolchain}"
CONFIG=Release

if [[ ! -x "$RUNTIME/build.sh" ]]; then
  echo "ERROR: dotnet/runtime submodule missing: git submodule update --init dotnet/runtime" >&2
  exit 1
fi

HOST_ARCH=$(uname -m)
if [[ "$(uname -s)" != "Linux" || "$HOST_ARCH" != "x86_64" ]]; then
  echo "ERROR: the riscv64 toolchain builds on a Linux x64 host only (this is $(uname -s) $HOST_ARCH)" >&2
  exit 1
fi

JIT_SO="$RUNTIME/artifacts/bin/coreclr/linux.x64.$CONFIG/libclrjit_unix_riscv64_x64.so"
AOTSDK="$RUNTIME/artifacts/bin/coreclr/linux.riscv64.$CONFIG/aotsdk"
STAMP="$RUNTIME/artifacts/.cosmos-riscv64-toolchain"
COMMIT=$(git -C "$RUNTIME" rev-parse HEAD)

if [[ -f "$STAMP" && "$(cat "$STAMP")" == "$COMMIT" && -f "$JIT_SO" && -f "$AOTSDK/System.Private.CoreLib.dll" ]]; then
  echo "riscv64 toolchain up to date for dotnet/runtime $COMMIT"
else
  echo "Building the riscv64 toolchain from dotnet/runtime $COMMIT..."

  # The submodule lives inside this repository, so MSBuild walks up to our
  # Directory.Packages.props and turns central package management on for the
  # runtime's projects, which are not written for it. NuGet audit is off: the
  # release branch pins task dependencies with advisories published after it,
  # and the runtime build treats warnings as errors.
  common=(-c "$CONFIG" /p:ManagePackageVersionsCentrally=false /p:ImportDirectoryPackagesProps=false /p:NuGetAudit=false)

  # 1. The cross JITs, among them the riscv64 one (a community target).
  #    Configuring coreclr checks for libraries no JIT links against:
  #    libgssapi_krb5 (located with find_library, then loaded with dlopen at
  #    run time on Linux, so any value satisfies the check) and lttng (event
  #    tracing, off).
  # Run from the submodule: the runtime's scripts call dotnet, which reads the
  # global.json above the working directory, and ours pins another SDK.
  cd "$RUNTIME"
  ./build.sh -subset clr.alljitscommunity "${common[@]}" \
    --cmakeargs "-DLIBGSS=gssapi_krb5" \
    --cmakeargs "-DFEATURE_EVENTSOURCE_XPLAT=0"

  # 2. AsmOffsets.cs, the structure offsets CoreLib compiles in. The native
  #    NativeAOT runtime build writes it by preprocessing AsmOffsets.cspp with
  #    the target's definitions; that build needs a riscv64 sysroot, the
  #    preprocessing alone does not.
  offsets_dir="$RUNTIME/artifacts/obj/coreclr/linux.riscv64.$CONFIG/nativeaot/Runtime/Full"
  mkdir -p "$offsets_dir"
  clang++ -x c++ -E -P \
    -DHOST_64BIT -DTARGET_64BIT -DHOST_RISCV64 -DTARGET_RISCV64 \
    -DHOST_UNIX -DTARGET_UNIX -DTARGET_LINUX \
    -DFEATURE_CACHED_INTERFACE_DISPATCH \
    -I"$RUNTIME/src/coreclr/nativeaot/Runtime/riscv64" \
    "$RUNTIME/src/coreclr/nativeaot/Runtime.Base/src/AsmOffsets.cspp" \
    > "$offsets_dir/AsmOffsets.cs"

  # 3. System.Private.CoreLib, TypeLoader, Reflection.Execution and
  #    StackTraceMetadata for riscv64 (managed only).
  ./build.sh -subset clr.nativeaotlibs -a riscv64 "${common[@]}"
  cd "$REPO_ROOT"

  echo "$COMMIT" > "$STAMP"
fi

rm -rf "$OUT"
mkdir -p "$OUT/jit/linux-x64" "$OUT/aotsdk"
cp "$JIT_SO" "$OUT/jit/linux-x64/"
cp "$AOTSDK"/System.Private.*.dll "$OUT/aotsdk/"
echo "$COMMIT" > "$OUT/runtime-commit.txt"

echo "riscv64 toolchain staged in $OUT:"
ls -la "$OUT/jit/linux-x64" "$OUT/aotsdk"

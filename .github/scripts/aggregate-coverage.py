#!/usr/bin/env python3
"""
Aggregate coverage data from multiple test suite JSON files.
Unions method-level hits across suites to produce per-project and overall coverage.
"""
import json
import glob
import os
import sys

def main():
    # Collect all candidate JSON files from downloaded coverage artifacts.
    # Do not rely on specific filenames; keep only schema-valid coverage reports.
    candidate_files = sorted(glob.glob("./coverage/**/*.json", recursive=True))
    json_files = []
    for path in candidate_files:
        try:
            with open(path) as f:
                data = json.load(f)
            if isinstance(data, dict) and "assemblies" in data and "architecture" in data:
                json_files.append(path)
        except Exception:
            continue

    if not json_files:
        summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
        if summary_path:
            with open(summary_path, "a") as f:
                f.write("No coverage data found. Coverage instrumentation may not be enabled.\n")
        else:
            print("No coverage data found.")
        sys.exit(0)

    # Aggregate method-level data across all suites
    # Key: (assembly, "Type::Method") -> hit by ANY suite = covered
    # Group by architecture to produce one report per arch.
    arch_data = {}  # arch -> { (asm, method_key) -> hit:bool }
    suite_summaries = []  # for per-suite table

    for jf in json_files:
        with open(jf) as f:
            data = json.load(f)

        suite = data.get("suite", "?")
        arch = data.get("architecture", "?")
        suite_summaries.append({
            "suite": suite,
            "arch": arch,
            "total": data.get("totalMethods", 0),
            "hit": data.get("hitMethods", 0),
            "pct": data.get("percentage", 0),
        })

        if arch not in arch_data:
            arch_data[arch] = {}

        for asm in data.get("assemblies", []):
            asm_name = asm["name"]
            for m in asm.get("methods", []):
                key = (asm_name, m["name"])
                # Union: if ANY suite hit this method, it's covered
                if key not in arch_data[arch]:
                    arch_data[arch][key] = m["hit"]
                elif m["hit"]:
                    arch_data[arch][key] = True

    # Assemblies that only apply to a specific architecture.
    # When reporting for one architecture, exclude the other ones' HALs.
    ARCH_SPECIFIC = {
        "x64":     {"Cosmos.Kernel.HAL.ARM64", "Cosmos.Kernel.HAL.RISCV64"},
        "arm64":   {"Cosmos.Kernel.HAL.X64", "Cosmos.Kernel.HAL.RISCV64"},
        "riscv64": {"Cosmos.Kernel.HAL.X64", "Cosmos.Kernel.HAL.ARM64"},
    }

    # Build per-project stats for each architecture
    report_lines = []
    report_lines.append("# 📊 Code Coverage Report\n")

    for arch in sorted(arch_data.keys()):
        methods = arch_data[arch]
        exclude_asms = ARCH_SPECIFIC.get(arch, set())

        # Group by assembly — track both stats and missed method names
        asm_stats = {}   # asm_name -> {"total": int, "hit": int}
        asm_missed = {}  # asm_name -> [method_key, ...]
        for (asm_name, method_key), hit in methods.items():
            if asm_name in exclude_asms:
                continue
            if asm_name not in asm_stats:
                asm_stats[asm_name] = {"total": 0, "hit": 0}
                asm_missed[asm_name] = []
            asm_stats[asm_name]["total"] += 1
            if hit:
                asm_stats[asm_name]["hit"] += 1
            else:
                asm_missed[asm_name].append(method_key)

        total_all = sum(s["total"] for s in asm_stats.values())
        hit_all = sum(s["hit"] for s in asm_stats.values())
        pct_all = (hit_all / total_all * 100) if total_all > 0 else 0

        report_lines.append(f"## {arch} — Overall: {hit_all}/{total_all} methods ({pct_all:.1f}%)\n")
        report_lines.append("| Project | Methods Hit | Total | Coverage |")
        report_lines.append("|---------|------------|-------|----------|")

        for asm_name in sorted(asm_stats.keys()):
            s = asm_stats[asm_name]
            p = (s["hit"] / s["total"] * 100) if s["total"] > 0 else 0
            bar = "🟩" if p == 100 else ("🟨" if p >= 50 else "🟥")
            report_lines.append(f"| {bar} {asm_name} | {s['hit']} | {s['total']} | {p:.1f}% |")

        report_lines.append(f"| **Total** | **{hit_all}** | **{total_all}** | **{pct_all:.1f}%** |")
        report_lines.append("")

        # Per-project uncovered methods (details/spoiler)
        for asm_name in sorted(asm_stats.keys()):
            missed = sorted(asm_missed.get(asm_name, []))
            if not missed:
                continue
            s = asm_stats[asm_name]
            n_missed = len(missed)
            report_lines.append(f"<details><summary>❌ {asm_name} — {n_missed} uncovered method{'s' if n_missed != 1 else ''}</summary>\n")
            report_lines.append("```")
            for m in missed:
                report_lines.append(m)
            report_lines.append("```")
            report_lines.append("\n</details>\n")

    # Per-suite breakdown
    report_lines.append("<details><summary>Per-suite breakdown</summary>\n")
    report_lines.append("| Suite | Arch | Methods Hit | Total | Coverage |")
    report_lines.append("|-------|------|------------|-------|----------|")
    for s in suite_summaries:
        report_lines.append(f"| {s['suite']} | {s['arch']} | {s['hit']} | {s['total']} | {s['pct']:.1f}% |")
    report_lines.append("\n</details>\n")

    repo = os.environ.get("GITHUB_REPOSITORY", "")
    run_id = os.environ.get("GITHUB_RUN_ID", "")
    if repo and run_id:
        report_lines.append(f"📋 [View full report](https://github.com/{repo}/actions/runs/{run_id})")

    report_text = "\n".join(report_lines)

    # Write to step summary
    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary_path:
        with open(summary_path, "a") as f:
            f.write(report_text)

    # Write PR comment file
    with open("/tmp/coverage_pr_comment.md", "w") as f:
        f.write(report_text)

    # Signal that we found data
    output_path = os.environ.get("GITHUB_OUTPUT")
    if output_path:
        with open(output_path, "a") as f:
            f.write("found_data=true\n")

    print(f"Processed {len(json_files)} coverage files")


if __name__ == "__main__":
    main()

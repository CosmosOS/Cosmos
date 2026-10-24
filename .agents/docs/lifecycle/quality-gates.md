---
title: Quality gates
layout: default
parent: Lifecycle
nav_order: 3
---

# Quality gates

The layered defense that protects the codebase. The governing principle:

> **The test suite is the primary line of defense. The other gates are the secondary line.**

Every fix adds a regression test, so the suite is a *living, growing* body of regressions — over
time it catches automatically what today still needs a judgment gate. The other gates exist to
catch what the suite doesn't *yet* cover, and each thing they catch should become a new test.

The gates are **layered and independent.** No single gate is trusted alone; they catch different
classes of defect, and on a reproduced finding the stricter verdict wins.

---

## The pre-fix discipline (before any code changes)

Quality starts before the fix, not after:

1. **Reproduce the bug first.** Prove it's actually broken on the current trunk by running the real
   repro — don't theorize from the issue text. A bug you can't reproduce is a bug you can't verify
   you fixed.
2. **Write the failing test first** when practical (red), then make it pass (green). This guarantees
   the test actually exercises the bug.
3. **Fix the class, not the instance.** Once you understand the root cause, grep for sibling call
   paths with the same flaw and fix them together.

## The gates (in order)

### 1. The test suite — the primary line
- Run it **to completion, never sampled.** A partial run that looks green hides the regression in
  the part you skipped.
- Every fix adds a regression test. The suite only grows; coverage is a ratchet.
- A green suite is necessary but **not sufficient** — it only covers what's been written. That's why
  the other gates exist.
- **Coverage-delta check** (worth building as the system matures): confirm the new regression test
  actually exercises the changed lines, so it can't pass vacuously.

### 2. Automated code review
An independent reviewer pass over the diff (a code-review tool or a dedicated reviewer agent). It
reads the change in isolation and flags concerns. Treat its output as *input*, not verdict.

### 3. Adversarial review
A **second, differently-tuned** review pass. The whole point is that it catches *different* things
than the first — a second independent opinion surfaces findings the first missed. On a finding that
both can check, the **stricter verdict wins** once it's reproduced.

> Why two reviews and not one? Empirically, the second pass catches real defects the first misses on
> a meaningful fraction of non-trivial changes. The cost is small; the saved regressions are not.

### 4. Visual verification
For any change with a visible surface, a passing test suite says nothing about whether it *looks*
right. Capture before/after screenshots at the real viewports (desktop and mobile), and **inspect
them** — never ship a visible change on tests alone. For interactive surfaces, *drive the
interaction*; a screenshot of a gesture surface is not proof the gesture works.

### 5. The authoritative re-run (immediately before merge)
Run gates 1–4 **fresh, right before merge**, independent of any earlier reviewer's verdict or cached
dossier. A prior "looks clean" may have verified only one code path or a stale base. This final
independent run is what actually authorizes the merge.

---

## Flakes: never tolerate one

A flaky test is a **defect**, not noise to be re-run away. Often it's a real product race condition
or ordering bug wearing a costume.

- When a flake appears, **root-cause and fix it** so it stops being flaky — never rerun-and-forget.
- A classifier that says "transient → just rerun" is dangerous: it normalizes papering over real
  bugs. If you build flake detection at all, make its output *"flake detected — here's the
  signature, go fix it,"* not *"ignored, moving on."* A flake **ledger** that drives root-fixes is a
  better shape than a rerun-classifier.

## Untrusted-code execution

Gates 1, 4, and 5 all *run* the PR's code: the suite, the driven interaction, the re-run before
merge. When the PR is untrusted, treat that code as adversarial. **Never run it unsandboxed.** Run
inside a locked-down sandbox with no network and no credential access that fails closed if it can't
be built. If contributor-code execution is switched off or the sandbox can't be built, the run does
not happen and the gate is not all-clear; a skipped suite is never a passed one. Inert reading of
the diff needs no sandbox; *execution* is gated. The
[security spine](../reference/security-spine.md#4-the-sandbox-untrusted-code-execution) says which
PRs count as untrusted and what to do when the run can't happen.

---

## What "done" means

A change is done when:
- it's merged **and** tagged **and** (if you deploy) the running version serves it;
- a regression test covers the fix and is in the suite;
- the visible surface (if any) was inspected;
- the author is credited and the issue is closed.

A passed gate alone is not "done." A description of a working artifact is not a working artifact.

---

_Related: [PR lifecycle](pr-lifecycle.md) · [the watchdog pattern](../playbooks/watchdog-pattern.md)
· [security spine](../reference/security-spine.md)._

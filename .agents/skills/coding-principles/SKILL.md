---
name: coding-principles
description: The craft-level discipline for writing and editing code well — load before any non-trivial code change. Surgical edits, define-done-up-front, read-before-write, fail-loud, tests-assert-intent. The Builder's companion to pr-deep-review.
---

# coding-principles

**When to load:** before writing or editing code on any non-trivial change — a bug fix, a feature, a
rebase-with-fixes. The Builder's keystroke-level discipline; pairs with
[`pr-deep-review`](../pr-deep-review/SKILL.md) (which reviews the result). Band B.

Full reasoning: [coding principles reference](../../docs/reference/coding-principles.md).

## Before you write
1. **Resolve ambiguity.** If the requirement is unclear or the approach has real trade-offs, ask
   first. A wrong change that passes tests is worse than no change.
2. **Name "done."** Write down the observable fact that must be true to finish — a scenario, a
   failing test that should pass, a state that should appear. This is your acceptance check.
3. **Read before you write.** Read the file's exports, the callers of what you're touching, and the
   utilities it depends on. For any "swap A for B" / "change the source of truth," find every
   consumer of A *before* changing the producer.
4. **Reproduce first** (for a bug). Prove it's broken on the current trunk; write the failing test
   (red) before the fix.

## While you write
5. **Climb the minimalism ladder.** Take the first rung that holds, *after* you understand the flow:
   need it at all? → already in this codebase (reuse, don't rebuild)? → stdlib? → platform/runtime
   capability (native control, CSS, DB constraint) before a dependency or hand-rolled code? → an
   already-installed dependency? → one line? → only then, minimum code that works. Never add a new
   dependency for what a few lines do. Mark a deliberate shortcut with a `ceiling:` comment naming
   the limit *and* the upgrade path. Never minimize away validation, data-loss handling, security, or
   the regression test. See [reference](../../docs/reference/coding-principles.md#the-minimalism-ladder).
6. **Surgical edits only.** Change exactly what the task needs. No drive-by cleanups, renames, or
   "while we're here" additions. No speculative features.
7. **One state, one owner.** Don't add a second writer for state something else already controls.
   The other side reads, never writes. When you hide/disable a control, guard what depends on it.
8. **Don't blend conflicting patterns.** If an old and new pattern contradict, pick one, commit, and
   flag the other for separate cleanup — never make both coexist (the runtime-coexistence bug).
9. **Match the codebase's conventions** over personal taste. Raise disagreements as a separate
   refactor proposal, not a competing style inside this change.

## Before you call it done
10. **Verify the real scenario.** Syntax-check, run the tests to completion, and confirm the exact
    "done" scenario from step 2 actually works in the *running* code. Iterate until it does — don't
    stop at the first green checkmark. "Looks right" / "tests pass" / "diff looks correct" are not
    done.
11. **Tests assert intent.** Each new test must be order-independent and must *fail* when the
    implementation is reverted (red-before-green). Assert the contract, not an incidental surface
    string.
12. **Fail loud.** Report the actual state: separate what you **verified** from what you **assumed**.
    Name any skipped step, excluded test, or unverified assumption. Never paper over a partial
    failure.

## Pitfalls
- **"It works on my machine" is a bug signature, not a pass** — environment divergence is real; the
  authoritative signal is the target environment (CI), not local. See
  [bug-shape catalog](../../docs/reference/bug-shapes.md#5-environment-divergence-works-locally-fails-in-ci).
- **Blaming the view for lost state** — reload from the source of truth first; if it doesn't recover,
  the bug is in the data/persist layer, not the renderer.
- **The vacuous test** — a test that can't fail protects nothing. Confirm red-before-green.
- **Running someone else's branch** — fixing on a contributor's branch means "run the tests" runs
  their code. The [sandbox rule](../../docs/reference/security-spine.md#4-the-sandbox-untrusted-code-execution)
  decides whether it may run; your own commits on top don't make the branch trusted.

## Verification
- The named "done" scenario was exercised in running code, not just asserted.
- The diff contains only what the task required (no scope creep).
- Every new test is isolated and fails on a reverted implementation.
- The summary distinguishes verified from assumed; nothing was silently skipped.

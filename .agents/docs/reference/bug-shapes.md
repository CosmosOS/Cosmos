---
title: Bug-shape catalog
layout: default
parent: Reference
nav_order: 6
---

# Bug-shape catalog

Recurring bug *classes* — the shapes that show up again and again across projects, pass unit tests,
survive green CI, and surface only under the right conditions. A reviewer who knows the shapes spots
them in a diff; a builder who knows them avoids writing them.

Each entry: what it looks like, why the usual checks miss it, and how to catch it. The shapes are
language- and framework-neutral — the examples are illustrative, not specific to any stack.

---

## 1. Runtime coexistence

**Shape:** Two implementations of the same thing both remain live — an old helper and its
replacement, two data shapes for the same record, two handlers for the same event. Each works in
isolation; together they conflict, double-fire, or one shadows the other.

**Why checks miss it:** Unit tests exercise each path *alone*, where each is correct. The bug needs
*both* to run, which only happens in the integrated runtime — exactly what unit tests don't cover.

**Catch it:**
- When a change replaces a pattern, grep for the *old* pattern across the whole codebase before
  merging. Leftover callers are the bug.
- A rename or migration must be all-or-nothing. A half-applied rename (new name produced, old name
  still expected downstream) is this shape.
- Prefer deleting the old path over leaving it "just in case." Dead code that still executes isn't
  dead.

---

## 2. Producer changed, consumer didn't

**Shape:** A change updates where a value comes from (a new source of truth, a new format, a renamed
field) but a downstream consumer still expects the old form. The producer side is fully correct in
isolation; the read side silently gets the wrong thing.

**Why checks miss it:** The diff looks complete — the producer was updated and *its* tests pass.
Nobody tested the consumer against the new shape because the consumer wasn't in the diff.

**Catch it:** The moment a change is "swap A for B" / "change the source of truth," trace every
reader of the old value. Probe both ends: is the old shape still produced anywhere? Does every
consumer handle the new shape? (This is [coding principle 8](coding-principles.md#8-read-the-consumer-not-just-the-producer).)

---

## 3. Persistence loss wearing a presentation costume

**Shape:** Data "disappears" or "resets," and it looks like a view/render bug — but the data was
actually lost at the source (a save/clear race, a write dropped on an edge path). You debug the
renderer for hours; the renderer was innocent.

**Why checks miss it:** View-only debugging never inspects the persisted state, so the loss is
invisible. Everything in the view layer looks correct because it *is* correct — it's faithfully
rendering data that's already gone.

**Catch it:** Before touching the view, reload from the source of truth.
- Recovers on reload → presentation/refresh-layer bug.
- Still gone after reload → data-layer loss; fix the write/persist path.

(This is [coding principle 6](coding-principles.md#6-find-which-layer-owns-the-lost-state-before-fixing-it).)

---

## 4. Split ownership of one piece of state

**Shape:** Two layers both control the same thing — two places deciding whether a control is
visible/enabled, two caches of the same value, two flags for the same condition. They drift out of
sync, producing intermittent wrongness.

A common sub-form: **hiding or disabling a control does not disable the state that depends on it.**
A control gets hidden, but the logic that reacts to it (a position calculation, a dependent handler,
a derived value) still runs against the now-hidden thing and computes garbage.

**Why checks miss it:** In the test environment both owners usually agree, so nothing drifts. The
divergence needs a specific sequence (resize, re-entry, a state change between the two writers) that
tests rarely reproduce.

**Catch it:** Give every piece of state exactly one owner; the other side reads, never writes (see
[one state, one owner](coding-principles.md#one-state-one-owner)). When you hide or disable
something, audit what *depends* on it and guard those paths too.

---

## 5. Environment divergence (works locally, fails in CI)

**Shape:** A test passes in one environment and fails in another on the same code. Common causes: a
dependency present locally but absent in CI (or the reverse), a test that imports something only one
environment has, reliance on wall-clock timing or available cores, or a global mutated by an
earlier test that only runs in one shard. The reverse — green in CI, red locally — is the same class.

**Why checks miss it:** "It passes on my machine" is the whole trap — the environment difference *is*
the bug, and you can't see it from inside the environment that passes.

**Catch it:**
- The authoritative signal is the suite passing in the *target* environment (usually CI), not
  locally.
- A test that fails on every version/shard of one environment but passes locally points at an
  environment-specific import or dependency — mock at the boundary rather than importing the thing
  that isn't there.
- A test green in isolation but red in the full run points at cross-test state leakage — isolate it
  (fresh fixtures, no shared globals), don't just re-run it.

---

## 6. The vacuous test

**Shape:** A test that passes whether or not the code is correct — it asserts something always true,
checks for the mere presence of a symbol, or pins a surface string unrelated to the actual behavior.
It adds a green checkmark and zero protection.

**Why checks miss it:** It's green, and green tests don't get scrutinized. It only reveals itself
when a real regression sails through it.

**Catch it:** For every new test, confirm it *fails* when the implementation is reverted or broken
(red-before-green). If it can't fail, it isn't testing anything. Assert the contract — the behavior
that matters — not the incidental surface. (See
[coding principle 5](coding-principles.md#5-test-isolation-and-tests-that-assert-intent).)

---

## 7. The flake that's actually a bug

**Shape:** A test fails intermittently. The tempting read is "flaky, just re-run." Often it's a real
race condition, ordering dependency, or resource bug in the *product* — the test is the only thing
loud enough to surface it.

**Why checks miss it:** A re-run policy is designed to make it invisible. "Transient → rerun"
normalizes papering over a genuine defect.

**Catch it:** Treat every flake as a defect. Root-cause the signature; fix the underlying race or
ordering bug. If you track flakes, the output should be "flake detected — here's the signature, go
fix it," never "ignored, moving on." (See
[quality gates](../lifecycle/quality-gates.md#flakes-never-tolerate-one).)

---

## 8. The instance fix that left the class alone

**Shape:** A bug is reported at one call site, you patch that site, and three sibling call sites with
the identical flaw ship untouched. The report is closed; the bug is still in the product.

**Why checks miss it:** The reported case now works and its test passes. Nothing points at the
siblings because nobody looked for them.

**Catch it:** When you find a defect, grep for its *shape* across the codebase and fix the whole
class in one change. (This is a [guiding principle](../architecture/index.md#the-guiding-principles)
and a recurring [anti-pattern](anti-patterns.md).)

---

## 9. The shared helper that deadlocks under a held lock

**Shape:** You widen a fix across sibling call sites by factoring it into one shared helper — the
right dedupe move. But the call sites differ in whether they *already hold a lock*. The helper
unconditionally re-acquires a non-reentrant lock, so the caller that already holds it hangs forever.

**Why checks miss it:** A mocked lock in a unit test doesn't block, so every test stays green. The
deadlock needs a *real* lock object and the specific caller that enters the helper already holding
it — an integration condition unit tests don't set up.

**Catch it:** When you extract shared code that touches a lock, audit each call site for whether it
already holds that lock, and make the helper's locking explicit (reentrant, or take-lock vs.
assume-lock variants). Prove it with a real-object end-to-end run, not a mock.

_Contributed by Teknium — see [reviewing at volume](reviewing-at-volume.md#two-traps-when-you-widen)._

---

## 10. Cleanup at the wrong lifecycle phase

**Shape:** A sibling site *looks* like the same bug as one you just fixed — unbounded growth of a map,
say — so you add the same cleanup there. But that site is a different lifecycle phase where the state
is still needed. Pruning per-item state at cache-eviction time is wrong if an evicted item's
*session* is still alive and rebuilds itself from that state.

**Why checks miss it:** The cleanup looks correct and its immediate test passes — the state does get
freed. The breakage only appears when something *downstream* tries to read the state after it's been
pruned, on a path the cleanup test never exercises.

**Catch it:** Before widening a cleanup, grep for where the state is *read* on resume/rebuild.
"Evicted from cache" and "the work ended" are different lifecycle phases; put the cleanup at the phase
where the state is provably no longer needed, not at the first phase that resembles the original bug.

_Contributed by Teknium — see [reviewing at volume](reviewing-at-volume.md#two-traps-when-you-widen)._

---

## 11. The stale branch that reverts recent work on merge

**Shape:** You merge a branch cut from an older trunk, and it silently reverts recent work — but
*only* when a stale copy of an unrelated file is part of the branch's own changes. A normal merge
(including a squash) is a three-way merge against the merge-base, so a file the branch never touched
keeps the trunk's newer version. The revert happens when the old copy rides *inside the diff*: an
agent that committed its whole working tree, a regenerated or vendored artifact, a wholesale file
rewrite, a conflict resolution that took the branch's side — or a salvage done with non-merge
mechanics (`git diff main..branch | git apply`, `git checkout branch -- .`) that replays stale
content wholesale.

**Why checks miss it:** The change under review looks correct and its tests pass. The reverted file
rides in on the diff as an incidental hunk nobody scrutinizes, or on the merge mechanics of a
non-standard salvage.

**Catch it:** Scan the branch's changed-file *list* for anything outside the change's stated intent —
a file the fix had no reason to touch is the red flag (being merely *behind* on untouched files is
normal and safe). Read the effective diff, not the commit list: a hunk that deletes recent trunk work
is the tell. Re-fetch the trunk immediately before merge (a green change can be superseded during its
own CI run by a parallel fix) and preview the merge result; `git log HEAD..origin/main -- <changed
files>` shows what landed since you branched so you can inspect any overlap.

_Contributed by Teknium — see [reviewing at volume](reviewing-at-volume.md#the-stale-branch-revert)._

---

## 12. The invariant the tests don't encode

**Shape:** A locally-correct diff violates an architectural invariant that no test checks — a cache
prefix that must stay stable across turns, a message sequence that must alternate roles, an id that
must be derived deterministically so a content-keyed cache still hits, a cache key that must include
every identity axis. The change behaves *identically* in every test and breaks (or silently taxes
everyone) in production.

**Why checks miss it:** The invariant isn't written down as an assertion anywhere, so "passes tests"
says nothing about it. The cost is often invisible (a cache-hit-rate collapse, a per-request price
multiplier) rather than a crash.

**Catch it:** Keep a short **invariants list** in the repo's agent-entry file, and require any review
of a touched subsystem to check the change against it. When you discover a new invariant the hard way,
add it to the list *and* — where possible — add a test that encodes it so it graduates from tribal
knowledge to an automatic check.

_Contributed by Teknium — see [reviewing at volume](reviewing-at-volume.md#invariants-a-reviewer-cant-see-in-the-diff)._

---

_How to use this: skim it before a deep review so the shapes are fresh, and add to it when your
project discovers a new recurring class. A catalog of the bugs that actually bite you is one of the
highest-leverage documents a project keeps._

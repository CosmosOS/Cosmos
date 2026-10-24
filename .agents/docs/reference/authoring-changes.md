---
title: Authoring a change that lands
layout: default
parent: Reference
nav_order: 5
---

# Authoring a change that lands cleanly

Most first-round review friction is preventable at authoring time. This page is written **to the
contributor** — human or AI agent — who is about to open a change. It is the author-side companion
to the maintainer-side [PR lifecycle](../lifecycle/pr-lifecycle.md): where that page describes how a
maintainer *receives, reviews, and ships* a change, this one describes how to *write and present*
one so the first review is **verification, not discovery.**

The [coding principles](coding-principles.md) cover the craft of writing correct code. This page
adds the two things that most often force a redo round even when the code is fine: **the dimension
you didn't consider**, and **the evidence you didn't show.**

If a project takes a high volume of agent-authored changes, link this from the repo's own
`CONTRIBUTING` / agent-entry file — the habits below are the ones agents get wrong most often and
most confidently, regardless of language or stack.

---

## Why changes get redone

Nearly every change that fails review fails for one of a small set of reasons. The
[bug-shape catalog](bug-shapes.md) names the ones that survive tests and CI; two more are
**authoring** failures the author can catch before opening anything:

1. **A dimension went unconsidered.** The fix is correct for the case in front of you and wrong for
   a sibling case — a second backend, an unusual layout, the cancel path, the empty or many-item
   case. The missed one is almost always a *different axis* than the one you were looking at.
2. **The evidence stayed in your head.** You did consider the siblings, you did confirm the test
   bites — but none of it is in the change description, so the reviewer has to rediscover it from
   scratch (or assume you didn't).

The rest of this page is the discipline that closes those two gaps. The correctness rules it leans
on already live in this repo; it points at them rather than restating them.

---

## Enumerate the state-space before you edit

This is the single most valuable authoring habit, and the one most often skipped. **Before
writing the fix, write down the dimensions the change touches** — then cover each, or mark it out of
scope on purpose. Most redo rounds are one un-enumerated dimension.

A generic checklist of axes (keep the ones that apply, add domain-specific ones):

- **Entry points** — every route, command, or caller that reaches the changed code.
- **Backends / implementations** — if the behavior has more than one implementation (a second
  storage engine, a fallback provider, an alternate platform), does the fix hold in each?
- **Cardinality** — zero items, one, many, and duplicates. Selection bugs hide in the "many" case.
- **Lifecycle exits** — success, error, cancellation, replacement, shrink/clear, teardown. A change
  that only handles the happy path is a change that leaks on the other five.
- **Trust / auth state** — authenticated vs. not, privileged vs. not, on vs. off.
- **Concurrency** — two workers, sessions, or profiles acting at once on the same state.
- **Input shape** — empty, hostile, aliased, oversized, wrong-encoding.

You do not have to handle every axis — you have to have *considered* every axis. "Out of scope: the
second backend, tracked in #NNN" is a complete answer. Silence is not.

This is the forward-looking form of **fixing the class, not the instance**
([quality gates](../lifecycle/quality-gates.md#the-pre-fix-discipline-before-any-code-changes),
[bug-shape #8](bug-shapes.md#8-the-instance-fix-that-left-the-class-alone)): the state-space is
where the class's other members live.

---

## Trace one authoritative value end-to-end

When a change turns on a value — an id, a path, a normalized name, a resolved setting — follow that
value through its whole pipeline:

```
input → normalize → decide → act → persist → clean up
```

…and use the **same resolved value at every stage.** The classic silent bug is a split: the code
that *decides* reads one form of the value (raw, un-normalized, or freshly re-fetched) while the
code that *acts* uses another (stale, cached, or differently-normalized). Both halves look correct
in isolation; together they act on the wrong thing.

This is the lifetime-not-just-content lens: getting a value *right* is not enough if you haven't
accounted for *when it goes stale, who owns it, and which stage sees which copy.* It pairs with
[**one state, one owner**](coding-principles.md#one-state-one-owner) — a value with two writers is a
value that will drift.

---

## Prove every resource is released on every exit

For each resource or mutation the change introduces — a cache entry, a lock, a temporary
environment change, a loading flag, a spawned handle, a pending-request entry — name its owner and
show it is cleaned up or invalidated on **every** exit path, not just success: error, cancellation,
replacement, shrink/clear, and teardown.

Happy-path testing structurally cannot see these leaks, because the leak is on the path the happy
test never takes. Walk the exits deliberately. This is
[one state, one owner](coding-principles.md#one-state-one-owner) extended from *correctness* to *lifetime*.

---

## Validate at the point of use, and scope by complete identity

Two adversarial-input habits that a naive check misses — the authoring-time edge of the
trust-boundary discipline the [security spine](security-spine.md) governs:

- **Check-then-use gaps are exploitable.** State can change between the moment you validate it and
  the moment you use it. Where the platform allows, validate the *thing you will actually use* — hold
  an open handle rather than re-resolving a name/path a second time. Validation is worthless if the
  thing you finally act on isn't the thing you validated.
- **Cache and key by *complete* identity.** A cache, marker, or dedupe key scoped by a partial
  identity leaks across the dimensions you left out of the key — profiles, sessions, tenants — under
  concurrency. Include every axis that distinguishes one caller's data from another's.

Treat any value that crosses a trust boundary as potentially crafted (odd delimiters, casing,
config aliases, path traversal). This is the input-side of **fail closed**: when you cannot
*confirm* something is safe, deny it, and never report a failure as a success
([coding principles #9](coding-principles.md#9-fail-loud--distinguish-verified-from-assumed)).

---

## Place a visible control by attention, not by code-proximity

For a change that adds a visible control, where it lives should be decided by how often it's used
and by where comparable apps in the same category put the equivalent — not by where the diff already
happens to be. A rarely-used per-item action belongs in an overflow/`⋮` menu; a global or data
action belongs in settings; only a genuinely frequent control earns a spot on a hot, always-visible
surface. Placing a control next to the code that produces it is the most common way a low-value
affordance ends up taxing attention on every future visit.

Then verify it *visually* — this half has a home in the
[visual-verification gate](../lifecycle/quality-gates.md#the-gates-in-order): capture before/after at the real viewports
(wide and narrow), and confirm nothing is clipped, no overflow-collapse is tripped, and no
hover-only affordance is stranded on touch.

---

## Already covered by the craft principles

These authoring habits are correctness rules that already have a home in this repo — apply them, no
need to relearn them here:

| Habit | Where it lives |
|---|---|
| A test must fail before the fix and pass after (no vacuous tests) | [quality gates](../lifecycle/quality-gates.md#the-pre-fix-discipline-before-any-code-changes) · [bug-shape #6](bug-shapes.md#6-the-vacuous-test) |
| Assert observable behavior, not a source string or a mock of the thing under test | [coding principles #5](coding-principles.md#5-test-isolation-and-tests-that-assert-intent) |
| Extend the existing mechanism (fallback, helper, extension point) — don't copy a parallel block | [the minimalism ladder](coding-principles.md#the-minimalism-ladder) |
| The diff is the task and nothing else; run neighboring tests, not just yours | [coding principles #2](coding-principles.md#2-surgical-edits-only) · [anti-patterns](anti-patterns.md#code-craft-anti-patterns) |
| Inspect any visible change visually at real viewports — tests say nothing about how it looks | [quality gates #4](../lifecycle/quality-gates.md#the-gates-in-order) |
| Match the codebase's conventions over personal taste | [coding principles #10](coding-principles.md#10-match-the-codebases-conventions-over-personal-taste) |

---

## Show your work in the change description

The fastest review is one where the reviewer can *see* that the above is handled instead of having
to discover the gaps. Make the change description carry the evidence — this is the author-side of
the system's **"a verdict with evidence, never a vibe"** rule, moved earlier so the first review
starts from proof:

- **The siblings you found** (the state-space axes you covered) — and any you deliberately left out
  of scope, named.
- **Proof the test bites** — that the new test failed against the unfixed code, for the right reason.
- **The verification run** — the affected tests *and* a sweep of neighboring ones, not only the ones
  you added.
- **Before/after evidence for any visible change** — at the real viewports, wide and narrow.
- **What you could not verify** — an explicit list. An admitted gap is good engineering; a hidden
  one is a bug waiting to be found in review, or after it.

Following this imperfectly still helps enormously: the reviewer can see exactly where your coverage
stops, instead of finding it the hard way. An honest *"I could not verify X"* moves the change
forward; a silent gap is how a regression ships.

---

_Related: [coding principles](coding-principles.md) · [quality gates](../lifecycle/quality-gates.md)
· [the bug-shape catalog](bug-shapes.md) · [PR lifecycle](../lifecycle/pr-lifecycle.md) (the
maintainer-side counterpart to this page)._

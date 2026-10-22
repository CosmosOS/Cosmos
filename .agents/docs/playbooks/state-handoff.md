---
title: The state handoff
layout: default
parent: Playbooks
nav_order: 7
---

# The state handoff

One role finishes a piece of work and another has to pick it up: the Watcher captures a report the
Reviewer will later judge, the Reviewer produces a verdict the Builder acts on, the Builder ships a
change the Steward announces and the watchdog fact-checks. Even a single agent wearing all four hats
hands off **to its own next run** — the session that ends is not the session that resumes. This
playbook is about the seam between those steps: how state left by one actor is found, trusted, and
acted on by the next.

It is the sibling of [the output loop](output-loop.md). That playbook covers handing state to a
**human**; this one covers handing state to **another agent, another role, or a future run of
yourself.** The two share a spine — durable, discoverable, honest state — and differ only in who the
consumer is.

> **This is a documented default, not a mandate.** The framework prescribes the *contract* below —
> the properties a handoff must have. It does **not** prescribe a mechanism. Markdown files are the
> recommended baseline because they travel everywhere and stay legible; several alternatives are
> listed, and the right one depends on your volume, concurrency, and runtime.

> You do not have to hand off at all. [One agent can wear all four roles](../architecture/index.md#the-four-roles)
> in a single session and pass nothing between processes. But the moment work crosses a process, a
> schedule, or a context window — and for any long task it eventually does — the contract applies,
> because in-context memory does not survive the boundary.

---

## The contract (mechanism-neutral)

Whatever you store handoff state *in*, it has to hold these five properties. They are the portable
part — true for a markdown file, a database row, or a runtime's memory store alike.

1. **Durable, not in-context.** A handoff must survive process death and context loss. State that
   lives only in one agent's conversation is gone the moment that session ends — which is exactly
   when the next actor needs it. Write it somewhere outside the context window before you rely on it.

2. **Discoverable at a known location — pull, don't push.** The consumer must be able to *find* the
   handoff without being told it exists: a known path, a known table, a known queue that the next
   actor always checks. This is [discovery over delivery](output-loop.md#1-discovery-over-delivery)
   applied between agents — a pushed notification is a convenience on top, never the only path.

3. **Never authoritative — re-derive from live truth.** Stored handoff state is a *pointer to what to
   check*, not proof of what is true. This is the [three verification axes](resilience-and-self-healing.md#verify-every-class-of-stored-state-the-three-axes)
   rule: the consumer reads the handoff to know *where to look*, then confirms against live ground
   truth (the repo, the tracker, the running system) before acting. A handoff note that cites code
   (`the guard lives at X`) rots into active misinformation the moment the code moves — treat those
   as the freshness axis, not durable fact.

4. **Append-only where it is a record; reconciled where it is a queue.** A handoff that exists to be
   *audited* — the action ledger a [watchdog re-verifies](watchdog-pattern.md#the-rules-that-make-it-trustworthy)
   — is append-only: you add, you never rewrite history. A handoff that exists as a *working queue*
   is mutable, but must be [reconciled to live truth in both directions](output-loop.md#3-keep-the-state-honest)
   — drop what reached a terminal state, add any live item it never recorded — or it drifts out of
   trust. Know which kind you are writing.

5. **Self-describing and identity-complete.** Any reader — a human, or a differently-built agent —
   should be able to parse a record without private knowledge: what it is, what it refers to, what
   state it is in. And key it by *complete* identity (which repo, which item, which profile/session)
   so two actors working at once don't collide or overwrite each other. Partial keys leak across the
   dimension you left out; see [one state, one owner](../reference/coding-principles.md#one-state-one-owner)
   and, under concurrency, guard shared writes per [coordination & durability](scheduled-jobs.md#coordination--durability).

## The shape

```
 [ROLE A] ──produces──▶ handoff record ──appended/updated──▶ [KNOWN LOCATION]
   done                  (self-describing,                         │
                          identity-keyed)                    [ROLE B] checks it (pull)
                                                                   │
                                              reads record ── says WHAT to check
                                                                   │
                                              confirms against ── LIVE TRUTH
                                                                   │
                                                     acts, then reconciles the record
```

The record never *is* the truth; it is the map to the truth. Role B always closes the loop against
the live system before it acts.

**Execution clearances need protected evidence.** A human-run gate receipt follows the
[sandbox rule](../reference/security-spine.md#4-the-sandbox-untrusted-code-execution): keep original
trusted evidence in an operator/reviewer-owned location outside the contributor-writable tree or
PR thread. A generic handoff may point there, but cannot authorize execution/merge by claiming a
runner name or matching commit. The consumer verifies origin, current head/trunk, fresh completion,
required scopes/results and independent review evidence; missing or unverifiable evidence stays
blocked. This stronger requirement is for security/execution clearance, not a ban on ordinary
handoff notes in the mechanisms below.

---

## The recommended default: append-only markdown at a known path

If you have no reason to choose otherwise, hand off through **markdown files in a known directory,
one record per unit of work, appended or status-flipped in place, committed to the repo.** It is the
baseline because it satisfies the whole contract with the least machinery:

- **Runtime-agnostic** — every agent framework and every human can read and write a text file; it
  couples you to nothing.
- **Human-legible** — the maintainer can read the handoff directly, with no tool, and see exactly
  what one agent left for the next.
- **Diffable and versioned** — committed to git, every handoff has an author, a timestamp, and a
  history for free; a stale or wrong record is visible in the diff.
- **Degrades gracefully** — a half-written file is still readable; there is no schema migration to
  break.

A workable record, mirroring the [output-loop index line](output-loop.md#what-this-can-look-like):

```
- [ ] 2026-06-24 · reviewer→builder · PR 482 · verdict: mergeable after rebase; conflict in one file
      → needs: rebase onto main, re-run suite · detail: state/handoffs/2026-06-24-pr-482.md
```

Single-writer by default; if two actors can append at once, take a file lock or write to
per-actor files and merge on read (see [coordination & durability](scheduled-jobs.md#coordination--durability)).

## Other mechanisms, and when to reach for them

The default is not always the fit. Pick by volume, concurrency, and what your runtime already gives
you — each of these can satisfy the same contract:

| Mechanism | Good when | Watch out for |
|---|---|---|
| **Markdown files at a known path** *(default)* | Most cases; low-to-moderate volume; you want human-legible, git-native handoffs | Concurrent writers need a lock or per-actor files; not built for high-frequency writes |
| **Structured store** (SQLite, JSON store, a table) | High volume, many concurrent writers, or you need to *query* the handoff state | Less human-legible; you now own a schema and its versioning |
| **The runtime's own memory / artifact store** | Your agent framework already provides durable, cross-session memory or an artifact bus | Couples the handoff to that runtime — keep the *contract* portable so you can move off it |
| **The VCS itself** (a branch, a commit, a PR/issue comment) | The work product *is* the handoff — Builder→Reviewer over a branch, a review verdict as a PR comment | Durable and audit-trailed for free, but coarse; not for high-frequency internal state |
| **A message queue / event bus** | Large multi-process fleets where actors run continuously and independently | Real operational overhead; overkill for a solo maintainer or a handful of crons |

Whatever you choose, the record stays the map and live truth stays the territory. A structured store
that a consumer trusts *instead of* re-checking the live system has broken property 3 no matter how
clean its schema is.

## Choosing, in one line

Start with markdown files; move to a structured store only when volume or concurrent writers make
flat files awkward; use the VCS when the artifact already carries the state; reach for a queue only
when you genuinely run a continuous multi-process fleet. Match the mechanism to the load, and keep
the five-property contract fixed across whichever you pick.

---

_Related: [the output loop](output-loop.md) (the human-facing sibling) · [the watchdog pattern](watchdog-pattern.md)
(the append-only ledger it re-verifies) · [resilience & self-healing](resilience-and-self-healing.md)
(the three verification axes) · [designing scheduled jobs](scheduled-jobs.md) (concurrency and lost-work durability)._

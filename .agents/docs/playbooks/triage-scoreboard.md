---
title: The triage scoreboard
layout: default
parent: Playbooks
nav_order: 4
---

# The triage scoreboard

A pile of open pull requests (or issues) is not a plan. The scoreboard turns it into a **ranked,
evidence-tagged review queue** so the maintainer always knows what to look at next and what state
each item is in. It's the Steward's central instrument.

---

## The idea
Compute, for every open item, a small set of **dimensions**, combine them into a single **composite
rank**, and render a living document sorted by that rank. Re-generate it on a schedule so it's always
current without anyone asking.

## Two kinds of dimension
- **Mechanical** (recomputed fresh every run, no judgment): CI status, mergeable/conflicting,
  how far behind the trunk, diff size, age, contributor trust score.
- **Judgment** (set by a reviewer/agent, persisted across runs): `scope_fit`, `criticality`, `risk`,
  scored on [the default rubric](#the-default-rubric). These survive regeneration so a human's
  assessment isn't lost when the mechanical data refreshes.

Keep the two separate in storage: mechanical data is disposable and re-derived; judgment data is
precious and persisted.

## The default rubric
The three judgment dimensions need a shared scale. Without one, two boards can't be compared and
the weights have nothing fixed to tune against. This is a default: replace it if your project
values something else, and keep four things when you do: named dimensions, a stated range, a
stated direction, and a written anchor for every level. Until your project writes a replacement
beside this playbook, the default applies and the board validates against its range.

Each dimension is an integer from 0 to 3. Higher always means *pick this up sooner*.

| Dimension | Measures | 0 | 1 | 2 | 3 |
|---|---|---|---|---|---|
| `scope_fit` | how directly the item serves a scope anchor | serves none | serves one in part | serves one plainly | the anchor can't be met without it |
| `criticality` | the harm the item fixes or reports | none, or already resolved | cosmetic, or an inconvenience | a shipped feature fails | data loss, or a core path fails |
| `risk` | the cost of leaving the item alone | nothing changes | the cost grows slowly | it worsens on a known trigger | it's worsening now, or another open change or issue you can point at is blocked on it |

What the table can't show:

- **Criticality is the harm; risk is whether the harm grows.** A broken export with no way around
  it is criticality 2. If nothing else breaks while it waits, it's risk 0.
- **A workaround lowers criticality by one level**, and never below 1.
- **A feature fixes no harm**, so its criticality is 0 and its rank comes from `scope_fit` and
  `risk`.
- **Score from what you found, not from what the item says.** The item's own text is a claim.
- **Risk is not the danger of merging.** That question picks the PR's lane and has no dimension
  here.
- **`scope_fit` 0 is a human's value:** an out-of-scope item a maintainer chose to keep open. An
  agent that finds no anchor takes the out-of-scope path and writes no 0. An item that could be
  argued in or out of scope is escalated, not graded.
- **A suspected vulnerability gets no judgment.** Run the
  [vulnerability divert](../reference/security-spine.md#6-the-vulnerability-divert) and write no
  score and no severity flag. The board is committed, and either would confirm in public what the
  divert keeps private. The board still lists the item like any other unjudged one, mechanical
  dimensions only, since a missing row would be its own signal. If the divert fires after a
  judgment was written, change nothing on the board: a retraction is a visible diff too. Note the
  judgment in the private record; whether to change it is the human's call, made with the rest of
  the private handling.
- **An agent writes only a dimension that is empty.** A dimension already on the board, set by a
  human or an earlier run, stands. An agent that disagrees says so in its output and leaves it
  alone. An empty dimension counts as unjudged for that dimension.

### Severity is a flag
Severity isn't a fourth scale. It's a flag stored with the judgment, and the refresh doesn't
recompute it.

- An agent sets the flag only when criticality is at the top level *and* the judgment cites
  evidence that doesn't come from the item's author: a line on the trunk, a failing check on the
  trunk, or a report from someone else whose claim the agent then confirmed on the trunk, by
  reading or with a repro it wrote itself under
  [the sandbox rule](../reference/security-spine.md#4-the-sandbox-untrusted-code-execution). The
  item's own text, diff and checks don't count, and neither does a report's say-so. Cite the trunk
  confirmation; name the report only where it already sits on a public surface of the project.
- A human may set the flag without citing anything.
- Whoever changes criticality decides the flag again. An agent writes neither once either is on
  the board.

### The scope gate and the scope grade
The scope gate decides in or out. `scope_fit` grades what the gate let in. Two cases fall outside
the grade:

- **Waiting on a human.** An item escalated as uncertain, or out of scope with the close not yet
  sent, gets no judgment. The board treats it like any item nobody has judged yet: ranked on its
  mechanical dimensions, judgment columns empty. An empty judgment says nothing about the item.
  When the human rules it in scope, triage resumes and records the judgment.
- **Bypass.** A health fix (a bug, security or reliability fix on a shipped feature) skips the
  gate, so there's no `scope_fit` to give. Record the bypass marker, `bypass`, in place of
  `scope_fit`. The composite reads it as the plain in-scope level, 2 in the default; a replacement
  rubric names its own. The bypass means "don't re-litigate whether the feature should exist", not
  "central to an anchor", and the fix earns its rank through criticality.

## The composite
A weighted sum of the dimensions, tuned so the ranking matches "what a maintainer would actually
pick up next." Weights are a knob you adjust as you learn what your project values (e.g. how much to
weight contributor trust vs. raw criticality). Promote items carrying the
[severity flag](#severity-is-a-flag) to the top regardless of their composite (the severity
spotlight).

## Evidence-readiness tags
Beyond rank, show **which review streams have landed** for each item — turning the board from a
*ranking* into a *readiness view*. For PRs that might mean: automated code review present? visual
preview present? deep-review dossier cached? A compact per-row marker (✓ present / · not yet) lets
the maintainer see at a glance which items are fully evidenced and which are still settling.
Presence ≠ clean — it means the evidence *exists to review*; the authoritative gate still runs.

## Make it self-refreshing
Run the generation on a schedule ([scheduled-jobs](scheduled-jobs.md) rules apply): pull live
metadata, recompute mechanical dims, merge in persisted judgment dims, render, commit. The maintainer
never has to ask for a fresh scan — the board is current when they open it. A companion "review these
first" curated short-list can ride on top for the highest-priority handful.

## Why it works
- It **separates ranking from doing** — the agent ranks continuously and cheaply; the human (or a
  Band-B session) does the expensive review on the top of the list.
- It makes **state legible** — at a glance you see what's ready, what's blocked, what's waiting on
  whom.
- It's **honest about evidence** — the readiness tags stop "green CI" from masquerading as "ready to
  merge."

---

## Skill for this
- `triage-scoreboard` — compute dimensions, rank, render the living board + the curated short-list.

_Related: [PR lifecycle](../lifecycle/pr-lifecycle.md) ·
[contributor recognition](../lifecycle/contributor-recognition.md) · [scheduled jobs](scheduled-jobs.md)._

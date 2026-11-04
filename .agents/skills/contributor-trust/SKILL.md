---
name: contributor-trust
description: Aggregate the contributor trust ledger into a shrunk-and-decayed reliability score that weights review priority and gate intensity — never a reason to skip a gate. Band A, no LLM: classification happens upstream at close, this is arithmetic over an existing event log.
---

# contributor-trust

**When to load:** computing the contributor reliability score that
[`triage-scoreboard`](../triage-scoreboard/SKILL.md) consumes as a mechanical dimension. Implements
the trust-ledger half of
[contributor recognition](../../docs/lifecycle/contributor-recognition.md). Band A —
deterministic, read-only, **no LLM**. Steward role. Loaded **on demand** during a scoreboard
refresh, not wired as its own scheduled job: the scoreboard recomputes mechanical dimensions fresh
each run, so an on-demand score is always current.

> Precondition: **a ledger path is configured.** Read `trust.ledger_path` from
> [`config.yaml`](../../setup/config.template.yaml); blank → there is no ledger to aggregate, stop
> (silent no-op).

> Config: the `trust:` block — `ledger_path`, `prior`, `shrinkage`, `half_life_days`. This skill
> **never writes events**: it reads the ledger and publishes the trust map.
> [`release-pipeline`](../release-pipeline/SKILL.md) writes an event at close.

## The ledger

An append-only record of **one event per PR's terminal outcome**. A bounce that later converges is
*one* good-rework event — not a negative followed by a positive. Intermediate bounces are not events
at all; only the terminal state is written.

A terminal outcome can still be **corrected** — a `clean-ship` later found to be a `regression` — by
*appending* a new event for the same `pr`, never by rewriting the old one. The log stays append-only;
the **last-appended event for a PR wins** at read time (see Step 2). "One event per PR" is therefore
an effective-state rule, not a promise that the raw log holds a single line per PR.

Each event: `pr` (repo#number) · `author` (handle) · `outcome` (below) · `at` (UTC date).

| Class | Outcomes | Scoring |
|---|---|---|
| **positive** | `clean-ship`, `shipped-with-minor-fixes`, `good-rework` | value `1` |
| **negative** | `not-fixed`, `went-stale`, `regression`, `critical-flaw` | value `0` |
| **unscored** | `neutral`, `superseded`, `shelved`, `wip`, `concept-call` | **excluded** — recorded, never judged |

## Steps

1. **Read `config.yaml`.** Load `trust.ledger_path`, `prior`, `shrinkage` (k), `half_life_days`.
   Blank `ledger_path` → silent no-op.
2. **Read the ledger, collapse to one event per `pr`, then group by `author`.** The **last-appended
   event for a PR wins** (last-write-wins over the append-only log), so a later correction supersedes
   an earlier entry and no PR is ever counted twice. Collapse by *append order*, not by `at` — `at` is
   the event's terminal date the decay in Step 4 needs, and a correction may legitimately carry a
   recent `at`. Malformed or unknown-outcome events are **surfaced, never guessed** — an event you
   can't classify is not an event you may score.
3. **Drop unscored events** from the computation. They stay in the ledger; they never move a score
   and never contribute to `effective_n`.
4. **Value and decay each scored event.** `v` = 1 (positive) or 0 (negative).
   `w = 0.5 ** (age_days / half_life_days)` — an event's weight halves every `half_life_days`.
5. **Shrink toward the prior:**

   ```
   score = (Σ wᵢ·vᵢ + k·prior) / (Σ wᵢ + k)
   ```

   Both mandated properties fall out of this one expression rather than being bolted on: a
   contributor with **zero** scored events scores **exactly** `prior` (the `Σw = 0` case reduces to
   `k·prior / k`), so the first-timer needs no special branch; and a contributor with one event
   barely leaves the prior.
6. **Publish the trust map**, keyed by handle:
   `{score, events_scored, effective_n, last_event_at}`. Publish `effective_n` (= `Σ wᵢ`, the decayed
   event count) alongside every score — it is what tells a consumer how much the score is *worth*. A
   `0.9` backed by an `effective_n` of `0.2` is almost entirely prior.
7. **Never write.** This skill does not append, edit, or reconcile the ledger. Same ledger + same
   clock → same map.

## Pitfalls

- **The score never gates.** It weights review *priority* and *gate intensity*. It is an input to
  triage, **never an excuse to skip the authoritative gate** — every change passes the gates
  regardless of who wrote it. A trust score that waves a change through has become the thing this
  system exists to prevent.
- **The trust map is internal triage state — don't render it publicly.** It carries negative signal
  and is an input to *your* review intensity, not a public leaderboard. Publishing per-contributor
  reliability scores would chill exactly the experimentation
  [contributor recognition](../../docs/lifecycle/contributor-recognition.md) exists to reward. Keep
  it beside the scoreboard as internal state, out of any rendered or public surface.
- **Today the score is upward-biased — say so, don't compute past it.** The only declared ledger
  writer is [`release-pipeline`](../release-pipeline/SKILL.md), which runs **only on the ship path**
  (it merges an *all-clear* PR). Nothing currently writes `went-stale`, `not-fixed`, `regression`, or
  `critical-flaw`. So the ledger accrues only positives **at every volume** — a high `score` is
  inflated whether `effective_n` is low or high, because `effective_n` measures how much *positive*
  evidence exists, not freedom from bias. A high score can mean "genuinely reliable" *or* "failures
  were never recorded," and **nothing in the ledger distinguishes them.** Treat every score as
  low-confidence until a
  [reconcile job](../../docs/playbooks/resilience-and-self-healing.md) records non-ship outcomes.
- **Don't over-judge one data point.** This is enforced by shrinkage, not by a special case. If you
  find yourself adding an `if events == 1` branch, the prior is doing the job already.
- **Unscored ≠ negative.** Excluding an event and penalizing it are different things. A superseded or
  shelved PR must not cost the author — that's how you teach contributors that experimenting is
  expensive.
- **One terminal event per PR (write side).** Write a bounce-then-converge as the single
  `good-rework` outcome it is — don't emit a negative and then a positive. Read-side last-write-wins
  (Step 2) is a backstop against accidental duplicates and the mechanism for deliberate corrections;
  it is not permission to append sloppily on the write side.
- **Guessing an outcome.** Classification is upstream judgment made at close. This skill classifies
  nothing — it aggregates. If an event's outcome is missing or unrecognized, surface it; never infer
  one from the PR.

## Verification

- Same ledger + same clock → the same trust map (deterministic; no model in the loop).
- A contributor with **zero** scored events scores **exactly** `prior`.
- A contributor with **one** recent positive event lands near `prior`, not at `1.0` (shrinkage holds:
  with `k=3.0`, `prior=0.5`, a single fresh positive gives `(1 + 1.5) / 4 = 0.625`).
- Unscored events move no score and add nothing to `effective_n`.
- A **corrected** outcome supersedes the earlier one: appending a `regression` event for a PR that
  already has a `clean-ship` scores that PR **once** — as the `regression` — because Step 2 keeps only
  the last-appended event per `pr`. Two events for one PR never double-count.
- Ageing an event several half-lives drives its weight toward zero (5 half-lives → `0.031`), so an
  all-stale ledger reverts to `prior`.
- The ledger is **unchanged** after a run (read-only).
- Blank `trust.ledger_path` → silent no-op.
- Every published `score` carries its `effective_n`.

_Related: [contributor recognition](../../docs/lifecycle/contributor-recognition.md) ·
[the triage scoreboard](../../docs/playbooks/triage-scoreboard.md) ·
[the reconcile pattern](../../docs/playbooks/resilience-and-self-healing.md)._

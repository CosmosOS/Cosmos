---
title: Designing scheduled jobs
layout: default
parent: Playbooks
nav_order: 3
---

# Designing scheduled jobs

Most of the autonomous system runs as scheduled jobs (crons). Good ones are quiet, cheap, and
trustworthy; bad ones spam you, drift, or fail silently. The patterns here are what separate the
two.

---

## Silent-on-no-op (the most important rule)
A scheduled job that runs every few hours should produce **nothing** when there's nothing to report.
- If it emits "nothing to do" every tick, you'll train yourself to ignore it — and miss the tick
  that mattered.
- Design the job so empty output = no notification. A job that's silent for a week and then speaks
  is one you'll actually read.

## Deterministic where possible (no LLM in the loop)
The most robust scheduled jobs are plain scripts with **no model**:
- no prompt-injection surface (nothing reads untrusted content into a model),
- no nondeterminism (same input → same output),
- no token cost, no latency.
Push every job toward this shape. Reserve the LLM for jobs that genuinely need judgment (summarize a
feed, draft a digest, reason about content) — and for those, the security spine's injection
guardrails are mandatory.

## Incremental, not full-rescan
A steady-state job should process only what's *new or changed*, not re-scan the entire back catalog
every tick. Mark what you've handled (a state file, a label, a ledger) and skip it next time. The
one-time backfill is a manual/initial run; the recurring job is cheap.

## Idempotent
Running the job twice changes nothing the first run didn't already do. This makes retries, overlaps,
and manual re-runs safe. Reconcile to a desired state rather than blindly appending.

## The heartbeat exception
One job type *should* always speak: a **liveness watchdog** that audits the whole fleet's health and
reports daily even when all-green. Here the **absence** of the green report is the alarm — if the
heartbeat goes quiet, something broke the heartbeat itself. Use this sparingly (one fleet-health
job), not for every cron.

## Coordination & durability
- **Concurrency:** if multiple jobs (or a job and a live session) can touch the same state, guard it
  with a lock so they don't collide. A simple file lock with a "someone else owns this item, skip
  it" exit is usually enough.
- **Orphaned work:** a job can do its work, commit locally, then die before pushing. Check for
  unpushed state at the top of the next run and flush it. Don't assume a local commit reached the
  remote.
- **Script location & secrets:** jobs that need a secret call a secret-isolating helper (the token
  never enters the job's context). Keep the helper scripts where the scheduler can resolve them.

## Failure surfacing
- A job that exits non-zero or times out should *alert*, not fail silently. Silent-on-no-op is for
  *success* with nothing to do — never for errors.
- Pair long autonomous jobs with a completion or error notification so a broken job can't hide.
- **Don't make push delivery the only path to the human.** If a job's output is delivered to a chat
  platform, treat that as an enhancement — a misconfigured or unsupported target can fail silently
  and leave the human blind. Keep a pull-based fallback so the output is always discoverable. See
  [the output loop](output-loop.md).

## A good scheduled job, summarized
> Deterministic if it can be · incremental · idempotent · silent when there's nothing to do · loud
> when it errors · guarded against collisions and lost work · and, if it writes to a public surface,
> paired with a [watchdog](watchdog-pattern.md).

---

_Related: [the watchdog pattern](watchdog-pattern.md) · [the autonomy ladder](autonomy-ladder.md) ·
[the output loop](output-loop.md)._

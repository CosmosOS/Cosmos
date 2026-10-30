---
title: The watchdog pattern
layout: default
parent: Playbooks
nav_order: 2
---

# The watchdog pattern

The pattern that makes autonomous action on public surfaces *safe*: an **independent process that
fact-checks what the autonomous actions did, against ground truth, and alarms on anything wrong.**

> A system that acts autonomously without a watchdog is trusting itself. A system with a watchdog is
> *verifiable*. The watchdog is what lets you sleep while the crons run.

---

## Why it's necessary
Autonomous actions self-report success ("labeled 86 PRs," "closed issue #N"). A self-report is not
proof — the action could have fired on a wrong signal, a stale base, or a misclassification. For
reversible low-stakes actions that's tolerable. For anything touching the **public** surface
(closing issues, labeling, posting), you want an *independent* check that the action was actually
correct. That independent check is the watchdog.

## The shape

```
   [autonomous action]  ──writes──▶  public surface
          │                              │
          │ appends to                   │
          ▼                              │
     [action ledger] ◀───reads──── [WATCHDOG] ───reads───▶ live ground truth
                                          │
                                          ▼
                              re-verify each action vs truth
                                          │
                         ┌────────────────┴───────────────┐
                      all correct                    mismatch found
                          │                                │
                       (silent)                   🔴 alarm to a human
```

## The rules that make it trustworthy
1. **Independent.** The watchdog re-derives correctness from live ground truth — it does *not* trust
   the action's own ledger as evidence of correctness, only as the list of *what to check*.
2. **It verifies; it does not act.** The watchdog never fixes anything itself. It surfaces problems
   with a diagnosis for a human. (An auto-fixing watchdog is just another unverified actor.)
3. **Deterministic where possible.** Like the actions it checks, the strongest watchdog is a no-LLM
   script comparing recorded actions to live state — no injection surface, no nondeterminism.
4. **Two severities.** 🔴 = a correctness failure (an action was *wrong* — act now). 🟡 = drift (a
   label is stale, a signal moved — reconcile soon). They route differently.
5. **Silent when all-clear; loud when not.** A watchdog that pages you every run trains you to
   ignore it. It should be silent on a clean check. *(Exception: a periodic "heartbeat" watchdog
   that always reports, so the **absence** of its green report is itself the alarm — use this for
   liveness, e.g. "is the whole fleet healthy?")*
6. **Extensible by construction.** Every time you add a new autonomous action, you add a check to
   the watchdog *in the same change*. The rule: nothing mutates the public surface without a
   corresponding fact-check.

> **Scope — this verifies *actions*, not all stored state.** The watchdog re-checks what autonomous
> actions did. Two sibling checks cover the other classes of stored state: the reconcile pattern
> keeps hand-maintained *ledgers* honest, and a freshness audit keeps *artifacts that cite code*
> (notes, handoffs, docs) true as the code moves. They verify against different ground truth, so
> don't stretch the watchdog to cover them — see
> [the three verification axes](resilience-and-self-healing.md#verify-every-class-of-stored-state-the-three-axes).

## What to check (examples)
- **Autonomous issue closes:** is the issue still closed? Is the cited PR really merged and does it
  really link the issue? Does the cited release tag actually contain the merge commit? Was it
  reopened with a complaint?
- **Autonomous labels:** does every applied label still match the source signal? Did the action
  override a human's label (it shouldn't have)?
- **Fleet liveness (heartbeat):** did every enabled scheduled job run on time, deliver, and not
  error?

## The payoff
With the watchdog in place, an action that was Band C (human-gated) can graduate to Band A
(autonomous) — *because the human's verification job has been replaced by an independent automated
one, with escalation to a human only on a real mismatch.* That's the whole trade: the watchdog buys
you autonomy you couldn't safely have otherwise.

---

_Related: [the autonomy ladder](autonomy-ladder.md) · [scheduled jobs](scheduled-jobs.md) ·
[architecture: the security spine](../architecture/index.md#the-security-spine)._

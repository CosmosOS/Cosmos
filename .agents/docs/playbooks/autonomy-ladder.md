---
title: The autonomy ladder
layout: default
parent: Playbooks
nav_order: 1
---

# The autonomy ladder

How a capability safely climbs from human-gated to fully autonomous. The bands (C → B → A) aren't
fixed labels — they're rungs, and this is the recipe for climbing one rung at a time without getting
hurt.

> The direction is always **C → B → A**: start with a human in the loop, earn trust, then remove the
> human *only once the action is provably mechanical or independently watched.*

---

## The rungs

**Band C (human-gated)** → the agent prepares, a human acts.
**Band B (session-autonomous)** → the agent acts during a supervised session, asking at real
decision points.
**Band A (autonomous)** → the agent acts unattended on a schedule/trigger.

## When a capability is allowed to climb

A capability may move up a rung only when it meets the bar for the higher rung:

| To reach | The action must be… |
|---|---|
| **Band B** | well-understood, with clear decision points where the agent knows to stop and ask |
| **Band A** | **reversible** AND **low-blast-radius** AND (**mechanical** OR **independently verified by a watchdog**) |

If an action is irreversible *and* touches a public surface *and* can't be made mechanical, it does
**not** belong in Band A — keep it at C, or keep a human on the final step in B.

## The promotion recipe

1. **Run it in the lower band first, and log every decision.** Before automating PR labeling, label
   by hand (or in a supervised session) and record what you'd have done. You're building an
   evidence base.
2. **Make it mechanical where you can.** The safest Band-A actions have *no LLM in the loop* — a
   deterministic script reading already-computed signals. No model means no prompt-injection
   surface and no nondeterminism. Push as much of the action into deterministic code as possible.
3. **Add the watchdog before you remove the human.** For anything touching a public surface, build
   the independent [fact-checker](watchdog-pattern.md) *first*, verify it catches a deliberately
   wrong action, *then* let the action run autonomously. The watchdog is the price of admission to
   Band A for public writes.
4. **Roll out gradually.** Don't flip a backfill of 100 items at once. Run a small mixed batch
   (some that should act, some that shouldn't), verify on the real surface, expand, then go full.
5. **Make it idempotent and silent-on-no-op.** A good Band-A job does nothing visible when there's
   nothing to do, and re-running it changes nothing. (See [scheduled jobs](scheduled-jobs.md).)
6. **Keep the kill switch.** Every autonomous action stays reversible and every cron stays
   pausable. If the watchdog alarms, you can stop the action and undo its effects.

## Worked example: autonomous PR labeling (C → A)
- **C:** maintainer labels PRs by hand.
- **B:** in a session, the agent proposes labels from computed signals; the human applies them.
- **A:** a deterministic, no-LLM script reconciles a tight allowlist of labels from already-computed
  data; it's add-only for human-meaningful labels (never overrides a human), idempotent, silent when
  nothing changes — and a watchdog re-verifies every label against the source signal. Rolled out in
  batches (6 → 20 → 40 → full), verified at each step.

That capability earned Band A because it became *mechanical*, *reversible*, *gradual*, and
*watched* — not because someone decided to trust it.

---

_Related: [the watchdog pattern](watchdog-pattern.md) · [scheduled jobs](scheduled-jobs.md) ·
[architecture: the bands](../architecture/index.md#the-autonomy-bands)._

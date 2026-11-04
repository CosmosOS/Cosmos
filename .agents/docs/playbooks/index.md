---
title: Playbooks
layout: default
nav_order: 5
has_children: true
---

# Cross-cutting playbooks

Patterns that span multiple lifecycle areas.

- **[The autonomy ladder](autonomy-ladder.md)** — how to promote an action C → B → A safely.
- **[The watchdog pattern](watchdog-pattern.md)** — independent fact-checking that makes autonomous
  public action safe.
- **[Designing scheduled jobs](scheduled-jobs.md)** — cron design: silent-on-no-op,
  deterministic-where-possible, the heartbeat pattern.
- **[The triage scoreboard](triage-scoreboard.md)** — turning a pile of open work into a ranked,
  evidence-tagged review queue.
- **[Resilience & self-healing](resilience-and-self-healing.md)** — keeping the maintenance system
  itself healthy: layered supervision (OS + app watchdog), the pre-restart guard, the reconcile
  pattern for hand-maintained ledgers, and the flake ledger.
- **[The output loop](output-loop.md)** — the bridge from "the steward produced a draft" to "the
  human found it, acted, and the state reflects it": discovery over delivery, audit trail vs.
  actionable output, and keeping local state honest.
- **[The state handoff](state-handoff.md)** — the agent-to-agent sibling of the output loop: how
  state left by one role (or one run) is found, trusted, and acted on by the next — the
  mechanism-neutral contract, a recommended markdown default, and other mechanisms with trade-offs.

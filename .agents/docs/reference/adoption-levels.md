---
title: Adoption levels
layout: default
parent: Reference
nav_order: 3
---

# Adoption levels

You don't install this system all at once. Start at the level that matches your trust and your time,
and climb. Each level is useful on its own; each builds on the one before.

---

## Level 0 — Read the model, change nothing
Just internalize the [architecture](../architecture/index.md): the four roles, the bands, the
security spine. Even with zero automation, the mental model improves how you triage by hand. Cost:
one read. Value: a clearer operating model.

## Level 1 — Manual playbooks (Band B, human drives)
Run the [setup interview](../../setup/setup-interview.md) and use the lifecycle playbooks as
checklists in supervised sessions. The agent helps review a PR, triage an issue, run a release —
but you're in the loop for everything. Cost: setup + your attention per session. Value: rigor and
consistency you didn't have before. **This is where most projects should start.**

## Level 2 — The scoreboard + digests (Band A reads, Band B acts)
Add the read-only autonomous layer: a [triage scoreboard](../playbooks/triage-scoreboard.md) that
ranks open work, a "what's waiting on us" digest, a chat [Watcher](../lifecycle/community.md) that
captures reports. Nothing acts on the world yet — these just *surface* what needs attention so your
Band-B sessions are sharper. Cost: a few scheduled jobs. Value: nothing falls through the cracks.

## Level 3 — Mechanical autonomous actions (Band A, watchdogged)
Promote the safe, mechanical actions to autonomous, each with a [watchdog](../playbooks/watchdog-pattern.md):
deterministic label-sync, the strict shipped-issue auto-close, templated release announcements.
Follow the [autonomy ladder](../playbooks/autonomy-ladder.md) — small batch, verify, expand, watch.
Cost: building the deterministic scripts + the watchdog. Value: the routine maintenance runs itself,
verifiably.

## Level 4 — Full pipeline
Deep PR review and builds run as Band-B sessions feeding a Band-A release pipeline; the contributor
trust ledger weights review; community monitoring and contributor recognition run continuously; a
fleet heartbeat watches the whole thing. The maintainer's job becomes *decisions and taste*, not
mechanics. Cost: the full build. Value: an agent co-maintainer.

---

## The climb is the point
Each level earns the next. Don't jump to Level 3 autonomy for an action you haven't run manually at
Level 1 — you won't have the evidence base or the watchdog to make it safe. The system is designed
to be *grown into*, and to stay safe at every height.

| Level | What's autonomous | What's human |
|---|---|---|
| 0 | nothing | everything |
| 1 | nothing | everything (agent-assisted) |
| 2 | reads, ranking, capture, digests | all actions |
| 3 | + mechanical reversible actions (watchdogged) | judgment, merges, public voice |
| 4 | + the routine pipeline | decisions, taste, the irreversible calls |

_Related: [autonomy ladder](../playbooks/autonomy-ladder.md) · [setup interview](../../setup/setup-interview.md)._

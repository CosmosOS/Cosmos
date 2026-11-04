# Documentation map

Everything in the system, organized by what you're trying to do. New here? The
[Quickstart](../quickstart.md) gets you running on your repo in ~15 minutes; the
[architecture overview](architecture/index.md) is the model it all rests on. Read whichever fits
where you are.

## Architecture — the operating model
The four ideas the whole system is built on.

- **[Overview](architecture/index.md)** — the four roles, the autonomy bands, the security spine,
  the guiding principles. Start here.
- [Security spine reference](reference/security-spine.md) — the concrete guardrail patterns.
- [Reporting a vulnerability](../SECURITY.md) — the private disclosure path; a suspected vulnerability
  never reaches the public tracker. The model behind it: [the vulnerability divert](reference/security-spine.md#6-the-vulnerability-divert).

## Lifecycle — end-to-end playbooks
How work travels through the project, one area at a time. Each is a self-contained playbook with the
decision states, the bands, and the skills for that area.

- **[Issue lifecycle](lifecycle/issue-lifecycle.md)** — from a vague chat report to a filed,
  triaged, resolved, closed-with-credit issue.
- **[Pull-request lifecycle](lifecycle/pr-lifecycle.md)** — capture → fit/scope screen → review →
  gate → merge → release → close.
- **[Quality gates](lifecycle/quality-gates.md)** — the layered defense: automated review,
  adversarial review, the living test suite, visual verification.
- **[Community](lifecycle/community.md)** — chat monitoring, mentions, announcements; the
  find/draft-safe vs public-write-gated line.
- **[Contributor recognition](lifecycle/contributor-recognition.md)** — the trust ledger, the
  funnel, credit and promotion.

## Playbooks — cross-cutting recipes
Patterns that span multiple areas.

- **[The autonomy ladder](playbooks/autonomy-ladder.md)** — how to promote an action C → B → A
  safely.
- **[The watchdog pattern](playbooks/watchdog-pattern.md)** — independent fact-checking that makes
  autonomous public action safe.
- **[Designing scheduled jobs](playbooks/scheduled-jobs.md)** — cron design: silent-on-no-op,
  deterministic-where-possible, the heartbeat pattern.
- **[The triage scoreboard](playbooks/triage-scoreboard.md)** — turning a pile of open work into a
  ranked, evidence-tagged review queue.
- **[Resilience & self-healing](playbooks/resilience-and-self-healing.md)** — layered supervision
  (OS + app watchdog), the pre-restart guard, the reconcile pattern, and the flake ledger.
- **[The output loop](playbooks/output-loop.md)** — getting steward output to the human and keeping
  local state honest: discovery over delivery, audit trail vs. actionable, post-action reconcile.
- **[The state handoff](playbooks/state-handoff.md)** — the agent-to-agent sibling of the output
  loop: how state left by one role or run is found, trusted, and acted on by the next.

## Setup — wire it to your project
- **[The setup interview](../setup/setup-interview.md)** — the documented question list the agent
  works through with you.
- [Config template](../setup/config.template.yaml) — what the interview produces.

## Reference
- [Steward on Steward](reference/steward-on-steward.md) - the config and the pull requests behind
  this repo, with an honest split of what's live vs. aspirational.
- [Glossary](reference/glossary.md) — the system's vocabulary.
- [FAQ](reference/faq.md) — short answers to the common adoption questions.
- [Adoption levels](reference/adoption-levels.md) — minimal to full; start small, climb.
- [Coding principles](reference/coding-principles.md) — the craft-level discipline for writing code.
- [Authoring a change that lands](reference/authoring-changes.md) — the contributor-side habits
  that get a change merged in one review round instead of several.
- [Reviewing at volume](reference/reviewing-at-volume.md) — maintainer-side field notes on what
  makes a change land, and the failure modes seen most often at high volume.
- [Bug-shape catalog](reference/bug-shapes.md) — recurring bug classes and how to catch each.
- [Anti-patterns](reference/anti-patterns.md) — the mistakes this system is designed to avoid.
- [Security spine](reference/security-spine.md) — guardrail patterns in detail.

## Skills
Loadable agent procedures live in [`skills/`](../skills/) and are linked from the playbook section
they belong to. Each skill is a focused, runnable procedure; the playbooks are the *why* and the
skills are the *how*.

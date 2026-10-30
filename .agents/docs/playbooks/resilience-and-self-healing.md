---
title: Resilience & self-healing
layout: default
parent: Playbooks
nav_order: 5
---

# Resilience & self-healing

The maintenance system itself is software that runs unattended — so it needs the same care you'd
give any production service: it should survive a reboot, restart when it crashes, notice when it's
silently wrong, and close the gaps in its own bookkeeping. This playbook is the set of patterns that
keep the *machine that maintains the project* healthy.

> Principle: **automate the recovery, not just the action.** A system that does work autonomously
> but can't recover from its own failures isn't autonomous — it's a liability with a delay.

---

## Layer your supervision (OS-level under app-level)

If your system runs a long-lived service (a web app, a gateway, a daemon), supervise it at **two**
layers — they catch different failures and neither alone is enough:

1. **OS-level supervision** (e.g. systemd, launchd, a process manager): handles *process died* and
   *machine rebooted*. It boot-starts the service, restarts it within seconds of a crash, tracks the
   real PID, and logs to the system journal. This is the floor — without it, a reboot or a crash
   takes you down until a human notices.
2. **Application-level health watchdog** (a scheduled job): handles what the OS *can't see* — a
   process that's alive but wedged (HTTP-hung, 500ing, deadlocked), or serving a stale version after
   a deploy that didn't restart. Crucially, it **alerts a human** (the OS restarts silently — you'd
   never know it flapped). It restarts *through* the OS supervisor (never fighting it).

The division: **OS = the machine, watchdog = the app + the alerting.** A common mistake is having
only one. Only the OS layer → you never learn about app-level wedges or version drift. Only the
watchdog → a reboot leaves you down until the next poll, and there's no instant crash-restart.

### The pre-restart guard (don't restart into a broken state)
An auto-restarting watchdog must refuse to restart when a restart would make things *worse*. If the
deployment is mid-change (a partial update, an in-progress operation), a blind restart can serve
broken or half-applied code. The guard: **if the deploy is in a known-unclean state, alert instead
of restarting** — a human is probably in the middle of something. Restart only from a clean state.

---

## The reconcile pattern (close the gaps in hand-maintained bookkeeping)

Any ledger a human (or agent) maintains *by hand* — a trust ledger, a release log, an outcome
tally — drifts when an event happens outside the normal flow (something merged while no one was
looking, a step got interrupted). The reconcile pattern is a scheduled job that **diffs the ledger
against ground truth and closes the gap**:

- Find the events that reached a terminal state but have no ledger entry.
- **Auto-record only the unambiguous, low-distortion cases** (ones where a wrong guess is harmless —
  e.g. an *unscored* outcome, or a case with one obvious classification).
- **Surface the judgment cases** with a suggested entry + the exact command to record it — never
  auto-guess a value that moves a score or makes a claim.
- Never mutate the source of truth (the live system); only the local ledger.

This keeps a hand-maintained record honest without pretending the agent can make every call. The
same shape works for "issues that should be closed," "PRs that went stale," "contributors whose
outcomes weren't logged" — detect the gap, auto-close the safe part, surface the rest.

---

## Verify every class of stored state (the three axes)

The reconcile pattern above keeps *ledgers* honest. It's one instance of a broader rule: **stored
state is never authoritative — only live source is — so every class of stored state needs a path
back to live truth.** An autonomous maintainer accumulates three distinct classes of stored state,
and each needs its own verification against a different ground truth:

| Axis | Verifies that… | Against | Pattern |
|------|----------------|---------|---------|
| **Action correctness** | autonomous actions did what they claimed (a close stuck; a label still matches its signal) | the live public surface | [the watchdog pattern](watchdog-pattern.md) |
| **Ledger honesty** | hand-maintained records and review queues match reality (nothing merged unlogged; no actioned item still shows pending) | the live system state | the reconcile pattern (above) + [the output loop](output-loop.md#3-keep-the-state-honest) |
| **Artifact freshness** | stored artifacts that *cite code* — handoff notes, design docs, "this lives at path X" memories — still match the code they reference | the live source at HEAD | *this section* |

The third axis is the easiest to miss, because a stale artifact keeps *working* — it just stops
being *true*. A note that says "the retry logic is in `client.go`, near the `doRequest` function" is
silently wrong the moment that file is rewritten or the function renamed. Unlike a stale ledger entry
(which is merely *absent* information), a stale artifact is **active misinformation** — worse than
nothing, because a reader (human or agent) trusts it and acts on a fiction. The watchdog checks
*actions*; the reconcile pattern checks *ledgers*; neither looks at artifacts-that-cite-code.

**The freshness-audit shape** — a documented default; adapt the storage to your project, the *shape*
is the contract, not any path:

- Iterate the artifacts that cite code (wherever you keep them).
- For each, extract the paths / symbols / line references it names.
- Re-read those against the current source at HEAD.
- Flag the mismatches — file gone, symbol renamed, references shifted past a threshold you pick.
- **Silent on all-clear**, like every good scheduled job; surface only the stale ones, each with its
  citation and what changed, for a human (or a follow-up fix job) to correct.

Like the watchdog, a freshness audit **verifies but does not fix** — a stale artifact needs the truth
re-derived deliberately, not an auto-edit that guesses.

### Detect on a schedule, not only on the way past

Every axis has a weak form and a strong form. The **weak form is on-demand**: re-verify a piece of
stored state only when a run happens to touch it (re-read a cited path before acting on the issue
that references it). That's necessary but insufficient — it only ever checks the state you stumble
across, and the rot you never revisit stays invisible. The **strong form is a scheduled, read-only
(Band A) detector** that proactively sweeps the whole class — every action in the ledger, every queue
item, every artifact — on a cadence, independent of whatever today's runs happened to touch. Do both:
verify-before-action for the item in front of you, and a scheduled sweep so nothing rots in a corner
no run has visited.

---

## Flakes self-heal too

A recurring flaky test is the test-suite's version of "alive but silently wrong." The mechanism that
keeps it from being forgotten — a **ledger that logs every flake and flags any test that recurs for
a root-fix** — is covered under [quality gates → flakes](../lifecycle/quality-gates.md#flakes-never-tolerate-one).
The resilience point is just that it belongs in the same family as the patterns above: detect, don't
tolerate, drive to zero.

---

## Self-test for any unattended subsystem
- If the process dies, does it come back **without a human**? (OS supervision)
- If it's alive but *wrong*, does someone **find out**? (health watchdog + alerting)
- If it auto-acts on a public surface, is each action **independently verified**? ([watchdog pattern](watchdog-pattern.md))
- Does its own bookkeeping **self-heal** when an event slips through? (reconcile)
- Do its stored artifacts that cite code **stay true** as the code moves? (freshness audit)
- Do recurring failures **get fixed, not re-run-away**? (flake ledger)

If you can't answer all six, the subsystem isn't truly autonomous yet — it's automated-until-it-isn't.

---

_Related: [the watchdog pattern](watchdog-pattern.md) · [scheduled jobs](scheduled-jobs.md) ·
[the autonomy ladder](autonomy-ladder.md) · [quality gates](../lifecycle/quality-gates.md#flakes-never-tolerate-one)._

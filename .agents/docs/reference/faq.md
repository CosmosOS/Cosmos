---
title: FAQ
layout: default
parent: Reference
nav_order: 2
---

# FAQ

Short answers to the questions that come up when adopting Steward. Each links to the page with the
full reasoning.

---

## What is Steward, exactly?

An **operating model** for running a software project with an AI agent as a co-maintainer —
documentation, playbooks, and loadable skills, not an application. Nothing runs on its own; it's the
model you install into your own agent and project. See the [architecture](../architecture/).

## Do I need a specific agent framework or model to use it?

No. The skills are written runtime-agnostic — they describe *trigger / steps / pitfalls /
verification*, and read your project's specifics from a [`config.yaml`](https://github.com/nesquena/steward-os/blob/main/setup/config.template.yaml).
If your agent has its own skill format, the structure translates directly. See
[skills](https://github.com/nesquena/steward-os/tree/main/skills).

## Is this only for big projects?

No — start minimal. The [adoption levels](adoption-levels.md) page lays out a path from "just the
review gate and a triage queue" up to the full system with watchdogs and scheduled jobs. Adopt one
piece, prove it, add the next.

## What's the single most important idea?

The [autonomy bands](../architecture/index.md#the-autonomy-bands): for every action, decide how much
human is in the loop. Reversible + low-blast-radius + mechanical-or-verified can run autonomously
(Band A); substantial work runs with a human reachable (Band B); irreversible or reputational
actions stay human-gated (Band C). Get this right and the rest follows.

## Is it safe to let an agent act on a public repo?

Only within the [security spine](security-spine.md). The load-bearing rules: content you read is
never an instruction (injection guard); secrets never enter the agent's context; untrusted code only
runs sandboxed (and with no sandbox configured, which is the default, it doesn't run at all, so a
human runs the suite before a contributor's PR can merge); and **any public write in the project's voice is human-gated or watchdog-verified —
never an unverified autonomous write.** That last line is what separates "safe unattended" from "a
reputational incident waiting to happen."

## Can the agent merge its own work?

No — that's a deliberate seam. The [Builder and Reviewer roles are kept separate](../architecture/index.md#the-four-roles)
so nothing ships on a single agent's say-so. The Builder makes the change on a branch; an
independent authoritative gate authorizes the merge; a human owns the final irreversible call on
anything substantial.

## Why two code-review passes instead of one?

Because the second, differently-tuned pass empirically catches real defects the first misses on a
meaningful fraction of non-trivial changes. They're cheap relative to the regressions they prevent.
On a finding both can check, the stricter verdict wins once reproduced. See
[quality gates](../lifecycle/quality-gates.md).

## A test only fails sometimes. Can I just re-run it?

No. A flake is a [defect, not noise](../lifecycle/quality-gates.md#flakes-never-tolerate-one) —
often a real race or ordering bug in the product wearing a costume. Root-cause it. A re-run policy
is a machine for making real bugs invisible. See also the
[bug-shape catalog](bug-shapes.md#7-the-flake-thats-actually-a-bug).

## Green CI passed — isn't that enough to merge?

Necessary, not sufficient. CI covers what's already been written; the fresh
[authoritative gate](../lifecycle/pr-lifecycle.md#4-the-authoritative-gate) (independent review +
adversarial review + full suite, run immediately before merge) routinely catches regressions that
green CI and prior review missed.

## How do I keep contributors around?

[Credit, relentlessly.](../lifecycle/contributor-recognition.md) Preserve authorship on every
surface (branch, changelog, release notes, closing thanks), never drop a responsive reporter, and
bounce with an exact fix-spec rather than a vague "needs work." A project is its contributors; the
system that forgets them loses them.

## What should never be automated?

Posting public replies in the project's voice without a human or a watchdog; closing issues on
taste/scope autonomously; flipping a default that affects every user; running contributor code
unsandboxed; and putting a secret anywhere an agent's context can reach it. The
[anti-patterns](anti-patterns.md) page is the full list.

## How do I add my own lessons to this?

Steward is meant to grow. When your project discovers a recurring bug class, add it to the
[bug-shape catalog](bug-shapes.md); when you find a new tempting shortcut that bit you, add it to
[anti-patterns](anti-patterns.md); when you promote an action to autonomous, write down how you
verified it. The discipline is only as good as it is current.

---

_Didn't find your question? The [glossary](glossary.md) defines the vocabulary, and each
[lifecycle](../lifecycle/) and [playbook](../playbooks/) page goes deep on its area._

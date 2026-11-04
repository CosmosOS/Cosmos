---
name: project-system-setup
description: Run the setup interview to wire Steward to a specific project. Use when first installing the system, or when the project's repos/platforms/CI change. Produces config.yaml.
---

# project-system-setup

**When to load:** first install of the system into a project, or when something structural changes
(new repo, new chat platform, new CI, new review tool).

**What it does:** works through the [setup interview](../../setup/setup-interview.md) conversationally
and produces the project's `config.yaml` from the
[template](../../setup/config.template.yaml).

## Steps

1. **Read** `setup/setup-interview.md` and `setup/config.template.yaml`. The interview is the
   question script; the template is the output shape.
2. **Interview the human, section by section.** Ask only what's needed; explain *why* each section
   matters (cite the playbook it feeds). Accept "skip / not yet" for any optional section.
   - Required: identity & repos, scope anchors, CI/tests, autonomy posture, secrets (if any public
     write).
   - Optional: chat/community, mentions, scheduled jobs.
3. **Confirm each answer** before writing it. Never guess — an unanswered question becomes a
   documented TODO in the output, not an assumption.
4. **Write `config.yaml`** from the template with the collected answers. Validate it parses.
5. **Emit a TODO list** of skipped sections and a **recommended starting job set** based on the
   autonomy posture (conservative default: scoreboard + digests at Level 2; nothing public-writing
   until a watchdog exists).
6. **Point the human to** [adoption levels](../../docs/reference/adoption-levels.md) so they pick a
   starting level and know how to climb.

## Pitfalls
- **Don't put secrets in config.** Record the *path* to the locked credentials file, never a token.
  If the human pastes a token, refuse it and explain the
  [secret-isolating helper](../../docs/reference/security-spine.md#2-secret-isolating-helper-scripts)
  pattern.
- **Don't over-automate at setup.** Default new actions to Band B/C; autonomy is *earned* via the
  [autonomy ladder](../../docs/playbooks/autonomy-ladder.md), not granted at install.
- **Scope anchors are the highest-value answer** — if the human is vague here, push for specifics,
  because the whole fit screen depends on them.

## Verification
- `config.yaml` exists and parses.
- Every required section is filled or explicitly marked TODO.
- The human knows their starting adoption level and the next rung.

# Skills

Loadable, runnable procedures — the *how* that pairs with the playbooks' *why*. Each skill is a
self-contained `SKILL.md` an agent loads when it enters that phase. They're written generically:
they read your project's [`config.yaml`](../setup/config.template.yaml) for the specifics.

| Skill | Role | Pairs with | Band |
|---|---|---|---|
| [`project-system-setup`](project-system-setup/SKILL.md) | Steward | [setup interview](../setup/setup-interview.md) | B |
| [`coding-principles`](coding-principles/SKILL.md) | Builder | [coding principles](../docs/reference/coding-principles.md) | B |
| [`pr-triage`](pr-triage/SKILL.md) | Steward/Reviewer | [PR lifecycle](../docs/lifecycle/pr-lifecycle.md) [0]–[2] | B |
| [`pr-deep-review`](pr-deep-review/SKILL.md) | Reviewer/Builder | [PR lifecycle](../docs/lifecycle/pr-lifecycle.md) [3]–[4] + [quality gates](../docs/lifecycle/quality-gates.md) | B |
| [`release-pipeline`](release-pipeline/SKILL.md) | Builder/Steward | [PR lifecycle](../docs/lifecycle/pr-lifecycle.md) [5]–[6] + [contributor recognition](../docs/lifecycle/contributor-recognition.md) | B |
| [`release-announce`](release-announce/SKILL.md) | Steward | [community lifecycle](../docs/lifecycle/community.md) announcements + [watchdog](../docs/playbooks/watchdog-pattern.md) | A |
| [`issue-capture`](issue-capture/SKILL.md) | Watcher | [issue lifecycle](../docs/lifecycle/issue-lifecycle.md) capture stage | A |
| [`issue-triage`](issue-triage/SKILL.md) | Steward | [issue lifecycle](../docs/lifecycle/issue-lifecycle.md) | A/B |
| [`label-sync`](label-sync/SKILL.md) | Steward | [issue lifecycle](../docs/lifecycle/issue-lifecycle.md) label step + [watchdog](../docs/playbooks/watchdog-pattern.md) | A |
| [`issue-autoclose`](issue-autoclose/SKILL.md) | Steward | [issue lifecycle](../docs/lifecycle/issue-lifecycle.md) close-with-credit + [watchdog](../docs/playbooks/watchdog-pattern.md) | A |
| [`issue-close-proposal`](issue-close-proposal/SKILL.md) | Steward | [issue lifecycle](../docs/lifecycle/issue-lifecycle.md) close-with-credit + [output loop](../docs/playbooks/output-loop.md) decision row | C |
| [`triage-scoreboard`](triage-scoreboard/SKILL.md) | Steward | [scoreboard playbook](../docs/playbooks/triage-scoreboard.md) | A |
| [`contributor-trust`](contributor-trust/SKILL.md) | Steward | [contributor recognition](../docs/lifecycle/contributor-recognition.md) trust ledger + [scoreboard playbook](../docs/playbooks/triage-scoreboard.md) | A |
| [`action-watchdog`](action-watchdog/SKILL.md) | Steward | [watchdog pattern](../docs/playbooks/watchdog-pattern.md) | A |

## How skills are written here
- **Generic, not project-specific.** A skill says "read the test command from config and run it,"
  not a hardcoded command. Your `config.yaml` supplies the specifics.
- **Trigger + steps + pitfalls + verification.** Each skill states when to load it, numbered steps,
  the traps to avoid, and how to confirm it worked.
- **Linked both ways.** The skill points back to its playbook for the reasoning; the playbook points
  to the skill for the procedure.

## Adapting a skill to your agent runtime
These `SKILL.md` files are runtime-agnostic. If your agent framework has its own skill format, the
body translates directly — the structure (trigger / steps / pitfalls / verification) is what
matters, not the file convention.

# Steward for Cosmos

This folder vendors [Steward](https://nesquena.github.io/steward-os/), an operating model for running a
project with an AI agent as a co-maintainer: issue triage, pull request review, quality gates, and the
safety rules that keep a human at every irreversible public write. Upstream is
[nesquena/steward-os](https://github.com/nesquena/steward-os) (MIT, see `LICENSE`), pinned at
`dec2ea4381fadf8767c473a247531e97371b603f` (2026-10-07).

The rule everything rests on: reading is cheap and safe; writing to a public surface in the project's
voice (a comment, a label, a close, a merge) is the act that needs a human decision.

## Layout

- `skills/` - the 14 `SKILL.md` procedures (`pr-triage`, `pr-deep-review`, `issue-triage`,
  `triage-scoreboard`, `release-pipeline`, ...). Agents that read `.agents/skills/` discover them as
  project skills; in Hermes, run `hermes skills trust` once in the repository.
- `docs/` - the playbooks the skills link to: architecture, lifecycle, playbooks, reference.
- `setup/config.yaml` - Cosmos's answers to the setup interview: repositories, scope anchors,
  philosophy veto, CI, autonomy bands. Every skill reads it. The top-level `setup/` folder of the
  repository is the Gen2 installer and is unrelated.
- `setup/setup-interview.md`, `setup/config.template.yaml` - the interview and the config shape; re-run
  the interview when the project's structure changes (a new repository, a chat integration, a review
  bot).
- `bin/config-lint`, `bin/skills-contract-lint` - upstream lints for the config and the skills (Ruby,
  standard library only).
- `bin/triage-scoreboard` - Cosmos's implementation of the `scoreboard-refresh` job (not upstream).

## Adoption level

Level 2 of the [adoption levels](docs/reference/adoption-levels.md): the playbooks run in supervised
sessions, where the agent proposes and a maintainer approves every public write, and the read-only
scoreboard runs on a schedule. Nothing comments, labels, closes or merges on its own. The bands per
action are under `autonomy:` in the config; climbing follows the
[autonomy ladder](docs/playbooks/autonomy-ladder.md).

## Using it

- Screen a pull request: load `pr-triage` and ask for the fit screen of PR #N; `pr-deep-review` runs
  the deep review and the quality gate.
- Triage an issue: load `issue-triage` and ask for a triage with a drafted (not posted) reply.
- Rank open work: `ruby .agents/bin/triage-scoreboard --print`. It needs Ruby and an authenticated
  `gh`, reads every repository listed in the config, and never writes to GitHub.

The scoreboard writes into the directory of `scheduled_jobs.outputs.index_path`, which is private and
outside the repository (the board can hold judgments the public tracker must not see):

- `index.md` - the ranked board and the "review these first" short-list.
- `board.json` - the same data for other jobs.
- `judgments.json` - the judgment dimensions, recorded by a maintainer or a supervised session on the
  [default rubric](docs/playbooks/triage-scoreboard.md#the-default-rubric); the script creates it empty
  and never writes to it again. A value outside the rubric is reported on the board, never clamped.
- `dossiers/<owner>__<name>__<number>.md` - deep-review dossiers; their presence is the `D` evidence tag.

```json
{
  "CosmosOS/Cosmos#3331": { "scope_fit": "bypass", "criticality": 2, "risk": 1, "severity": false }
}
```

A scheduler should run a reviewed copy of the script and of the config, never the working tree: a
checked-out contributor branch could change both.

## Validating

```bash
ruby .agents/bin/config-lint .agents/setup/config.yaml
ruby .agents/bin/skills-contract-lint
```

## Updating from upstream

Copy `skills/`, `docs/`, `quickstart.md`, `setup/setup-interview.md`, `setup/config.template.yaml`,
`bin/config-lint`, `bin/skills-contract-lint`, `lib/` and `_data/skills.yml` from a newer steward-os
commit, update the pin above and run both lints. `setup/config.yaml`, `bin/triage-scoreboard` and this
README belong to Cosmos. Differences from upstream: the site files (Jekyll config, theme, tests) are
not copied, and `bin/anchor-lint` with `lib/anchor_contract.rb` are dropped, since they check
Steward's own site and need the kramdown gem.

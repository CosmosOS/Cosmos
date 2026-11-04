# frozen_string_literal: true

require 'yaml'
require 'set'

# Enforces the skills-vs-docs contract: every skill the playbooks and the config
# template name must have a runnable skills/<name>/SKILL.md — or be declared as
# not-yet-built. See _data/skills.yml (policy).
#
# Split into a pure checker (SkillsContract.check — sets in, violations out) and
# an Extract module that reads the repo. The checker has no IO so it is trivially
# unit-testable; the extractors are individually testable against real files.
module SkillsContract
  Violation = Struct.new(:severity, :skill, :message, keyword_init: true) do
    SYMBOL = { error: 'x', warning: '!', info: '·' }.freeze

    def to_line
      "  #{SYMBOL.fetch(severity)} #{skill} — #{message}"
    end
  end

  # Pure. Given the resolved sets, return the list of violations.
  #
  # present           — skills that have a skills/<name>/SKILL.md
  # referenced_strict — skills named by a lifecycle "Skills for this area" section
  #                     or a scheduled job (drives missing-skill errors; low false-positive)
  # indexed           — skills linked from the skills/README.md table
  # planned           — declared not-yet-built skills (keeps the repo green today)
  # standalone        — present skills intentionally not wired to any lifecycle/job
  def self.check(present:, referenced_strict:, indexed:, planned: [], standalone: [])
    present = present.to_set
    referenced_strict = referenced_strict.to_set
    indexed = indexed.to_set
    planned = planned.to_set
    standalone = standalone.to_set

    violations = []

    # Referenced or wired, but no SKILL.md — the core contract. Planned skills
    # are reported as info (expected); anything else is an error.
    (referenced_strict - present).sort.each do |s|
      if planned.include?(s)
        violations << Violation.new(severity: :info, skill: s, message: 'planned (not yet built)')
      else
        violations << Violation.new(severity: :error, skill: s,
                                    message: "referenced/wired but no skills/#{s}/SKILL.md exists")
      end
    end

    # Planned list must only shrink: a planned skill that now exists is stale.
    (planned & present).sort.each do |s|
      violations << Violation.new(severity: :error, skill: s,
                                  message: "listed under planned: but skills/#{s}/SKILL.md now exists — remove it from _data/skills.yml")
    end

    # Index drift, both directions.
    (present - indexed).sort.each do |s|
      violations << Violation.new(severity: :error, skill: s,
                                  message: 'has a SKILL.md but is missing from the skills/README.md index')
    end
    (indexed - present).sort.each do |s|
      violations << Violation.new(severity: :error, skill: s,
                                  message: "linked in skills/README.md but skills/#{s}/SKILL.md does not exist")
    end

    # Orphan: a real skill no lifecycle section or job points at, not declared standalone.
    (present - referenced_strict - standalone).sort.each do |s|
      violations << Violation.new(severity: :warning, skill: s,
                                  message: "orphan — no 'Skills for this area' section or scheduled job references it (add to standalone: in _data/skills.yml if intentional)")
    end

    violations
  end

  # Repo readers. Each is small and testable against real files.
  module Extract
    SKILL_TOKEN = /\A[a-z][a-z0-9-]*\z/.freeze

    module_function

    # Skill dirs that actually have a SKILL.md.
    def present_skills(skills_dir)
      Dir.glob(File.join(skills_dir, '*', 'SKILL.md'))
         .map { |p| File.basename(File.dirname(p)) }
         .uniq.sort
    end

    # Skills linked from the README, taking the last path segment before /SKILL.md
    # so bare (name/SKILL.md), (skills/name/SKILL.md), and (./name/SKILL.md) all work.
    def indexed_skills(readme_path)
      File.read(readme_path)
          .scan(%r{([a-z0-9][a-z0-9-]*)/SKILL\.md})
          .flatten.uniq.sort
    end

    # The skill a "Skills for this area" bullet declares, or nil. The grammar is
    # `- `skill-name` — description`, so only the LEADING backtick is the skill.
    # Inline backticks later in the bullet (config keys, prose terms, parentheticals)
    # are deliberately ignored — treating them as skills causes false lint failures.
    def leading_skill(line)
      return nil unless line =~ /\A\s*-\s+`([^`]+)`/

      tok = Regexp.last_match(1)
      tok =~ SKILL_TOKEN ? tok : nil
    end

    # Skills declared inside every "## Skills for this (area|lifecycle)" section.
    def doc_section_skills(doc_paths)
      names = []
      doc_paths.each do |path|
        in_section = false
        File.foreach(path) do |line|
          if line =~ /\A##\s+Skills for this\b/
            in_section = true
            next
          end
          in_section = false if in_section && line =~ /\A(##\s|---|_Related)/
          next unless in_section

          name = leading_skill(line)
          names << name if name
        end
      end
      names.uniq
    end

    # scheduled_jobs.jobs[].name from the config template. Raises a clear error
    # (rather than a bare stacktrace) if the YAML is malformed or uses anchors.
    def job_names(config_path)
      cfg = YAML.safe_load(File.read(config_path)) || {}
      (cfg.dig('scheduled_jobs', 'jobs') || []).filter_map { |j| j['name'] }
    rescue Psych::Exception => e
      raise "could not parse #{config_path}: #{e.message}"
    end

    # The strict referenced set that drives missing-skill errors: lifecycle
    # sections ∪ job names, with job aliases resolved to their skill target.
    def referenced_strict(doc_paths:, config_path:, job_aliases: {})
      names = doc_section_skills(doc_paths).dup
      job_names(config_path).each do |name|
        names << (job_aliases[name] || name)
      end
      names.uniq.sort
    end
  end
end
